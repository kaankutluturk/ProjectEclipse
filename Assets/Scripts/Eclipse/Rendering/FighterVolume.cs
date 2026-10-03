using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eclipse.Rendering
{
    // Presentation geometry only. No colliders or authoritative rig changes.
    public sealed class FighterVolume : MonoBehaviour
    {
        Mesh mesh;
        MeshRenderer draw;
        MaterialPropertyBlock tint;
        static Material material;
        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;
        const int Sides = 20, Rings = 12;

        static Vector3 SurfacePoint(Vector3 point)
        {
            // Recovered panels were authored to overlap in the flattened view.
            // Attenuate their depth separation while retaining animated XYZ;
            // capsule radius still provides full solid cross-section depth.
            point.z *= .35f;
            return point;
        }

        public static FighterVolume Create(Transform parent)
        {
            var obj = new GameObject("Experimental 3D volume");
            obj.layer = ExperimentalFighterCamera.Layer;
            obj.transform.SetParent(parent, false);
            var result = obj.AddComponent<FighterVolume>();
            result.mesh = new Mesh { name = "Eclipse dynamic fighter volume" };
            result.mesh.MarkDynamic();
            obj.AddComponent<MeshFilter>().sharedMesh = result.mesh;
            result.draw = obj.AddComponent<MeshRenderer>();
            if (material == null)
            {
                var shader = Resources.Load<Shader>("shaders/EclipseFighterVolume");
                if (shader == null) { Destroy(obj); return null; }
                material = new Material(shader);
            }
            result.draw.sharedMaterial = material;
            result.draw.shadowCastingMode = ShadowCastingMode.Off;
            result.draw.receiveShadows = false;
            result.draw.lightProbeUsage = LightProbeUsage.Off;
            result.draw.reflectionProbeUsage = ReflectionProbeUsage.Off;
            result.tint = new MaterialPropertyBlock();
            return result;
        }

        void Colorize(Color color)
        {
            // Black silhouettes need a visible surface to reveal depth. Keep perk
            // and arena colour influence rather than editing shared materials.
            color = Color.Lerp(new Color(.32f, .38f, .44f, color.a), color, .5f);
            tint.SetColor("_Color", color);
            draw.SetPropertyBlock(tint);
        }

        public void Surface(Vector3[] source, int[] faces, Color color)
        {
            int count = faces.Length * 8;
            if (vertices == null || vertices.Length != count)
            {
                vertices = new Vector3[count]; triangles = new int[count];
                for (int i = 0; i < count; i++) triangles[i] = i;
                mesh.Clear();
            }
            var depth = new Vector3(0, 0, 6f);
            int at = 0;
            for (int i = 0; i < faces.Length; i += 3)
            {
                Vector3 a = SurfacePoint(source[faces[i]]), b = SurfacePoint(source[faces[i + 1]]), c = SurfacePoint(source[faces[i + 2]]);
                Triangle(ref at, a - depth, b - depth, c - depth);
                Triangle(ref at, c + depth, b + depth, a + depth);
                Wall(ref at, a, b, depth); Wall(ref at, b, c, depth); Wall(ref at, c, a, depth);
            }
            mesh.vertices = vertices; mesh.triangles = triangles;
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); Colorize(color);
        }
        void Triangle(ref int at, Vector3 a, Vector3 b, Vector3 c)
        { vertices[at++] = a; vertices[at++] = b; vertices[at++] = c; }
        void Wall(ref int at, Vector3 a, Vector3 b, Vector3 d)
        { Triangle(ref at, a - d, a + d, b + d); Triangle(ref at, a - d, b + d, b - d); }

        public void Capsule(Vector3 start, Vector3 end, float diameter, Color color)
        {
            start = SurfacePoint(start); end = SurfacePoint(end);
            if (vertices == null || vertices.Length != Sides * Rings)
            {
                vertices = new Vector3[Sides * Rings]; normals = new Vector3[vertices.Length];
                var indices = new List<int>();
                for (int r = 0; r < Rings - 1; r++)
                    for (int s = 0; s < Sides; s++)
                    {
                        int a = r * Sides + s, b = r * Sides + (s + 1) % Sides;
                        indices.Add(a); indices.Add(a + Sides); indices.Add(b);
                        indices.Add(b); indices.Add(a + Sides); indices.Add(b + Sides);
                    }
                triangles = indices.ToArray(); mesh.Clear();
            }
            Vector3 axis = end - start;
            float length = axis.magnitude, radius = Mathf.Max(.01f, diameter * .5f);
            var rotation = length > .0001f ? Quaternion.FromToRotation(Vector3.up, axis / length) : Quaternion.identity;
            for (int r = 0; r < Rings; r++)
            {
                bool upper = r >= Rings / 2;
                float latitude = upper ? (r - Rings / 2) * Mathf.PI / (Rings - 2) : -Mathf.PI * .5f + r * Mathf.PI / (Rings - 2);
                float y = Mathf.Sin(latitude), ring = Mathf.Cos(latitude);
                for (int s = 0; s < Sides; s++)
                {
                    float angle = s * Mathf.PI * 2 / Sides;
                    Vector3 normal = new Vector3(ring * Mathf.Cos(angle), y, ring * Mathf.Sin(angle));
                    int i = r * Sides + s;
                    normals[i] = rotation * normal;
                    vertices[i] = start + rotation * (normal * radius + (upper ? Vector3.up * length : Vector3.zero));
                }
            }
            mesh.vertices = vertices; mesh.normals = normals; mesh.triangles = triangles;
            mesh.RecalculateBounds(); Colorize(color);
        }

        void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
