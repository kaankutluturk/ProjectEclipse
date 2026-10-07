using System;
using System.Collections.Generic;
using Nekki.SF2.Core.Fights;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Multiplayer
{
    /// <summary>
    /// A live fighter wearing a versus loadout, idling, drawn into a UI RawImage. The
    /// fighter is built with the same menu model path as the profile screen
    /// (<see cref="ModelContainer.ShowParameters"/>) far away from any scene content, and a
    /// private camera frames whatever it renders, so the model's world scale never matters.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class VersusFighterPreview : MonoBehaviour
    {
        private const float FarX = -60000f;
        private const float RebuildDelay = .12f;
        private static int _nextSlot;
        /// <summary>The game's shadow-fighter ink; reads well on paper cards.</summary>
        public static readonly Color DefaultTint = new Color32(40, 20, 9, 255);

        private RawImage _image;
        private GameObject _world;
        private ModelContainer _container;
        private UnityEngine.Camera _camera;
        private RenderTexture _texture;
        private bool _persistForLoading;
        private VersusLoadout _shown, _wanted;
        private float _rebuildAt = -1f;
        private int _slot;
        private bool _mirror;
        private Color _tint = DefaultTint;
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private float _nextRendererScan;
        private Vector3 _center;
        private float _size = -1f;

        public VersusLoadout Loadout => _wanted ?? _shown;

        /// <summary>Adds a preview filling <paramref name="parent"/>; <paramref name="mirror"/> faces it left.</summary>
        public static VersusFighterPreview Create(RectTransform parent, bool mirror, Color? tint = null)
        {
            var rect = new GameObject("Fighter preview", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = rect.gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            image.color = new Color(1, 1, 1, 0);
            var preview = rect.gameObject.AddComponent<VersusFighterPreview>();
            preview._image = image;
            preview._mirror = mirror;
            if (tint.HasValue) preview._tint = tint.Value;
            preview._slot = _nextSlot++ % 64;
            image.uvRect = mirror ? new Rect(1, 0, -1, 1) : new Rect(0, 0, 1, 1);
            return preview;
        }

        /// <summary>Shows <paramref name="loadout"/>; quick successive calls rebuild once.</summary>
        public void Show(VersusLoadout loadout, bool immediately = false)
        {
            if (loadout == null) return;
            _wanted = loadout;
            if (_shown != null && _shown.Equals(loadout)) { _wanted = null; return; }
            _rebuildAt = immediately ? 0f : Time.unscaledTime + RebuildDelay;
        }

        private void Rebuild()
        {
            var loadout = _wanted;
            _wanted = null;
            _rebuildAt = -1f;
            if (loadout == null) return;
            try
            {
                EnsureWorld();
                var parameters = LocalVersusMatch.PrepareFighter(loadout, true, string.Empty);
                _container.ShowParameters(parameters, StageType.Stage.STAGE_SHOP_START, "Profile", _tint);
                _shown = loadout;
                _renderers.Clear();
                _nextRendererScan = 0f;
                _size = -1f;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Versus] Fighter preview for " + loadout + " failed: " + exception.Message);
            }
        }

        private void EnsureWorld()
        {
            if (_world != null) return;
            var origin = new Vector3(FarX - _slot * 500f, 0f, 0f);
            _world = new GameObject("Versus fighter preview " + _slot);
            _world.transform.position = origin;
            if (_persistForLoading) DontDestroyOnLoad(_world);
            _container = _world.AddComponent<ModelContainer>();
            _container.Init();
            var host = new GameObject("Versus fighter preview camera " + _slot);
            host.transform.SetParent(_world.transform, false);
            host.transform.localPosition = new Vector3(0, 0, -100f);
            _camera = host.AddComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 3f;
            _camera.nearClipPlane = .1f;
            _camera.farClipPlane = 600f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0, 0, 0, 0);
            _camera.depth = -99;
            _camera.allowHDR = false;
            var rect = ((RectTransform)transform).rect;
            int width = Mathf.Clamp(Mathf.RoundToInt(rect.width * 1.5f), 128, 1024), height = Mathf.Clamp(Mathf.RoundToInt(rect.height * 1.5f), 128, 1024);
            _texture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32) { name = "Versus fighter preview", antiAliasing = 4 };
            _texture.Create();
            _camera.targetTexture = _texture;
            _camera.aspect = (float)width / height;
            _image.texture = _texture;
        }

        private void LateUpdate()
        {
            if (_rebuildAt >= 0f && Time.unscaledTime >= _rebuildAt) Rebuild();
            if (_container != null) { _container.SyncPreviewFreeze(); _container.KeepPreviewWalls(_wallLeft, _wallRight); }
            UpdateEdgeLines();
            if (_camera == null) return;
            if (Time.unscaledTime >= _nextRendererScan)
            {
                _nextRendererScan = Time.unscaledTime + .5f;
                _renderers.Clear();
                _world.GetComponentsInChildren(false, _renderers);
                _renderers.RemoveAll(renderer => renderer == null || renderer.gameObject == _camera.gameObject);
            }
            if (!TryBounds(out var bounds)) return;
            // Frame the pose; grow at once, shrink slowly, so attacks never jitter the framing.
            float needed = Mathf.Max(bounds.extents.y, bounds.extents.x / Mathf.Max(.1f, _camera.aspect)) * 1.18f;
            _size = _size < 0 ? needed : needed > _size ? needed : Mathf.Lerp(_size, needed, 1f - Mathf.Exp(-Time.unscaledDeltaTime * .8f));
            var center = bounds.center;
            _center = _center == Vector3.zero ? center : Vector3.Lerp(_center, center, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 4f));
            _camera.orthographicSize = Mathf.Max(.01f, _size);
            _camera.transform.position = new Vector3(_center.x, _center.y - _size * .04f, bounds.min.z - 50f);
            float alpha = Mathf.MoveTowards(_image.color.a, 1f, Time.unscaledDeltaTime * 4f);
            _image.color = new Color(1, 1, 1, alpha);
        }

        private bool TryBounds(out Bounds bounds)
        {
            bounds = default;
            bool any = false;
            foreach (var renderer in _renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                var b = renderer.bounds;
                if (b.size.sqrMagnitude <= 0f || float.IsNaN(b.center.x)) continue;
                if (!any) { bounds = b; any = true; }
                else bounds.Encapsulate(b);
            }
            return any;
        }

        // ---- Moveset Lab playback ----

        private readonly Dictionary<ModelEdge, LineRenderer> _edgeLines = new Dictionary<ModelEdge, LineRenderer>();
        private Material _lineMaterial;
        /// <summary>Draws the playing move's active attacking edges as red capsules.</summary>
        public bool ShowAttackEdges { get; set; }

        /// <summary>Rebuilds the fighter even for the same loadout (after move edits change availability).</summary>
        public void Refresh() { if (Loadout == null) return; _wanted = Loadout; _shown = null; _rebuildAt = 0f; }

        public bool IsReady => _container != null && _container.PreviewModel != null;
        public bool Paused { get => _container != null && _container.PreviewPaused; set { if (_container != null) _container.PreviewPaused = value; } }

        /// <summary>Starts <paramref name="move"/> on the fighter. False when the fighter cannot use it.</summary>
        public bool PlayMove(string move)
        {
            if (!IsReady) return false;
            var animation = AnimationData.GetAnimationByName(move, false);
            if (animation == null) return false;
            _container.PreviewModel.PlayAnimationDelay(animation);
            // Apply the pending request so a paused preview shows the first frame.
            _container.PreviewStep(1);
            return true;
        }

        public void Step(int ticks) { if (IsReady) _container.PreviewStep(Mathf.Max(0, ticks)); }

        private const int MaxSeekTicks = 2000;

        /// <summary>
        /// Shows <paramref name="move"/> at keyframe <paramref name="frame"/>, paused. The move
        /// replays from the fighter's start position and is simulated tick by tick up to the
        /// frame, so the pose (and any travel) is exactly what normal playback reaches there.
        /// Returns the ticks simulated, or -1 when the fighter cannot play the move.
        /// </summary>
        public int SeekMove(string move, int frame)
        {
            if (!IsReady) return -1;
            var animation = AnimationData.GetAnimationByName(move, false);
            if (animation == null) return -1;
            Paused = true;
            ResetPosition();
            _container.PreviewModel.PlayAnimationDelay(animation);
            _container.PreviewStep(1);
            int ticks = 1;
            while (ticks < MaxSeekTicks && PlayingMove == animation.Name && Keyframe < frame) { _container.PreviewStep(1); ticks++; }
            return PlayingMove == animation.Name ? ticks : -1;
        }

        /// <summary>
        /// Simulates until the playing move reaches its next keyframe. Returns the ticks taken,
        /// or -1 when the move ended (the fighter moved on to another move) first.
        /// </summary>
        public int StepKeyframe()
        {
            if (!IsReady) return -1;
            string move = PlayingMove;
            int start = Keyframe;
            for (int ticks = 1; ticks <= 64; ticks++)
            {
                _container.PreviewStep(1);
                if (PlayingMove != move) return -1;
                if (Keyframe != start) return ticks;
            }
            return -1;
        }

        /// <summary>Puts the fighter back on its start position (moves that travel otherwise drift on replay).</summary>
        public void ResetPosition()
        {
            if (!IsReady) return;
            _container.ResetModelPosition();
            PlaceWalls();
        }

        private const float OpenWall = 4000f;
        private float? _wallBehind;
        private float _wallLeft = -OpenWall, _wallRight = OpenWall;

        /// <summary>
        /// Puts a wall <paramref name="gap"/> units behind the fighter's start position, or none
        /// (null). The game's own wall rules then decide wall hits, as against an arena wall.
        /// </summary>
        public void SetWallBehind(float? gap)
        {
            _wallBehind = gap;
            PlaceWalls();
        }

        private void PlaceWalls()
        {
            _wallLeft = -OpenWall; _wallRight = OpenWall;
            if (IsReady && _wallBehind.HasValue)
            {
                var model = _container.PreviewModel;
                float x = model.Body.GetCenterOfMassNode().GetStart().GetX();
                if (model.GetFacingSign() >= 0) _wallLeft = x - _wallBehind.Value;
                else _wallRight = x + _wallBehind.Value;
            }
            if (IsReady) _container.KeepPreviewWalls(_wallLeft, _wallRight);
        }

        public string PlayingMove => IsReady ? _container.PreviewModel.GetCurrentAnimation()?.Name : null;
        /// <summary>Ticks since the playing move started.</summary>
        public int MoveTick => IsReady ? _container.PreviewModel.GetAnimationModule().GetFrameInMove() : 0;
        /// <summary>Current keyframe (the frame numbers intervals use).</summary>
        public int Keyframe => IsReady && _container.PreviewModel.GetAnimationModule().GetCurrentInfo() != null ? _container.PreviewModel.GetAnimationModule().GetCurrentFrame() : 0;

        /// <summary>Names of the fighter's edges (body and weapon), for the hitbox editor.</summary>
        public List<string> EdgeNames()
        {
            var names = new List<string>();
            if (!IsReady) return names;
            foreach (var edge in _container.PreviewModel.Body.GetAllEdges()) if (!string.IsNullOrEmpty(edge.get_Name()) && !names.Contains(edge.get_Name())) names.Add(edge.get_Name());
            return names;
        }

        /// <summary>An edge of the fighter: body edges join two skeleton (N*) nodes; the rest are weapon or rig parts.</summary>
        public struct EdgeInfo { public string Name; public float Radius; public bool Body; }

        /// <summary>Every named edge of the fighter, with what the hitbox editor groups them by.</summary>
        public List<EdgeInfo> Edges()
        {
            var result = new List<EdgeInfo>();
            if (!IsReady) return result;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in _container.PreviewModel.Body.GetAllEdges())
            {
                string name = edge.get_Name();
                if (string.IsNullOrEmpty(name) || !seen.Add(name)) continue;
                string from = edge.FromNode?.GetName() ?? string.Empty, to = edge.ToNode?.GetName() ?? string.Empty;
                bool body = from.StartsWith("N", StringComparison.Ordinal) && to.StartsWith("N", StringComparison.Ordinal) && from.IndexOf('-') < 0 && to.IndexOf('-') < 0;
                result.Add(new EdgeInfo { Name = name, Radius = edge.CollisionRadius, Body = body });
            }
            return result;
        }

        /// <summary>
        /// When set, draws every edge it gives a colour (instead of only the playing move's
        /// active attacking edges), for the hitbox editor.
        /// </summary>
        public Func<string, Color?> EdgeOverlay { get; set; }

        /// <summary>The drawn edge nearest a screen point over this preview, or null when none is within <paramref name="maxPixels"/>.</summary>
        public string EdgeAt(Vector2 screenPoint, UnityEngine.Camera uiCamera, float maxPixels)
        {
            if (!IsReady || _camera == null) return null;
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, uiCamera, out var local)) return null;
            var size = rect.rect.size;
            if (size.x <= 0 || size.y <= 0) return null;
            var uv = new Vector2((local.x - rect.rect.xMin) / size.x, (local.y - rect.rect.yMin) / size.y);
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return null;
            if (_mirror) uv.x = 1f - uv.x;
            var model = _container.PreviewModel;
            var root = model.UnityObject.transform;
            var p = new Vector2(uv.x * size.x, uv.y * size.y);
            string best = null;
            float bestDistance = maxPixels;
            foreach (var edge in model.Body.GetAllEdges())
            {
                string name = edge.get_Name();
                if (string.IsNullOrEmpty(name) || EdgeOverlay != null && EdgeOverlay(name) == null) continue;
                edge.UpdateCollisionPoints();
                var a = Pixels(root, edge.CollisionStart, size); var b = Pixels(root, edge.CollisionEnd, size);
                var ab = b - a;
                float t = ab.sqrMagnitude < 1e-4f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                float distance = Vector2.Distance(p, a + ab * t);
                if (distance < bestDistance) { bestDistance = distance; best = name; }
            }
            return best;
        }

        private Vector2 Pixels(Transform root, Vector3f point, Vector2 size)
        {
            var viewport = _camera.WorldToViewportPoint(root.TransformPoint(new Vector3(point.GetX(), point.GetY(), -1.6f)));
            return new Vector2(viewport.x * size.x, viewport.y * size.y);
        }

        private LineRenderer EdgeLine(Model model, ModelEdge edge)
        {
            if (_edgeLines.TryGetValue(edge, out var line) && line != null) return line;
            var host = new GameObject("Lab edge " + edge.get_Name());
            host.transform.SetParent(model.UnityObject.transform, false);
            line = host.AddComponent<LineRenderer>();
            if (_lineMaterial == null) _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial = _lineMaterial; line.useWorldSpace = false; line.positionCount = 2; line.numCapVertices = 8;
            line.alignment = LineAlignment.TransformZ; line.sortingOrder = 32760;
            _edgeLines[edge] = line;
            return line;
        }

        private void UpdateEdgeLines()
        {
            var active = new HashSet<ModelEdge>();
            if (EdgeOverlay != null && IsReady)
            {
                var model = _container.PreviewModel;
                foreach (var edge in model.Body.GetAllEdges())
                {
                    string name = edge.get_Name();
                    var color = string.IsNullOrEmpty(name) ? null : EdgeOverlay(name);
                    if (color == null) continue;
                    active.Add(edge);
                    edge.UpdateCollisionPoints();
                    var line = EdgeLine(model, edge);
                    line.startColor = line.endColor = color.Value;
                    var start = edge.CollisionStart; var end = edge.CollisionEnd;
                    line.SetPosition(0, new Vector3(start.GetX(), start.GetY(), -1.6f));
                    line.SetPosition(1, new Vector3(end.GetX(), end.GetY(), -1.6f));
                    line.startWidth = line.endWidth = Mathf.Max(.035f, edge.CollisionRadius * 2f);
                    line.enabled = true;
                }
            }
            else if (ShowAttackEdges && IsReady)
            {
                var model = _container.PreviewModel;
                var edges = model.GetAnimationModule()?.GetAttackingEdges();
                if (edges != null)
                    foreach (var edge in edges)
                    {
                        active.Add(edge);
                        var line = EdgeLine(model, edge);
                        line.startColor = line.endColor = new Color(1f, .22f, .18f, .92f);
                        var start = edge.CollisionStart; var end = edge.CollisionEnd;
                        line.SetPosition(0, new Vector3(start.GetX(), start.GetY(), -1.6f));
                        line.SetPosition(1, new Vector3(end.GetX(), end.GetY(), -1.6f));
                        line.startWidth = line.endWidth = Mathf.Max(.04f, edge.CollisionRadius * 2f);
                        line.enabled = true;
                    }
            }
            foreach (var pair in _edgeLines) if (pair.Value != null && !active.Contains(pair.Key)) pair.Value.enabled = false;
        }

        /// <summary>Keeps the live model and camera through the fight scene transition.</summary>
        internal void KeepAliveDuringLoading()
        {
            _persistForLoading = true;
            // ModelContainer's render layer, fighter models and camera are children
            // of this root. Preserve their normal update/render path while the UI
            // remains visible, then destroy them with their owning preview.
            if (_world != null) DontDestroyOnLoad(_world);
        }

        private void OnEnable()
        {
            if (_world != null) _world.SetActive(true);
        }

        private void OnDisable()
        {
            // The persistent world does not inherit the menu's active state.
            // Stop rendering and ticking as soon as the fight replaces the splash.
            if (_world != null) _world.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_world != null) Destroy(_world);
            if (_texture != null) { _texture.Release(); Destroy(_texture); }
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }
    }
}
