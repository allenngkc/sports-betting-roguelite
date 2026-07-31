using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR
{
    /// <summary>
    /// Procedural chamfered-box mesh generator for the room art pass (direction B, Vice Grip).
    ///
    /// Every renderable object in the graybox room is a Unity primitive cube, which is why the
    /// room reads as flat and untouched: a perfectly sharp 90-degree edge catches no light, so
    /// nothing separates one form from another. A chamfer gives every edge a narrow bevel face
    /// angled between its two neighbours, which picks up a highlight and reads as thickness -
    /// the "stylise through form, not through flatness" rule from the approved direction.
    ///
    /// Meshes are generated at TRUE WORLD SIZE with localScale left at 1, not scaled unit cubes.
    /// That matters: scaling a unit cube stretches its bevel, so a thin wall and a chunky bunk
    /// post would end up with visibly different edge widths. Generating per size keeps the
    /// chamfer a constant physical width across the whole room.
    ///
    /// UVs are world-scale planar per face (1 unit = 1 UV), giving uniform texel density on
    /// every surface for free - the standard Phase 4 texturing needs.
    ///
    /// Meshes are cached as assets keyed by (size, bevel), so identical objects such as the four
    /// desk legs share one mesh and rebuilds are deterministic.
    /// </summary>
    public static class ChamferedBoxMesh
    {
        private const string MeshFolder = "Assets/SBR/Environment/Meshes";

        /// <summary>
        /// Bevel scaled to the object's smallest dimension, so the phone chassis gets a tight
        /// chamfer and the couch gets a chunky one without any per-call-site tuning.
        /// </summary>
        public static float DefaultBevel(Vector3 size)
        {
            // Tuning pass: 0.12 / 0.03m cap read as a thin highlight rather than chunk at the
            // standing camera's 68deg from ~3m. Raised so the large forms - couch, stool, bunk
            // slab - carry a bevel wide enough to read as a face, which is what sells the
            // heavy stylised silhouette. Small electronics stay tight via the floor clamp.
            float min = Mathf.Min(size.x, Mathf.Min(size.y, size.z));
            return Mathf.Clamp(min * 0.18f, 0.004f, 0.050f);
        }

        public static Mesh GetOrCreate(Vector3 size, float bevel)
        {
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            // Never let the bevel eat the face it is cutting into.
            float maxBevel = Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.45f;
            bevel = Mathf.Clamp(bevel, 0.001f, Mathf.Max(0.001f, maxBevel));

            if (!AssetDatabase.IsValidFolder(MeshFolder))
                AssetDatabase.CreateFolder("Assets/SBR/Environment", "Meshes");

            string path = $"{MeshFolder}/{Key(size, bevel)}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            Mesh mesh = Build(size, bevel);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        /// <summary>
        /// R7. A flat quad for wear decals, cached as an asset like the boxes.
        ///
        /// Matches Unity's own Quad convention - it renders on its local -Z side - so callers can
        /// keep using <c>LookRotation(-facing, up)</c> and the normal ends up pointing along
        /// <c>facing</c>. Local +X is the run direction, local +Y is the height.
        ///
        /// UVs carry an EXPLICIT repeat count rather than the 0..1 of Unity's primitive. Wear
        /// runs vary in length - the long walls are 4.0m and the short ones 2.6m - and with 0..1
        /// UVs a single shared material would stretch the same texture across both, so the dirt
        /// would visibly change scale at the corner where they meet. Baking repeats into the mesh
        /// keeps texel density authored per run and leaves one material serving every wall.
        /// </summary>
        public static Mesh GetOrCreateQuad(Vector2 size, Vector2 uvRepeats)
        {
            size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

            if (!AssetDatabase.IsValidFolder(MeshFolder))
                AssetDatabase.CreateFolder("Assets/SBR/Environment", "Meshes");

            string F(float v) => Mathf.RoundToInt(v * 10000f).ToString();
            string path = $"{MeshFolder}/quad_{F(size.x)}_{F(size.y)}" +
                          $"_u{F(uvRepeats.x)}_v{F(uvRepeats.y)}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            float hx = size.x * 0.5f, hy = size.y * 0.5f;
            var mesh = new Mesh
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path),
                vertices = new[]
                {
                    new Vector3(-hx, -hy, 0f), // 0 bottom-left
                    new Vector3( hx, -hy, 0f), // 1 bottom-right
                    new Vector3(-hx,  hy, 0f), // 2 top-left
                    new Vector3( hx,  hy, 0f), // 3 top-right
                },
                normals = new[] { -Vector3.forward, -Vector3.forward,
                                  -Vector3.forward, -Vector3.forward },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(uvRepeats.x, 0f),
                    new Vector2(0f, uvRepeats.y),
                    new Vector2(uvRepeats.x, uvRepeats.y),
                },
                // Viewed from -Z with +Y up, world +X is screen-right, so clockwise - Unity's
                // front-face winding - runs top-left, top-right, bottom-right.
                triangles = new[] { 2, 3, 1, 2, 1, 0 },
            };
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static string Key(Vector3 s, float b)
        {
            string F(float v) => Mathf.RoundToInt(v * 10000f).ToString();
            return $"cbox_{F(s.x)}_{F(s.y)}_{F(s.z)}_b{F(b)}";
        }

        private static Mesh Build(Vector3 size, float b)
        {
            Vector3 h = size * 0.5f;
            var verts = new List<Vector3>(96);
            var norms = new List<Vector3>(96);
            var uvs = new List<Vector2>(96);
            var tris = new List<int>(132);

            // Three vertices per corner, one pulled out to each of the corner's three faces.
            Vector3 P(int ax, int sx, int sy, int sz) => ax switch
            {
                0 => new Vector3(sx * h.x, sy * (h.y - b), sz * (h.z - b)),
                1 => new Vector3(sx * (h.x - b), sy * h.y, sz * (h.z - b)),
                _ => new Vector3(sx * (h.x - b), sy * (h.y - b), sz * h.z),
            };

            int[] s = { -1, 1 };
            // Ring order around a face, so the four corners form a quad rather than a bowtie.
            int[,] ring = { { -1, -1 }, { -1, 1 }, { 1, 1 }, { 1, -1 } };

            // ---- 6 inset faces
            foreach (int sign in s)
            {
                // +/-X : varies (y, z)
                AddQuad(verts, norms, uvs, tris, new Vector3(sign, 0, 0),
                    P(0, sign, ring[0, 0], ring[0, 1]), P(0, sign, ring[1, 0], ring[1, 1]),
                    P(0, sign, ring[2, 0], ring[2, 1]), P(0, sign, ring[3, 0], ring[3, 1]));

                // +/-Y : varies (x, z)
                AddQuad(verts, norms, uvs, tris, new Vector3(0, sign, 0),
                    P(1, ring[0, 0], sign, ring[0, 1]), P(1, ring[1, 0], sign, ring[1, 1]),
                    P(1, ring[2, 0], sign, ring[2, 1]), P(1, ring[3, 0], sign, ring[3, 1]));

                // +/-Z : varies (x, y)
                AddQuad(verts, norms, uvs, tris, new Vector3(0, 0, sign),
                    P(2, ring[0, 0], ring[0, 1], sign), P(2, ring[1, 0], ring[1, 1], sign),
                    P(2, ring[2, 0], ring[2, 1], sign), P(2, ring[3, 0], ring[3, 1], sign));
            }

            // ---- 12 edge chamfers, each bridging the two faces it sits between
            foreach (int a in s)
            foreach (int c in s)
            {
                // Edge running along X, fixed (y=a, z=c): bridges the Y face and the Z face.
                AddQuad(verts, norms, uvs, tris, new Vector3(0, a, c).normalized,
                    P(1, -1, a, c), P(1, 1, a, c), P(2, 1, a, c), P(2, -1, a, c));

                // Edge running along Y, fixed (x=a, z=c): bridges the X face and the Z face.
                AddQuad(verts, norms, uvs, tris, new Vector3(a, 0, c).normalized,
                    P(0, a, -1, c), P(0, a, 1, c), P(2, a, 1, c), P(2, a, -1, c));

                // Edge running along Z, fixed (x=a, y=c): bridges the X face and the Y face.
                AddQuad(verts, norms, uvs, tris, new Vector3(a, c, 0).normalized,
                    P(0, a, c, -1), P(0, a, c, 1), P(1, a, c, 1), P(1, a, c, -1));
            }

            // ---- 8 corner triangles
            foreach (int sx in s)
            foreach (int sy in s)
            foreach (int sz in s)
            {
                AddTri(verts, norms, uvs, tris, new Vector3(sx, sy, sz).normalized,
                    P(0, sx, sy, sz), P(1, sx, sy, sz), P(2, sx, sy, sz));
            }

            var mesh = new Mesh { name = Key(size, b) };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<Vector3> v, List<Vector3> n, List<Vector2> uv,
                                    List<int> t, Vector3 normal,
                                    Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            EnsureWinding(normal, ref a, ref b, ref c, ref d);
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            for (int k = 0; k < 4; k++) n.Add(normal);
            uv.Add(PlanarUv(a, normal)); uv.Add(PlanarUv(b, normal));
            uv.Add(PlanarUv(c, normal)); uv.Add(PlanarUv(d, normal));
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
            t.Add(i); t.Add(i + 2); t.Add(i + 3);
        }

        private static void AddTri(List<Vector3> v, List<Vector3> n, List<Vector2> uv,
                                   List<int> t, Vector3 normal, Vector3 a, Vector3 b, Vector3 c)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f)
                (a, c) = (c, a);
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            for (int k = 0; k < 3; k++) n.Add(normal);
            uv.Add(PlanarUv(a, normal)); uv.Add(PlanarUv(b, normal)); uv.Add(PlanarUv(c, normal));
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }

        /// <summary>
        /// Unity front faces wind so that Cross(b-a, c-a) points along the face normal; flip the
        /// ring if this quad came out the other way round rather than hand-ordering all 18 of them.
        /// </summary>
        private static void EnsureWinding(Vector3 normal, ref Vector3 a, ref Vector3 b,
                                          ref Vector3 c, ref Vector3 d)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) >= 0f)
                return;
            (a, b, c, d) = (d, c, b, a);
        }

        /// <summary>World-scale planar UV: 1 world unit = 1 UV unit on every surface.</summary>
        private static Vector2 PlanarUv(Vector3 p, Vector3 n)
        {
            float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
            if (ax >= ay && ax >= az) return new Vector2(p.z, p.y);
            if (ay >= az) return new Vector2(p.x, p.z);
            return new Vector2(p.x, p.y);
        }
    }
}
