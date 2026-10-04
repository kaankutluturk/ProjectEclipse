using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eclipse.Modding;

public partial class Fight
{
    private sealed class OwnedProjectile : IModProjectile
    {
        public Fight Fight;
        public Model Root, Model;
        public ModId Owner;
        public ModScriptSession Session;
        public int Round, Born, Lifetime;
        public long Sequence;
        public bool Removing;
        public PendingProjectileSpawn Birth;
        public int Requests;
        public double X, Y, Z;
        public bool TrySnapshot(out ModProjectileSnapshot snapshot, out string error)
        {
            snapshot = null;
            if (!Fight.ProjectileValid(this, false, out error)) return false;
            if (Birth != null) { error = "Projectile is still initializing."; return false; }
            var view = EclipseFighterOperations.Capture(Model);
            if (view == null) { error = "Projectile geometry is unavailable."; return false; }
            snapshot = new ModProjectileSnapshot(Sequence.ToString(CultureInfo.InvariantCulture), Model.get_Name(),
                Model.GetCurrentAnimation()?.Name ?? string.Empty, view.X, view.Y, view.Z,
                Math.Max(0, Fight.fightTimeInFrame - Born), Lifetime);
            return true;
        }
        public bool TryMoveBy(double x, double y, double z, out string error)
        {
            if (!Fight.ProjectileValid(this, true, out error)) return false;
            if (!ModProjectileLimits.ValidDisplacement(x, y, z) ||
                !ModProjectileLimits.ValidDisplacement(X + x, Y + y, Z + z))
            { error = "Individual and combined projectile displacement must be finite and within -100..100 per axis this step."; return false; }
            if (x == 0 && y == 0 && z == 0) return true;
            if (Requests >= ModProjectileLimits.MaximumRequestsPerStep)
            { error = "Projectile already has 32 nonzero movement requests pending this step."; return false; }
            X += x; Y += y; Z += z; Requests++;
            return true;
        }
        public bool TryRemove(out string error)
        {
            if (!Fight.ProjectileValid(this, true, out error)) return false;
            Removing = true; X = Y = Z = 0; return true;
        }
    }
    private readonly Dictionary<Model, OwnedProjectile> _eclipseProjectiles = new Dictionary<Model, OwnedProjectile>();
    private sealed class PendingProjectileSpawn
    {
        public Model Root;
        public ModScriptSession Session;
        public ModId Owner;
        public ProjectileDefinition Definition;
        public int Round;
        public double X, Y, Z;
        public Action<string, string> Complete;
    }
    private readonly List<PendingProjectileSpawn> _eclipseProjectileSpawns = new List<PendingProjectileSpawn>();
    private bool _applyingEclipseProjectileSpawns;
    private PendingProjectileSpawn _materializingEclipseProjectile;

    private bool TryQueueEclipseProjectileSpawn(Model root, ModId owner, DefinitionId definition,
        double x, double y, double z, Action<string, string> complete, out string error)
    {
        error = null;
        var session = ModRuntime.Scripts;
        if (_applyingEclipseProjectileSpawns || session == null || !CanMoveEclipseFighter(root) || IsPaused() ||
            !ProjectileOwnerActive(session, owner))
        { error = "Projectile spawning requires a living main fighter during an active offline simulation callback."; return false; }
        if (definition.Namespace != owner || !session.Content.TryGetProjectile(definition, out var prefab))
        { error = "Projectile definition must be registered by the calling mod."; return false; }
        if (!ModFighterMotionLimits.IsValid(x, y, z))
        { error = "Spawn offset must be finite and within -1000..1000 per axis."; return false; }
        if (!CanSpawnEclipseProjectile(root, owner.Value, prefab.Specification.LifetimeFrames))
        { error = "Projectile ownership capacity is full."; return false; }
        _eclipseProjectileSpawns.Add(new PendingProjectileSpawn {
            Root = root, Owner = owner, Session = session, Definition = prefab, Round = round.round,
            X = x, Y = y, Z = z, Complete = complete });
        return true;
    }

