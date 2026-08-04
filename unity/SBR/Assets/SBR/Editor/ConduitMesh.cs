using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR
{
    /// <summary>
    /// Procedural tube-mesh generator for the ceiling/wall conduit and cable-bundle art pass.
    /// Two flavours share one tube builder: a rigid <see cref="GetOrCreateTube"/> for stapled
    /// conduit runs, and a slack <see cref="GetOrCreateSaggingCable"/> that pre-sags the polyline
    /// so bundles read as hanging between fixings rather than snapped taut.
    ///
    /// Points are authored in the caller's LOCAL space and the mesh is built in that same space -
    /// the GameObject is expected to sit at the origin. Meshes are cached as assets keyed by
    /// <paramref name="key"/>, so calling either method twice with the same key is idempotent
    /// and never rebuilds.
    /// </summary>
    public static class ConduitMesh
    {
        private const string MeshFolder = "Assets/SBR/Environment/Meshes";

        public static Mesh GetOrCreateTube(string key, IList<Vector3> points, float radius, int sides)
        {
            string path = AssetPath(key);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            ValidatePoints(points);
            Mesh mesh = BuildTubeMesh(points, radius, sides);
            return SaveAsset(mesh, key, path);
        }

        public static Mesh GetOrCreateSaggingCable(string key, IList<Vector3> points, float radius,
            int sides, float sagAmount, int segmentsPerSpan)
        {
            string path = AssetPath(key);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            ValidatePoints(points);
            if (segmentsPerSpan < 1)
                throw new System.InvalidOperationException("ConduitMesh: segmentsPerSpan must be >= 1.");

            List<Vector3> dense = BuildSaggingPolyline(points, sagAmount, segmentsPerSpan);
            Mesh mesh = BuildTubeMesh(dense, radius, sides);
            return SaveAsset(mesh, key, path);
        }

        // ---- asset cache ----------------------------------------------------------------

        private static string AssetPath(string key) => $"{MeshFolder}/conduit_{key}.asset";

        private static Mesh SaveAsset(Mesh mesh, string key, string path)
        {
            mesh.name = $"conduit_{key}";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            return mesh;
        }

        private static void ValidatePoints(IList<Vector3> points)
        {
            if (points == null || points.Count < 2)
                throw new System.InvalidOperationException(
                    "ConduitMesh: at least 2 points are required to build a tube.");

            for (int i = 1; i < points.Count; i++)
            {
                if ((points[i] - points[i - 1]).sqrMagnitude < 1e-8f)
                    throw new System.InvalidOperationException(
                        $"ConduitMesh: consecutive duplicate points at index {i - 1}/{i}.");
            }
        }

        // ---- sagging polyline -------------------------------------------------------------

        /// <summary>
        /// Subdivides each span into <paramref name="segmentsPerSpan"/> samples and pulls each
        /// one down by a catenary-like parabola (sag = sagAmount * 4t(1-t)), so the resulting
        /// denser polyline reads as slack cable rather than a straight rigid run once tubed.
        /// </summary>
        private static List<Vector3> BuildSaggingPolyline(IList<Vector3> points, float sagAmount, int segmentsPerSpan)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[i + 1];
                // Span i > 0 skips t=0: it is identical to the previous span's t=1 sample.
                int startSeg = i == 0 ? 0 : 1;
                for (int s = startSeg; s <= segmentsPerSpan; s++)
                {
                    float t = (float)s / segmentsPerSpan;
                    Vector3 p = Vector3.Lerp(a, b, t);
                    p.y -= sagAmount * 4f * t * (1f - t);
                    result.Add(p);
                }
            }
            return result;
        }

        // ---- rotation-minimising frames -----------------------------------------------------

        /// <summary>
        /// One (forward, normal, binormal) frame per path point. Interior points use the
        /// bisector of their incoming/outgoing directions as forward, so the ring at a corner
        /// sits on the joint's miter plane instead of gapping or pinching.
        ///
        /// The normal/binormal are NOT re-derived from a fixed world "up" at every point (a
        /// naive Quaternion.LookRotation(forward, Vector3.up) flips the instant forward passes
        /// through vertical - which happens constantly here, since runs go along the ceiling
        /// and straight down a wall). Instead each frame is parallel-transported from the
        /// previous one: rotate the previous normal by the minimal rotation that takes the
        /// previous forward onto the new forward, then re-orthogonalise against the new forward
        /// to cancel floating-point drift. This carries orientation continuously along the path
        /// with no twist and no flip.
        /// </summary>
        private static void ComputeFrames(IList<Vector3> points, Vector3[] forward, Vector3[] normal, Vector3[] binormal)
        {
            int n = points.Count;

            forward[0] = (points[1] - points[0]).normalized;
            forward[n - 1] = (points[n - 1] - points[n - 2]).normalized;
            for (int i = 1; i < n - 1; i++)
            {
                Vector3 inDir = (points[i] - points[i - 1]).normalized;
                Vector3 outDir = (points[i + 1] - points[i]).normalized;
                Vector3 bisector = inDir + outDir;
                forward[i] = bisector.sqrMagnitude > 1e-8f ? bisector.normalized : outDir;
            }

            // Seed the first ring with any vector perpendicular to the initial tangent.
            Vector3 seed = Mathf.Abs(Vector3.Dot(forward[0], Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
            normal[0] = Vector3.ProjectOnPlane(seed, forward[0]).normalized;
            binormal[0] = Vector3.Cross(forward[0], normal[0]).normalized;

            for (int i = 1; i < n; i++)
            {
                Quaternion transport = Quaternion.FromToRotation(forward[i - 1], forward[i]);
                Vector3 carried = transport * normal[i - 1];
                carried -= forward[i] * Vector3.Dot(carried, forward[i]); // re-orthogonalise
                normal[i] = carried.normalized;
                binormal[i] = Vector3.Cross(forward[i], normal[i]).normalized;
            }
        }

        // ---- core tube builder --------------------------------------------------------------

        private static Mesh BuildTubeMesh(IList<Vector3> points, float radius, int sides)
        {
            if (sides < 3)
                throw new System.InvalidOperationException("ConduitMesh: sides must be >= 3.");

            int n = points.Count;
            var forward = new Vector3[n];
            var normal = new Vector3[n];
            var binormal = new Vector3[n];
            ComputeFrames(points, forward, normal, binormal);

            // Cumulative arc length drives V so texel density stays uniform along the run.
            var dist = new float[n];
            for (int i = 1; i < n; i++)
                dist[i] = dist[i - 1] + Vector3.Distance(points[i], points[i - 1]);

            var vertices = new List<Vector3>(n * sides + (sides + 1) * 2);
            var normals = new List<Vector3>(vertices.Capacity);
            var uvs = new List<Vector2>(vertices.Capacity);
            var triangles = new List<int>();

            // One ring of `sides` vertices per path point, radial normals, U wraps 0..1.
            for (int i = 0; i < n; i++)
            {
                for (int s = 0; s < sides; s++)
                {
                    float angle = (float)s / sides * Mathf.PI * 2f;
                    Vector3 dir = normal[i] * Mathf.Cos(angle) + binormal[i] * Mathf.Sin(angle);
                    vertices.Add(points[i] + dir * radius);
                    normals.Add(dir);
                    uvs.Add(new Vector2((float)s / sides, dist[i]));
                }
            }

            // Side walls: two triangles per quad between consecutive rings. Winding is
            // Cross(v1-v0, v2-v0) == outward normal, which is Unity's front-facing convention.
            for (int i = 0; i < n - 1; i++)
            {
                int ringA = i * sides;
                int ringB = (i + 1) * sides;
                for (int s = 0; s < sides; s++)
                {
                    int s1 = (s + 1) % sides;
                    int a0 = ringA + s, a1 = ringA + s1, b0 = ringB + s, b1 = ringB + s1;
                    triangles.Add(a0); triangles.Add(a1); triangles.Add(b1);
                    triangles.Add(a0); triangles.Add(b1); triangles.Add(b0);
                }
            }

            // End caps: fan of triangles from a duplicated centre vertex, flat normal along the
            // path tangent (duplicated so RecalculateNormals is never needed to fix the cap -
            // sharing the radial wall vertices would smear their curved normals into the cap).
            AddCap(vertices, normals, uvs, triangles, points[0], normal[0], binormal[0], radius, sides,
                -forward[0], flipWinding: true);
            AddCap(vertices, normals, uvs, triangles, points[n - 1], normal[n - 1], binormal[n - 1], radius, sides,
                forward[n - 1], flipWinding: false);

            int vertCount = vertices.Count;
            var mesh = new Mesh
            {
                indexFormat = vertCount > 65000
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static void AddCap(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
            List<int> triangles, Vector3 center, Vector3 ringNormal, Vector3 ringBinormal, float radius,
            int sides, Vector3 capNormal, bool flipWinding)
        {
            int centerIndex = vertices.Count;
            vertices.Add(center);
            normals.Add(capNormal);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int ringStart = vertices.Count;
            for (int s = 0; s < sides; s++)
            {
                float angle = (float)s / sides * Mathf.PI * 2f;
                float c = Mathf.Cos(angle), sn = Mathf.Sin(angle);
                vertices.Add(center + (ringNormal * c + ringBinormal * sn) * radius);
                normals.Add(capNormal);
                uvs.Add(new Vector2(0.5f + c * 0.5f, 0.5f + sn * 0.5f));
            }

            for (int s = 0; s < sides; s++)
            {
                int s0 = ringStart + s;
                int s1 = ringStart + (s + 1) % sides;
                if (flipWinding)
                {
                    triangles.Add(centerIndex); triangles.Add(s1); triangles.Add(s0);
                }
                else
                {
                    triangles.Add(centerIndex); triangles.Add(s0); triangles.Add(s1);
                }
            }
        }
    }
}
