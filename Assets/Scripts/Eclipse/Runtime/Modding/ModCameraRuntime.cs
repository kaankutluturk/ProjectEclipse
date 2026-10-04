using System;

namespace Eclipse.Modding
{
    public sealed class ModCameraSettings
    {
        public double? CenterX { get; }
        public double OffsetY { get; }
        public double? Zoom { get; }
        public ModCameraSettings(double? centerX = null, double offsetY = 0, double? zoom = null)
        {
            if (centerX.HasValue && (!Finite(centerX.Value) || Math.Abs(centerX.Value) > 10000) ||
                !Finite(offsetY) || Math.Abs(offsetY) > 1000 ||
                zoom.HasValue && (!Finite(zoom.Value) || zoom.Value < .25 || zoom.Value > 4))
                throw new ArgumentException("Camera center_x must be finite in -10000..10000, offset_y in -1000..1000 and zoom in 0.25..4.");
            CenterX = centerX; OffsetY = offsetY; Zoom = zoom;
        }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
    public interface IModCameraControl : IDisposable
    {
        bool IsActive { get; }
        bool TrySet(ModCameraSettings settings, out string error);
    }
    public interface IModFighterCamera
    {
        bool TryAcquireCamera(ModId owner, ModCameraSettings settings, out IModCameraControl camera, out string error);
    }
    // One exclusive presentation owner per native fight. Neither requests nor
    // releases modify the native automatic camera state or its effect timers.
    public sealed class ModCameraSlot : IDisposable
    {
        private Lease current;
        private bool closed;
        public ModCameraSettings Settings => current != null && current.IsActive ? current.Settings : null;
        public bool TryAcquire(ModId owner, ModCameraSettings settings, Func<bool> alive, out IModCameraControl camera, out string error)
        {
            camera = null; error = null;
            if (settings == null || alive == null) throw new ArgumentNullException();
            if (closed) { error = "Camera slot is closed."; return false; }
            if (current != null && current.IsActive)
            { error = "Fight camera is already owned by " + current.Owner + ". Reuse or release its current handle."; return false; }
            var lease = new Lease(this, owner, settings, alive);
            current = lease;
            if (!lease.IsActive) { error = "Camera owner is no longer active."; return false; }
            camera = lease; return true;
        }
        public void Clear() { current?.Dispose(); }
        public void Dispose() { if (closed) return; closed = true; Clear(); }
        private sealed class Lease : IModCameraControl
        {
            private ModCameraSlot slot;
            private Func<bool> alive;
            internal ModId Owner { get; }
            internal ModCameraSettings Settings { get; private set; }
            internal Lease(ModCameraSlot slot, ModId owner, ModCameraSettings settings, Func<bool> alive)
            { this.slot = slot; Owner = owner; Settings = settings; this.alive = alive; }
            public bool IsActive
            {
                get
                {
                    if (slot == null || slot.closed || slot.current != this) return false;
                    bool active;
                    try { active = alive(); } catch { active = false; }
                    if (!active) Dispose();
                    return active && slot != null && !slot.closed && slot.current == this;
                }
            }
            public bool TrySet(ModCameraSettings settings, out string error)
            {
                if (settings == null) throw new ArgumentNullException(nameof(settings));
                error = null;
                if (!IsActive) { error = "Camera ownership expired or was released."; return false; }
                Settings = settings; return true;
            }
            public void Dispose()
            {
                var previous = slot; slot = null; alive = null;
                if (previous != null && previous.current == this) previous.current = null;
            }
        }
    }
    public sealed class ModCameraScope : IDisposable
    {
        private ModCameraInstance current;
        private bool closed;
        public bool TryAcquire(IModFighterCamera source, ModId owner, ModCameraSettings settings, out ModCameraInstance camera, out string error)
        {
            camera = null; error = null;
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (closed) { error = "Camera scope is closed."; return false; }
            if (current != null && current.IsActive) { error = "This script already owns a fight camera. Reuse or release its handle."; return false; }
            current?.Release();
            if (source == null) { error = "Camera control is unavailable in this host."; return false; }
            IModCameraControl native = null;
            try
            {
                if (!source.TryAcquireCamera(owner, settings, out native, out error) || native == null)
                { error = error ?? "Camera acquisition failed."; return false; }
                if (closed || !native.IsActive)
                { error = closed ? "Camera scope is closed." : "Camera owner is no longer active."; return false; }
                current = new ModCameraInstance(native); camera = current; return true;
            }
            catch (Exception failure) { error = "Camera acquisition failed: " + failure.Message; return false; }
            finally
            {
                // Failed providers can return a partially allocated control. Attempt
                // disposal once, even when disposal itself throws, and never adopt
                // a control after reentrant scope teardown.
                if (camera == null && native != null)
                    try { native.Dispose(); }
                    catch (Exception failure) { error += " Cleanup failed: " + failure.Message; }
            }
        }
        public void Dispose() { if (closed) return; closed = true; current?.Release(); current = null; }
    }
    public sealed class ModCameraInstance
    {
        private IModCameraControl native;
        internal ModCameraInstance(IModCameraControl native) { this.native = native; }
        public bool IsActive => native != null && native.IsActive;
        public bool TrySet(ModCameraSettings settings, out string error)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            error = null;
            if (!IsActive) { Release(); error = "Camera ownership expired or was released."; return false; }
            return native.TrySet(settings, out error);
        }
        public bool Release()
        {
            var previous = native; native = null;
            if (previous == null) return false;
            previous.Dispose(); return true;
        }
    }
}
