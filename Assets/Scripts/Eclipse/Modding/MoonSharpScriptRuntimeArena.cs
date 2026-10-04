using MoonSharp.Interpreter;

namespace Eclipse.Modding
{
    public sealed partial class MoonSharpScriptRuntime
    {
        private sealed partial class MoonSharpScriptContext
        {
            private readonly ModArenaMarkerScope _arenaMarkers = new ModArenaMarkerScope();
            private readonly System.Runtime.CompilerServices.ConditionalWeakTable<Table, ModArenaMarkerInstance> _markerHandles =
                new System.Runtime.CompilerServices.ConditionalWeakTable<Table, ModArenaMarkerInstance>();
            private sealed class ArenaQueryBudget { public int Used; }
            private void AddWorldModule(Table root)
            {
                var world = new Table(_script);
                world.Set("remove_marker", DynValue.NewCallback((ctx, args) => ApiCall("sf2.world.remove_marker", () =>
                    DynValue.NewBoolean(Marker(args, 1).Remove()))));
                world.Set("is_marker_active", DynValue.NewCallback((ctx, args) => ApiCall("sf2.world.is_marker_active", () =>
                    DynValue.NewBoolean(Marker(args, 1).IsActive))));
                world.Set("set_marker_color", DynValue.NewCallback((ctx, args) => ApiCall("sf2.world.set_marker_color", () =>
                {
                    var marker = Marker(args, 2);
                    if (args[1].Type != DataType.String) throw new ModContentException("Marker color must be #RRGGBB or #RRGGBBAA.");
                    return DynValue.NewBoolean(marker.SetColor(new ModUiColor(args[1].String)));
                })));
                world.Set("set_marker_rect", DynValue.NewCallback((ctx, args) => ApiCall("sf2.world.set_marker_rect", () =>
                {
                    var marker = Marker(args, 2);
                    bool changed = marker.TrySetRect(ArenaRect(args[1]), out var error);
                    return DynValue.NewTuple(DynValue.NewBoolean(changed), error == null ? DynValue.Nil : DynValue.NewString(error));
                })));
                world.Set("set_marker_sprite", DynValue.NewCallback((ctx, args) => ApiCall("sf2.world.set_marker_sprite", () =>
                {
                    var marker = Marker(args, 2);
                    bool changed = marker.TrySetSprite(ArenaSprite(args[1]), out var error);
                    return DynValue.NewTuple(DynValue.NewBoolean(changed), error == null ? DynValue.Nil : DynValue.NewString(error));
                })));
                root.Set("world", DynValue.NewTable(world));
            }
            private ModArenaMarkerInstance Marker(CallbackArguments args, int count)
            {
                ThrowIfDisposed(); _api.RequireCapability("presentation.visuals");
                if (args.Count != count || args[0].Type != DataType.Table || !_markerHandles.TryGetValue(args[0].Table, out var marker))
                    throw new ModContentException("Expected an arena marker owned by this script and exactly " + count + " arguments.");
                return marker;
            }
            private AssetId ArenaSprite(DynValue value)
            {
                if (value.Type != DataType.Table || !_spriteHandles.TryGetValue(value.Table, out var sprite))
                    throw new ModContentException("Arena artwork requires a sprite handle from this script context.");
                return sprite;
            }
            private ModArenaRect ArenaRect(DynValue value)
            {
                if (value.Type != DataType.Table) throw new ModContentException("Arena rectangle must be a table.");
                var t = value.Table; ValidateFields(t, "rectangle", "x", "y", "width", "height");
                double Number(string name)
                {
                    var v = t.Get(name);
                    if (v.Type != DataType.Number) throw new ModContentException("Rectangle " + name + " must be a number.");
                    return v.Number;
                }
                return new ModArenaRect(Number("x"), Number("y"), Number("width"), Number("height"));
            }
            private DynValue OverlapRect(CallbackArguments args, Table handle, IModFighterOperations fighter,
                bool active, bool opponent, ArenaQueryBudget budget)
            {
                if (!active) throw new ScriptRuntimeException("Fighter observations have expired.");
                if (opponent) _api.RequireCapability("combat.target");
                int offset = args[0].Type == DataType.Table && args[0].Table == handle ? 1 : 0;
                if (args.Count - offset != 1) throw new ModContentException("overlaps_rect requires exactly one rectangle.");
                var rect = ArenaRect(args[offset]);
                if (++budget.Used > 32) throw new ModContentException("A combat callback may make at most 32 rectangle queries, shared by self and opponent.");
                if (!(fighter is IModFighterRegions source)) return DynValue.NewTuple(DynValue.Nil, DynValue.NewString("Arena geometry is unavailable in this host."));
                if (!source.TryOverlapRect(rect, out var overlaps, out var error))
                    return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(error ?? "Arena geometry is unavailable."));
                return DynValue.NewTuple(DynValue.NewBoolean(overlaps), DynValue.Nil);
            }
            private DynValue MarkRect(CallbackArguments args, Table handle, IModFighterOperations fighter, ModEffectEvent kind, bool active, bool spriteArtwork = false)
            {
                if (!active) throw new ScriptRuntimeException("Fighter operations have expired.");
                _api.RequireCapability("presentation.visuals");
                if (_api.Registration != null && !_api.Registration.IsCatalogFrozen)
                    throw new ModContentException("Arena markers require all mod registration to be complete.");
                if (_uiCloseDepth != 0 || kind == ModEffectEvent.FightBegin || kind == ModEffectEvent.RoundBegin || kind == ModEffectEvent.RoundEnd || kind == ModEffectEvent.FightEnd)
                    throw new ModContentException("Arena markers require an active simulation callback outside cleanup.");
                int offset = args[0].Type == DataType.Table && args[0].Table == handle ? 1 : 0;
                int required = spriteArtwork ? 2 : 1;
                if (args.Count - offset < required || args.Count - offset > required + 1)
                    throw new ModContentException(spriteArtwork ? "mark_sprite requires a sprite handle, rectangle and optional color." : "mark_rect requires a rectangle and optional color.");
                AssetId sprite = spriteArtwork ? ArenaSprite(args[offset++]) : default(AssetId);
                var rect = ArenaRect(args[offset]); var color = args[offset + 1];
                if (!color.IsNil() && color.Type != DataType.String) throw new ModContentException("Marker color must be #RRGGBB or #RRGGBBAA.");
                var tint = new ModUiColor(color.IsNil() ? (spriteArtwork ? "#ffffffff" : "#ffcc3366") : color.String);
                ModArenaMarkerInstance marker; string error;
                bool created = spriteArtwork ? _arenaMarkers.TryCreateSprite(fighter as IModFighterArtwork, sprite, rect, tint, out marker, out error)
                    : _arenaMarkers.TryCreate(fighter as IModFighterRegions, rect, tint, out marker, out error);
                if (!created) return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(error));
                var table = new Table(_script); _markerHandles.Add(table, marker);
                return DynValue.NewTuple(DynValue.NewTable(table), DynValue.Nil);
            }
        }
    }
}