    private bool ProjectileSpawnValid(PendingProjectileSpawn request) => request.Round == round.round &&
        ReferenceEquals(request.Session, ModRuntime.Scripts) && ProjectileOwnerActive(request.Session, request.Owner) &&
        CanMoveEclipseFighter(request.Root);

    private static void FinishEclipseProjectileSpawn(PendingProjectileSpawn request, string id, string error)
    {
        var complete = request.Complete; request.Complete = null;
        try { complete?.Invoke(id, error); }
        catch (Exception failure) { UnityEngine.Debug.LogWarning("[ModProjectiles] Receipt update failed: " + failure.Message); }
    }

    private void ApplyEclipseProjectileSpawns()
    {
        if (IsPaused() || _eclipseProjectileSpawns.Count == 0) return;
        _applyingEclipseProjectileSpawns = true;
        try
        {
            // Keep other reservations counted while materializing each request.
            while (_eclipseProjectileSpawns.Count != 0)
            {
                var request = _eclipseProjectileSpawns[0]; _eclipseProjectileSpawns.RemoveAt(0);
                if (!ProjectileSpawnValid(request))
                { FinishEclipseProjectileSpawn(request, null, "Fighter, owner, round or session changed before spawn."); continue; }
                Model child = null;
                try
                {
                    var root = EclipseFighterOperations.Capture(request.Root);
                    if (root == null) throw new InvalidOperationException("Fighter position is unavailable.");
                    request.X += root.X; request.Y += root.Y; request.Z += root.Z;
                    _materializingEclipseProjectile = request;
                    child = ModRuntime.SpawnProjectile(request.Root, request.Definition);
                    if (child == null || !_eclipseProjectiles.TryGetValue(child, out var entry))
                        throw new InvalidOperationException("Native creation rejected the projectile.");
                }
                catch (Exception failure)
                {
                    // A native create listener can throw after registration, before
                    // the factory returns its model. Retire that birth as well.
                    foreach (var entry in _eclipseProjectiles.Values.Where(p => ReferenceEquals(p.Birth, request)).ToArray())
                        RetireEclipseProjectile(entry);
                    FinishEclipseProjectileSpawn(request, null, "Native projectile spawn failed: " + failure.Message);
                }
                finally { _materializingEclipseProjectile = null; }
            }
        }
        finally { _applyingEclipseProjectileSpawns = false; }
    }

    private void InitializeEclipseProjectileBirths()
    {
        foreach (var entry in _eclipseProjectiles.Values.ToArray())
            if (entry.Birth != null) InitializeEclipseProjectile(entry.Model);
    }

