using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadMeshCreator : MonoBehaviour
{
    [Range(.05f, 1.5f)]
    public bool autoUpdate;
    public float spacing = 1;
    public float roadWidth = 1;
    public float thickness = 1;
    public float tiling = 1;
    public float offsetY = .1f;

    private PathCreator pathCreator;
    private VertexPath vertexPath;
    private Path path;

    private void Start()
    {
        pathCreator = GetComponent<PathCreator>();
        path = pathCreator.path;

        vertexPath = new VertexPath(path, transform, .3f, spacing);
    }

    private void UpdateRoadMesh()
    {
        Vector3[] vertices = new Vector3[path.PointsCount * 8];
        Vector2[] UVs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int trisLength = 2 * (path.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] roadTriangles = new int[trisLength * 3];
        int[] underRoadTriangles = new int[trisLength * 3];
        int[] sideOfRoadTriangles = new int[trisLength * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        for (int i = 0; i < vertexPath.PointsCount; ++i)
        {
            Vector3 localUp = vertexPath.up;
            Vector3 localRight = Vector3.Cross(localUp, vertexPath.GetTangent(i));

            Vector3 widthVector = localRight * Mathf.Abs(roadWidth) - transform.position;
            Vector3 vertSideA = vertexPath.GetPoint(i) - widthVector;
            Vector3 vertSideB = vertexPath.GetPoint(i) + widthVector;
            vertSideA.y = GetTerrainPosition(vertSideA).y + offsetY;
            vertSideB.y = GetTerrainPosition(vertSideB).y + offsetY;

            vertices[vertIndex] = vertSideA;
            vertices[vertIndex + 1] = vertSideB;
            vertices[vertIndex + 2] = vertSideA;
        }
    }

    private Vector3 GetTerrainPosition(Vector3 point)
    {
        point.y = 100;
        return Physics.Raycast(point, Vector3.down, out RaycastHit hit, 100, 1) ? hit.point : Vector3.zero;
    }

    /* Prev Update Mesh
    private Mesh CreateRoadMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector2.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            forward.Normalize();
            Vector3 left = Quaternion.Euler(0, -90, 0) * forward;

            var position = points[i] + Vector3.up * offsetY;
            verts[vertIndex] = position + left * roadWidth * .5f;
            verts[vertIndex + 1] = position - left * roadWidth * .5f;

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
    */
}
