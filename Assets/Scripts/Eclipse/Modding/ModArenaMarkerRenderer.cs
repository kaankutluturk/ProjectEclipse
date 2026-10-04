using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Modding
{
    // Arena-local geometry, independent of the fighter's animation and lifetime.
    // The current player render transform supplies the native coordinate mapping,
    // including mirrored arenas and a replacement model after a form change.
    internal sealed class ModArenaMarkerRenderer : MonoBehaviour, IModArenaArtwork
    {
        internal const int MaximumMarkers = 64;
        private static readonly HashSet<ModArenaMarkerRenderer> markers = new HashSet<ModArenaMarkerRenderer>();
        private Func<bool> alive;
        private Func<Transform> coordinates;
        private Mesh mesh;
        private Material material;
        private bool closed;
        private ModArenaRect rectangle;
        private Vector2[] shape = { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        private Vector2[] textureCoordinates = { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        private int[] triangles = { 0, 1, 2, 0, 2, 3 };

        internal static bool TryCreate(ModArenaRect rect, ModUiColor color, Func<bool> alive, Func<Transform> coordinates,
            out IModArenaMarker result, out string error, AssetId? sprite = null)
        {
            result = null; error = null;
            foreach (var old in new List<ModArenaMarkerRenderer>(markers))
                if (old == null) markers.Remove(old); else if (!old.IsActive) old.Dispose();
            if (!alive() || coordinates() == null) { error = "An active arena render transform is required."; return false; }
            if (markers.Count >= MaximumMarkers) { error = "The session already has 64 active arena markers."; return false; }
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) { error = "Arena marker shader is unavailable."; return false; }
            var host = new GameObject("Eclipse arena marker");
            var marker = host.AddComponent<ModArenaMarkerRenderer>();
            try
            {
                marker.alive = alive; marker.coordinates = coordinates;
                marker.material = new Material(shader); marker.SetColor(color);
                marker.mesh = new Mesh { name = "Eclipse arena artwork" };
                marker.rectangle = rect;
                if (sprite.HasValue) marker.SetSprite(sprite.Value); else marker.SetRect(rect);
                host.AddComponent<MeshFilter>().sharedMesh = marker.mesh;
                host.AddComponent<MeshRenderer>().sharedMaterial = marker.material;
                markers.Add(marker); marker.Refresh(); result = marker; return true;
            }
            catch { marker.Dispose(); throw; }
        }
        public bool IsActive
        {
            get
            {
                if (closed) return false;
                if (alive == null || !alive() || coordinates == null || coordinates() == null) { Dispose(); return false; }
                return true;
            }
        }
        public void SetColor(ModUiColor color)
        {
            if (material != null) material.color = new Color32(color.R, color.G, color.B, color.A);
        }
        public void SetRect(ModArenaRect rect)
        {
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            rectangle = rect; RebuildGeometry();
        }
        public void SetSprite(AssetId asset)
        {
            var sprite = ModRuntime.Host?.TypedAssets?.LoadSprite(asset);
            if (sprite == null) throw new InvalidOperationException("Arena sprite is unavailable: " + asset);
            var vertices = sprite.vertices; var uv = sprite.uv; var indices = sprite.triangles;
            if (vertices.Length < 3 || vertices.Length > 2048 || uv.Length != vertices.Length || indices.Length == 0)
                throw new InvalidOperationException("Arena sprite requires 3..2048 native vertices and matching UVs.");
            var normalized = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                normalized[i] = new Vector2((vertices[i].x * sprite.pixelsPerUnit + sprite.pivot.x) / sprite.rect.width,
                    1 - (vertices[i].y * sprite.pixelsPerUnit + sprite.pivot.y) / sprite.rect.height);
            var converted = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++) converted[i] = indices[i];
            shape = normalized; textureCoordinates = uv; triangles = converted;
            material.mainTexture = sprite.texture; RebuildGeometry();
        }
        private void RebuildGeometry()
        {
            var vertices = new Vector3[shape.Length];
            for (int i = 0; i < vertices.Length; i++) vertices[i] = new Vector3(
                (float)(rectangle.X + shape[i].x * rectangle.Width), (float)(rectangle.Y + shape[i].y * rectangle.Height), -.25f);
            var colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            mesh.Clear(); mesh.vertices = vertices; mesh.colors = colors; mesh.uv = textureCoordinates; mesh.triangles = triangles; mesh.RecalculateBounds();
        }
        private void LateUpdate() { if (IsActive) Refresh(); }
        private void Refresh()
        {
            var source = coordinates();
            transform.SetParent(source.parent, false);
            transform.localPosition = source.localPosition; transform.localRotation = source.localRotation;
            transform.localScale = source.localScale; gameObject.layer = source.gameObject.layer;
        }
        public void Dispose()
        {
            if (closed) return;
            closed = true; markers.Remove(this); alive = null; coordinates = null;
            if (material != null) Destroy(material); if (mesh != null) Destroy(mesh);
            material = null; mesh = null;
            if (gameObject != null) { gameObject.SetActive(false); Destroy(gameObject); }
        }
        private void OnDestroy() { Dispose(); }
    }
}