    // Native birth selection has rendered the first weapon keyframe. No collision
    // pass includes these newborns until the next simulation step.
    private void InitializeEclipseProjectile(Model child)
    {
        if (!_eclipseProjectiles.TryGetValue(child, out var entry) || entry.Birth == null) return;
        var request = entry.Birth;
        try
        {
            if (!ProjectileValid(entry, false, out _) || !ProjectileSpawnValid(request) || !child.ExplicitBirthAnimationStarted || child.GetCurrentAnimation() == null)
                throw new InvalidOperationException("Projectile was cancelled or its birth move did not start.");
            // WeaponModel renders its first keyframe during birth selection,
            // before Model.Render refreshes the cached center of mass. The
            // native translation path refreshes geometry even for a zero shift.
            child.ShiftModelPosition(new Vector3f(0, 0, 0), true);
            var view = EclipseFighterOperations.Capture(child);
            if (view == null) throw new InvalidOperationException("Projectile geometry is unavailable.");
            // The native constraint solver can slightly change the center after
            // translation. Correct its residual within a bounded birth-only pass.
            var placed = view;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                child.ShiftModelPosition(new Vector3f((float)(request.X - placed.X), (float)(request.Y - placed.Y),
                    (float)(request.Z - placed.Z)), true);
                placed = EclipseFighterOperations.Capture(child);
                if (placed == null || NearSpawnPoint(placed.X, request.X) && NearSpawnPoint(placed.Y, request.Y) && NearSpawnPoint(placed.Z, request.Z)) break;
            }
            if (placed == null || !NearSpawnPoint(placed.X, request.X) || !NearSpawnPoint(placed.Y, request.Y) ||
                !NearSpawnPoint(placed.Z, request.Z))
                throw new InvalidOperationException("Native constraints prevented spawn placement: requested " +
                    request.X.ToString("R", CultureInfo.InvariantCulture) + "," + request.Y.ToString("R", CultureInfo.InvariantCulture) + "," + request.Z.ToString("R", CultureInfo.InvariantCulture) +
                    "; observed " + (placed == null ? "unavailable" : placed.X.ToString("R", CultureInfo.InvariantCulture) + "," +
                        placed.Y.ToString("R", CultureInfo.InvariantCulture) + "," + placed.Z.ToString("R", CultureInfo.InvariantCulture)) + ".");
            entry.Birth = null; entry.Born = fightTimeInFrame;
            FinishEclipseProjectileSpawn(request, entry.Sequence.ToString(CultureInfo.InvariantCulture), null);
        }
        catch (Exception failure)
        {
            entry.Birth = null;
            RetireEclipseProjectile(entry);
            FinishEclipseProjectileSpawn(request, null, "Native projectile initialization failed: " + failure.Message);
        }
    }
    private static bool NearSpawnPoint(double actual, double expected) => !double.IsNaN(actual) &&
        !double.IsInfinity(actual) && Math.Abs(actual - expected) <= 0.05;
    private long _eclipseProjectileSequence;
    private ModAttackSource CaptureEclipseAttackSource(Model attacker, Model.StrikeResult strike)
    {
        if (attacker == null || strike?.Point == null) return null;
        double x = strike.Point.GetX(), y = strike.Point.GetY(), z = strike.Point.GetZ();
        if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y) || double.IsNaN(z) || double.IsInfinity(z)) return null;
        bool owned = _eclipseProjectiles.TryGetValue(attacker, out var entry) &&
            ReferenceEquals(entry.Session, ModRuntime.Scripts) && ProjectileOwnerActive(entry.Session, entry.Owner) &&
            entry.Round == round.round && entry.Root == attacker.GetRootModel();
        string kind = owned ? "projectile" : attacker == attacker.GetRootModel() ? "fighter" : "native_child";
        return new ModAttackSource(kind, attacker.get_Name(), strike.AttackAnimation?.Name, x, y, z,
            owned ? entry.Sequence.ToString(CultureInfo.InvariantCulture) : null, owned ? entry.Owner.ToString() : null);
    }
    private bool ProjectileOwnerActive(ModScriptSession session, ModId owner) => session != null && !session.IsDisposed &&
        session.ActiveMods.Any(mod => mod.Id == owner);

    internal bool CanSpawnEclipseProjectile(Model parent, string ownerName, int lifetime)
    {
        bool accepted = ModId.TryParse(ownerName, out var owner) &&
            lifetime >= 1 && lifetime <= ModProjectileLimits.MaximumLifetimeFrames &&
            ModRuntime.Scripts != null && ProjectileOwnerActive(ModRuntime.Scripts, owner) &&
            CanMoveEclipseFighter(parent?.GetRootModel()) && !IsPaused() &&
            _eclipseProjectiles.Count + _eclipseProjectileSpawns.Count < ModProjectileLimits.MaximumPerFight &&
            _eclipseProjectiles.Values.Count(p => p.Owner == owner) + _eclipseProjectileSpawns.Count(p => p.Owner == owner) < ModProjectileLimits.MaximumPerMod;
        if (!accepted) UnityEngine.Debug.LogWarning("[ModProjectiles] Spawn rejected for " + ownerName + ": active offline fighter, valid lifetime and available ownership capacity required.");
        return accepted;
    }
    internal void RegisterEclipseProjectile(Model parent, Model child, string ownerName, int lifetime)
    {
        // Called after parent linkage but BEFORE the native create event/register/render.
        var birth = _materializingEclipseProjectile;
        _materializingEclipseProjectile = null;
        _eclipseProjectiles.Add(child, new OwnedProjectile {
            Fight = this, Root = parent.GetRootModel(), Model = child, Owner = ModId.Parse(ownerName),
            Session = ModRuntime.Scripts, Round = round.round, Born = fightTimeInFrame,
            Lifetime = lifetime, Sequence = ++_eclipseProjectileSequence, Birth = birth
        });
    }
    private bool ProjectileValid(OwnedProjectile entry, bool mutation, out string error)
    {
        error = null;
        if (!_eclipseProjectiles.TryGetValue(entry.Model, out var registered) || !ReferenceEquals(registered, entry) ||
            entry.Removing || JLEFIKJODGG.Contains(entry.Model) ||
            (!LNDLFINJHDB.Contains(entry.Model) && !HCPGFOCGDAA.Contains(entry.Model)) ||
            !ReferenceEquals(entry.Session, ModRuntime.Scripts) || !ProjectileOwnerActive(entry.Session, entry.Owner) ||
            entry.Round != round.round || !CanMoveEclipseFighter(entry.Root) ||
            entry.Model.GetRootModel() != entry.Root || entry.Birth == null && fightTimeInFrame - entry.Born >= entry.Lifetime ||
            (mutation && IsPaused()))
        { error = "Projectile has expired, is removing, or is outside its active offline owner/round."; return false; }
        return true;
    }
    private bool TryGetEclipseProjectiles(Model root, ModId owner, out IReadOnlyList<IModProjectile> projectiles, out string error)
    {
        projectiles = null; error = null;
        if (!CanMoveEclipseFighter(root) || !ProjectileOwnerActive(ModRuntime.Scripts, owner))
        { error = "Projectile observations require a living main fighter during an active offline round."; return false; }
        projectiles = _eclipseProjectiles.Values.Where(p => p.Root == root && p.Owner == owner && p.Birth == null && ProjectileValid(p, false, out _))
            .OrderBy(p => p.Sequence).Cast<IModProjectile>().ToArray();
        return true;
    }
    private void UpdateEclipseProjectiles()
    {
        foreach (var entry in _eclipseProjectiles.Values.ToArray())
            if (!ProjectileValid(entry, false, out _)) RetireEclipseProjectile(entry);
    }
    private void ApplyEclipseProjectiles()
    {
        if (IsPaused()) return;
        foreach (var entry in _eclipseProjectiles.Values.OrderBy(p => p.Sequence).ToArray())
        {
            if (!ProjectileValid(entry, true, out _)) { RetireEclipseProjectile(entry); continue; }
            double x = entry.X, y = entry.Y, z = entry.Z;
            entry.X = entry.Y = entry.Z = 0; entry.Requests = 0;
            if (x == 0 && y == 0 && z == 0) continue;
            try { entry.Model.ShiftModelPosition(new Vector3f((float)x, (float)y, (float)z), true); }
            catch (Exception failure) { UnityEngine.Debug.LogWarning("[ModProjectiles] Native displacement failed: " + failure.Message); }
        }
    }
    private void RetireEclipseProjectile(OwnedProjectile entry)
    {
        entry.Removing = true;
        if (entry.Birth != null) FinishEclipseProjectileSpawn(entry.Birth, null, "Projectile retired before native initialization.");
        entry.Birth = null;
        _eclipseProjectiles.Remove(entry.Model);
        if (LNDLFINJHDB.Contains(entry.Model) || HCPGFOCGDAA.Contains(entry.Model)) RequestModelRemoval(entry.Model);
    }
    private void ForgetEclipseProjectile(Model model)
    {
        if (_eclipseProjectiles.TryGetValue(model, out var entry) && entry.Birth != null)
            FinishEclipseProjectileSpawn(entry.Birth, null, "Projectile removed before native initialization.");
        _eclipseProjectiles.Remove(model);
    }
    private void CancelEclipseProjectiles()
    {
        var pending = _eclipseProjectileSpawns.ToArray(); _eclipseProjectileSpawns.Clear();
        foreach (var request in pending) FinishEclipseProjectileSpawn(request, null, "Projectile spawn cancelled by round/fight teardown.");
        foreach (var entry in _eclipseProjectiles.Values.ToArray()) RetireEclipseProjectile(entry);
    }
}
