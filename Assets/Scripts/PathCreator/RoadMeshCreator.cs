using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(PathCreator))]
public class RoadMeshCreator : MonoBehaviour
{

    public bool autoUpdate;
    [Range(0.01f, 1f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public float thickness = 1;
    public float tiling = 1;
    public float offsetY = .1f;

    private PathCreator pathCreator;
    private VertexPath vertexPath;
    private Path path;

    private MeshRenderer meshRenderer = null;
    private MeshFilter meshFilter = null;
    private MeshCollider meshCollider = null;
    private Mesh mesh = null;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        meshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh();

        pathCreator = GetComponent<PathCreator>();
        path = pathCreator.path;
    }

    private void Update()
    {
        if (autoUpdate)
        {
            UpdateRoadMesh2();
        }    
    }

    private void UpdateRoadMesh()
    {
        vertexPath = new VertexPath(path, transform, .3f, spacing);

        Vector3[] vertices = new Vector3[vertexPath.PointsCount * 8];
        Vector2[] UVs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int trisLength = 2 * (vertexPath.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] roadTriangles = new int[trisLength * 3];
        int[] underRoadTriangles = new int[trisLength * 3];
        int[] sideOfRoadTriangles = new int[trisLength * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        for (int i = 0; i < vertexPath.PointsCount; i++, vertIndex += 8, triIndex += 6)
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
            vertices[vertIndex + 2] = vertSideA - localUp * thickness;
            vertices[vertIndex + 3] = vertSideB - localUp * thickness;

            vertices[vertIndex + 4] = vertices[vertIndex];
            vertices[vertIndex + 5] = vertices[vertIndex + 1];
            vertices[vertIndex + 6] = vertices[vertIndex + 2];
            vertices[vertIndex + 7] = vertices[vertIndex + 3];

            UVs[vertIndex] = new Vector2(0, vertexPath.times[i]);
            UVs[vertIndex] = new Vector2(1, vertexPath.times[i]);

            normals[vertIndex] = localUp;
            normals[vertIndex + 1] = localUp;
            normals[vertIndex + 2] = localUp;
            normals[vertIndex + 3] = -localUp;

            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;

            if (i < vertexPath.PointsCount - 1 || vertexPath.isClosedLoop)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % vertices.Length;
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % vertices.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    sideOfRoadTriangles[triIndex + 2 + j] = (vertIndex + sideOfRoadTriangles[j]) % vertices.Length;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.normals = normals;
        mesh.subMeshCount = 3;
        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(underRoadTriangles, 1);
        mesh.SetTriangles(sideOfRoadTriangles, 2);
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        //meshCollider.sharedMesh = mesh;
    }

    private Vector3 GetTerrainPosition(Vector3 point)
    {
        point.y = 100;
        return Physics.Raycast(point, Vector3.down, out RaycastHit hit, 100, 1) ? hit.point : Vector3.zero;
    }

    private void UpdateRoadMesh2()
    {
        vertexPath = new VertexPath(path, transform, .3f, spacing);

        Vector3[] verts = new Vector3[vertexPath.PointsCount * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (vertexPath.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < vertexPath.PointsCount; i++)
        {
            Vector3 forward = Vector2.zero;
            if (i < vertexPath.PointsCount - 1 || path.IsClosed)
            {
                forward += vertexPath.GetPoint((i + 1) % vertexPath.PointsCount) - vertexPath.GetPoint(i);
            }
            if (i > 0 || path.IsClosed)
            {
                forward += vertexPath.GetPoint(i) - vertexPath.GetPoint((i - 1 + vertexPath.PointsCount) % vertexPath.PointsCount);
            }

            forward.Normalize();
            Vector3 left = Quaternion.Euler(0, -90, 0) * forward;

            var position = vertexPath.GetPoint(i) + Vector3.up * offsetY;
            verts[vertIndex] = position + left * roadWidth * .5f;
            verts[vertIndex + 1] = position - left * roadWidth * .5f;

            float completionPercent = i / (float)(vertexPath.PointsCount - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < vertexPath.PointsCount - 1 || path.IsClosed)
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

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
