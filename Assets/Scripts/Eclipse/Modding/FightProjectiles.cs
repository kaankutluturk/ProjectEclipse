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
        public int Requests;
        public double X, Y, Z;
        public bool TrySnapshot(out ModProjectileSnapshot snapshot, out string error)
        {
            snapshot = null;
            if (!Fight.ProjectileValid(this, false, out error)) return false;
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
    private long _eclipseProjectileSequence;
    private bool ProjectileOwnerActive(ModScriptSession session, ModId owner) => session != null && !session.IsDisposed &&
        session.ActiveMods.Any(mod => mod.Id == owner);

    internal bool CanSpawnEclipseProjectile(Model parent, string ownerName, int lifetime)
    {
        bool accepted = ModId.TryParse(ownerName, out var owner) &&
            lifetime >= 1 && lifetime <= ModProjectileLimits.MaximumLifetimeFrames &&
            ModRuntime.Scripts != null && ProjectileOwnerActive(ModRuntime.Scripts, owner) &&
            CanMoveEclipseFighter(parent?.GetRootModel()) && !IsPaused() &&
            _eclipseProjectiles.Count < ModProjectileLimits.MaximumPerFight &&
            _eclipseProjectiles.Values.Count(p => p.Owner == owner) < ModProjectileLimits.MaximumPerMod;
        if (!accepted) UnityEngine.Debug.LogWarning("[ModProjectiles] Spawn rejected for " + ownerName + ": active offline fighter, valid lifetime and available ownership capacity required.");
        return accepted;
    }
    internal void RegisterEclipseProjectile(Model parent, Model child, string ownerName, int lifetime)
    {
        // Called after parent linkage but BEFORE the native create event/register/render.
        _eclipseProjectiles.Add(child, new OwnedProjectile {
            Fight = this, Root = parent.GetRootModel(), Model = child, Owner = ModId.Parse(ownerName),
            Session = ModRuntime.Scripts, Round = round.round, Born = fightTimeInFrame,
            Lifetime = lifetime, Sequence = ++_eclipseProjectileSequence
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
            entry.Model.GetRootModel() != entry.Root || fightTimeInFrame - entry.Born >= entry.Lifetime ||
            (mutation && IsPaused()))
        { error = "Projectile has expired, is removing, or is outside its active offline owner/round."; return false; }
        return true;
    }
    private bool TryGetEclipseProjectiles(Model root, ModId owner, out IReadOnlyList<IModProjectile> projectiles, out string error)
    {
        projectiles = null; error = null;
        if (!CanMoveEclipseFighter(root) || !ProjectileOwnerActive(ModRuntime.Scripts, owner))
        { error = "Projectile observations require a living main fighter during an active offline round."; return false; }
        projectiles = _eclipseProjectiles.Values.Where(p => p.Root == root && p.Owner == owner && ProjectileValid(p, false, out _))
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
        _eclipseProjectiles.Remove(entry.Model);
        if (LNDLFINJHDB.Contains(entry.Model) || HCPGFOCGDAA.Contains(entry.Model)) RequestModelRemoval(entry.Model);
    }
    private void ForgetEclipseProjectile(Model model) => _eclipseProjectiles.Remove(model);
    private void CancelEclipseProjectiles()
    {
        foreach (var entry in _eclipseProjectiles.Values.ToArray()) RetireEclipseProjectile(entry);
    }
}
