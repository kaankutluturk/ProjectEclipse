using System;
using System.Collections.Generic;

namespace Eclipse.Modding
{
    // Detached native contact provenance. IDs identify observations, not capabilities.
    public sealed class ModAttackSource
    {
        public string Kind { get; } public string ModelName { get; } public string AnimationName { get; }
        public string ProjectileId { get; } public string ProjectileOwner { get; }
        public double X { get; } public double Y { get; } public double Z { get; }
        public ModAttackSource(string kind, string modelName, string animationName, double x, double y, double z,
            string projectileId = null, string projectileOwner = null)
        {
            if (kind != "fighter" && kind != "projectile" && kind != "native_child")
                throw new ArgumentException("Unknown native attack source kind.", nameof(kind));
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y) || double.IsNaN(z) || double.IsInfinity(z))
                throw new ArgumentOutOfRangeException(nameof(x));
            if ((kind == "projectile") != (!string.IsNullOrEmpty(projectileId) && !string.IsNullOrEmpty(projectileOwner)) ||
                kind != "projectile" && (projectileId != null || projectileOwner != null))
                throw new ArgumentException("Only an owned projectile source supplies its ID and owner.");
            Kind = kind; ModelName = modelName ?? string.Empty; AnimationName = animationName ?? string.Empty;
            ProjectileId = projectileId; ProjectileOwner = projectileOwner; X = x; Y = y; Z = z;
        }
    }

    public static class ModProjectileLimits
    {
        public const int DefaultLifetimeFrames = 180, MaximumLifetimeFrames = 600;
        public const int MaximumPerMod = 16, MaximumPerFight = 64, MaximumQueriesPerCallback = 32;
        public const int MaximumRequestsPerStep = 32;
        public const double MaximumDisplacement = 100;
        public static bool ValidDisplacement(double x, double y, double z) =>
            Valid(x) && Valid(y) && Valid(z);
        private static bool Valid(double value) => !double.IsNaN(value) && !double.IsInfinity(value) &&
            Math.Abs(value) <= MaximumDisplacement;
    }

    // Copied observations contain no native model or Unity objects.
    public sealed class ModProjectileSnapshot
    {
        public string Id { get; } public string Name { get; } public string Animation { get; }
        public double X { get; } public double Y { get; } public double Z { get; }
        public int AgeFrames { get; } public int LifetimeFrames { get; }
        public ModProjectileSnapshot(string id, string name, string animation, double x, double y, double z,
            int ageFrames, int lifetimeFrames)
        {
            Id = id; Name = name; Animation = animation; X = x; Y = y; Z = z;
            AgeFrames = ageFrames; LifetimeFrames = lifetimeFrames;
        }
    }

    public interface IModProjectile
    {
        bool TrySnapshot(out ModProjectileSnapshot snapshot, out string error);
        bool TryMoveBy(double x, double y, double z, out string error);
        bool TryRemove(out string error);
    }
    public interface IModFighterProjectiles
    {
        bool TryGetProjectiles(ModId owner, out IReadOnlyList<IModProjectile> projectiles, out string error);
    }
}
