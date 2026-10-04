using UnityEngine;

namespace Eclipse.Rendering
{
    // A separate perspective pass keeps the recovered arena and UI projection.
    // Owned by the native ViewerModel, so scene/fight teardown removes it.
    public sealed class ExperimentalFighterCamera : MonoBehaviour
    {
        public const int Layer = 30;
        const int Mask = 1 << Layer;
        UnityEngine.Camera source, perspective;
        bool removedSourceLayer;

        public static bool ActiveFor(Transform modelPart)
        {
            if (!SF2DisplayFrameRate.Experimental3DEnabled) return false;
            var player = Fight.GetCurrentFight()?.GetPlayerModel()?.GetRenderObject();
            Transform viewer = player != null ? player.transform.parent : null;
            // Menu sparring and previews have their own viewers/cameras, even
            // while a real fight exists. Never draw those through the fight pass.
            if (viewer == null || !modelPart.IsChildOf(viewer) || UnityEngine.Camera.main == null) return false;
            var driver = viewer.GetComponent<ExperimentalFighterCamera>();
            if (driver == null) driver = viewer.gameObject.AddComponent<ExperimentalFighterCamera>();
            return driver.EnsureCamera();
        }

        bool EnsureCamera()
        {
            var current = UnityEngine.Camera.main;
            if (current == null) return false;
            if (source != current) { RestoreSource(); source = current; }
            if (perspective == null)
            {
                perspective = new GameObject("Eclipse experimental perspective camera").AddComponent<UnityEngine.Camera>();
                // Native camera coordinates point down the screen. Reuse its
                // projection reflection and culling lifecycle for this pass.
                if (source.GetComponent<Nekki.SF2.Core.Scripts.InvertCamera>() != null)
                    perspective.gameObject.AddComponent<Nekki.SF2.Core.Scripts.InvertCamera>();
                perspective.clearFlags = CameraClearFlags.Depth;
                perspective.cullingMask = Mask;
                perspective.orthographic = false;
                perspective.fieldOfView = 32f;
                perspective.allowHDR = false;
                perspective.allowMSAA = true;
                perspective.nearClipPlane = .1f;
                perspective.farClipPlane = 100000f;
            }
            if ((source.cullingMask & Mask) != 0) { source.cullingMask &= ~Mask; removedSourceLayer = true; }
            perspective.enabled = true;
            return true;
        }
        void LateUpdate()
        {
            if (!SF2DisplayFrameRate.Experimental3DEnabled || Fight.GetCurrentFight() == null)
            { if (perspective != null) perspective.enabled = false; RestoreSource(); return; }
            if (!EnsureCamera()) return;
            perspective.rect = source.rect; perspective.aspect = source.aspect;
            perspective.targetTexture = source.targetTexture; perspective.targetDisplay = source.targetDisplay;
            perspective.depth = source.depth + .1f;
            // Match the original framing at the central fighter plane, then view
            // the real XYZ pose without changing the original viewing direction.
            Vector3 forward = source.transform.forward;
            Vector3 focus = source.transform.position + forward * Vector3.Dot(transform.position - source.transform.position, forward);
            float distance = source.orthographicSize / Mathf.Tan(perspective.fieldOfView * Mathf.Deg2Rad * .5f);
            Quaternion angle = source.transform.rotation;
            perspective.transform.SetPositionAndRotation(focus - angle * Vector3.forward * distance, angle);
        }
        void RestoreSource()
        { if (source != null && removedSourceLayer) source.cullingMask |= Mask; removedSourceLayer = false; }
        void OnDisable() { RestoreSource(); if (perspective != null) perspective.enabled = false; }
        void OnDestroy() { RestoreSource(); if (perspective != null) Destroy(perspective.gameObject); }
    }
}
