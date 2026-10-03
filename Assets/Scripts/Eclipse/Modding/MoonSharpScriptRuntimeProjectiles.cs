using System;
using MoonSharp.Interpreter;

namespace Eclipse.Modding
{
    public sealed partial class MoonSharpScriptRuntime
    {
        private sealed partial class MoonSharpScriptContext
        {
            private sealed class ProjectileQueryBudget { public int Used; }
            private DynValue GetFighterProjectiles(CallbackArguments args, Table fighterTable,
                IModFighterOperations fighter, ModEffectEvent kind, Func<bool> active, ProjectileQueryBudget budget)
            {
                CheckProjectileCallback(kind, active);
                int offset = args[0].Type == DataType.Table && args[0].Table == fighterTable ? 1 : 0;
                if (args.Count != offset) throw new ScriptRuntimeException("projectiles expects no arguments.");
                if (++budget.Used > ModProjectileLimits.MaximumQueriesPerCallback)
                    throw new ScriptRuntimeException("At most 32 projectile queries are allowed per callback.");
                if (!(fighter is IModFighterProjectiles provider))
                    return DynValue.NewTuple(DynValue.Nil, DynValue.NewString("Projectile observations are unavailable."));
                if (!provider.TryGetProjectiles(_api.Mod.Id, out var projectiles, out var failure))
                    return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(failure ?? "Projectile observations rejected."));
                var list = new Table(_script);
                for (int i = 0; i < projectiles.Count; i++) list.Set(i + 1, ProjectileTable(projectiles[i], kind, active));
                return DynValue.NewTuple(DynValue.NewTable(list), DynValue.Nil);
            }
            private void CheckProjectileCallback(ModEffectEvent kind, Func<bool> active)
            {
                if (!active()) throw new ScriptRuntimeException("Projectile references have expired; reacquire inside the current callback.");
                _api.RequireCapability("combat.projectiles");
                if (kind == ModEffectEvent.FightBegin || kind == ModEffectEvent.RoundBegin ||
                    kind == ModEffectEvent.RoundEnd || kind == ModEffectEvent.FightEnd)
                    throw new ScriptRuntimeException("Projectile operations require an active simulation callback.");
            }
            private DynValue ProjectileTable(IModProjectile projectile, ModEffectEvent kind, Func<bool> active)
            {
                var projectileTable = new Table(_script);
                int Offset(CallbackArguments args)
                {
                    CheckProjectileCallback(kind, active);
                    return args[0].Type == DataType.Table && args[0].Table == projectileTable ? 1 : 0;
                }
                DynValue Result(bool accepted, string error) => DynValue.NewTuple(DynValue.NewBoolean(accepted),
                    accepted ? DynValue.Nil : DynValue.NewString(error ?? "Projectile command rejected."));
                projectileTable.Set("snapshot", DynValue.NewCallback((ctx, args) =>
                {
                    if (args.Count != Offset(args)) throw new ScriptRuntimeException("Projectile snapshot expects no arguments.");
                    if (!projectile.TrySnapshot(out var snapshot, out var failure))
                        return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(failure ?? "Projectile expired."));
                    var observation = new Table(_script);
                    observation.Set("id", DynValue.NewString(snapshot.Id));
                    observation.Set("name", DynValue.NewString(snapshot.Name));
                    observation.Set("animation_name", DynValue.NewString(snapshot.Animation));
                    observation.Set("age_frames", DynValue.NewNumber(snapshot.AgeFrames));
                    observation.Set("lifetime_frames", DynValue.NewNumber(snapshot.LifetimeFrames));
                    var position = new Table(_script);
                    position.Set("x", DynValue.NewNumber(snapshot.X)); position.Set("y", DynValue.NewNumber(snapshot.Y)); position.Set("z", DynValue.NewNumber(snapshot.Z));
                    observation.Set("position", DynValue.NewTable(position));
                    return DynValue.NewTuple(DynValue.NewTable(observation), DynValue.Nil);
                }));
                projectileTable.Set("move_by", DynValue.NewCallback((ctx, args) =>
                {
                    int offset = Offset(args), count = args.Count - offset;
                    if (count < 2 || count > 3 || args[offset].Type != DataType.Number || args[offset + 1].Type != DataType.Number ||
                        count == 3 && !args[offset + 2].IsNil() && args[offset + 2].Type != DataType.Number)
                        throw new ScriptRuntimeException("Projectile move_by expects numeric x, y and optional z.");
                    double x = args[offset].Number, y = args[offset + 1].Number,
                        z = count == 3 && !args[offset + 2].IsNil() ? args[offset + 2].Number : 0;
                    if (!ModProjectileLimits.ValidDisplacement(x, y, z))
                        throw new ScriptRuntimeException("Projectile displacement must be finite and within -100..100 on each axis.");
                    return Result(projectile.TryMoveBy(x, y, z, out var failure), failure);
                }));
                projectileTable.Set("remove", DynValue.NewCallback((ctx, args) =>
                {
                    if (args.Count != Offset(args)) throw new ScriptRuntimeException("Projectile remove expects no arguments.");
                    return Result(projectile.TryRemove(out var failure), failure);
                }));
                return DynValue.NewTable(projectileTable);
            }
        }
    }
}
