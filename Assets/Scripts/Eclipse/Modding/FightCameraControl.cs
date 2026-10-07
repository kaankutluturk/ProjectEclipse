using System;
using System.Collections.Generic;
using Eclipse.Modding;

public partial class Fight
{
    private readonly ModCameraSlot _eclipseCamera = new ModCameraSlot();
    internal ModCameraSettings GetEclipseCameraSettings(Render render) =>
        GetCurrentFight() == this && _Camera?.GetRender() == render ? _eclipseCamera.Settings : null;
    private bool TryAcquireEclipseCamera(Model model, ModId owner, ModCameraSettings settings,
        out IModCameraControl camera, out string error)
    {
        camera = null; error = null;
        if (model == null) { error = "Camera fighter is unavailable."; return false; }
        var session = ModRuntime.Scripts;
        bool player = model == GetPlayerModel();
        _eclipseActors.TryGetValue(model, out var actor);
        bool Alive() => GetCurrentFight() == this && !get_IsRaidFight() &&
            ReferenceEquals(session, ModRuntime.Scripts) && ProjectileOwnerActive(session, owner) &&
            (actor != null ? actor.Owner == owner && actor.Birth == null && ActorValid(actor, false, out _) :
                CanMoveEclipseFighter(player ? GetPlayerModel() : GetEnemyModel()));
        if (model == null || session == null || IsPaused() || !Alive() ||
            actor == null && model != GetPlayerModel() && model != GetEnemyModel())
        { error = "Camera acquisition requires a living fighter in an active offline non-raid round."; return false; }
        int roundNumber = round.round;
        return _eclipseCamera.TryAcquire(owner, settings, () => round.round == roundNumber && Alive(), out camera, out error);
    }
}

namespace Eclipse.Modding
{
    // Reversible native layer offsets. Each draw removes its previous contribution
    // before native positioning, so interpolation/repeated draws cannot accumulate.
    internal sealed class ModCameraProjection
    {
        private readonly Dictionary<LocationSelector, float> vertical = new Dictionary<LocationSelector, float>();
        internal void Begin()
        {
            foreach (var entry in vertical)
            {
                var body = entry.Key.GetLayerObject();
                if (body != null) entry.Key.SetPositionY(body.transform.localPosition.y - entry.Value);
            }
            vertical.Clear();
        }
        internal void ApplyVertical(LocationSelector layer, double offset, float zoom, float factor)
        {
            float amount = -(float)offset * zoom * factor;
            if (amount == 0) return;
            float before = layer.GetLayerObject().transform.localPosition.y;
            layer.SetPositionY(before + amount);
            vertical[layer] = layer.GetLayerObject().transform.localPosition.y - before;
        }
    }
}
