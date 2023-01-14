using UnityEngine;

public class RoadMeshCreator : MonoBehaviour
{
    [Header("Road Holder Object Setting")]
    [SerializeField] private GameObject RoadObject = null;

    private MeshRenderer meshRenderer = null;
    private MeshFilter meshFilter = null;
    private MeshCollider meshCollider = null;
    private Mesh mesh = null;

    private VertexPath preVertexPath = null;

    [Header("Mesh Setting")]
    [SerializeField] private float RoadSpacing = 1;
    [SerializeField] private float RoadWidth = 1;
    [SerializeField] private float RoadThickness = .5f;
    [SerializeField] private float OffsetY = .1f;
    [Space(10)]

    [Header("Material Setting")]
    [SerializeField] private float TextureTiling = 8;

    private void Start()
    {
        meshRenderer = RoadObject.GetComponent<MeshRenderer>();
        meshFilter = RoadObject.GetComponent<MeshFilter>();
        meshCollider = RoadObject.GetComponent<MeshCollider>();
        mesh = new Mesh();

        meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, TextureTiling);
    }

    public void SetActiveVisual(bool isActive)
    {
        RoadObject.SetActive(isActive);

        if (!isActive)
        {
            preVertexPath = null;
        }
    }

    public void GenerateRoad()
    {
        var road = Instantiate(RoadObject, Vector3.zero, Quaternion.identity);
        road.GetComponent<VertexPathData>().Initialize(preVertexPath);
        mesh = new Mesh();
    }

    public void UpdateRoadMesh(Path path)
    {
        preVertexPath = new VertexPath(path, RoadObject.transform, .3f, RoadSpacing);

        Vector3[] vertices = new Vector3[preVertexPath.PointsCount * 8];
        Vector2[] UVs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int trisLength = 2 * (preVertexPath.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] roadTriangles = new int[trisLength * 3];
        int[] underRoadTriangles = new int[trisLength * 3];
        int[] sideOfRoadTriangles = new int[trisLength * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        for (int i = 0; i < preVertexPath.PointsCount; i++)
        {
            Vector3 localUp = Vector3.Cross(preVertexPath.GetTangent(i), preVertexPath.GetNormal(i));
            Vector3 localRight = preVertexPath.GetNormal(i);

            // Find position to left and right of current path vertex
            Vector3 vertSideA = preVertexPath.GetPoint(i) - localRight * Mathf.Abs(RoadWidth);
            Vector3 vertSideB = preVertexPath.GetPoint(i) + localRight * Mathf.Abs(RoadWidth);

            vertSideA.y = GetTerrainPosition(vertSideA + transform.position).y + OffsetY;
            vertSideB.y = GetTerrainPosition(vertSideB + transform.position).y + OffsetY;

            // Add top of road vertices
            vertices[vertIndex + 0] = vertSideA;
            vertices[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            vertices[vertIndex + 2] = vertSideA - localUp * RoadThickness;
            vertices[vertIndex + 3] = vertSideB - localUp * RoadThickness;

            // Duplicate vertices to get flat shading for sides of road
            vertices[vertIndex + 4] = vertices[vertIndex + 0];
            vertices[vertIndex + 5] = vertices[vertIndex + 1];
            vertices[vertIndex + 6] = vertices[vertIndex + 2];
            vertices[vertIndex + 7] = vertices[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            UVs[vertIndex + 0] = new Vector2(0, preVertexPath.times[i]);
            UVs[vertIndex + 1] = new Vector2(1, preVertexPath.times[i]);

            // Top of road normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of road normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;
            // Sides of road normals
            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;

            // Set triangle indices
            if (i < preVertexPath.PointsCount - 1 || path.IsClosed)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % vertices.Length;
                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % vertices.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % vertices.Length;
                }

            }

            vertIndex += 8;
            triIndex += 6;
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
        meshCollider.sharedMesh = mesh;
    }

    private Vector3 GetTerrainPosition(Vector3 point)
    {
        point.y = 100;
        return Physics.Raycast(point, Vector3.down, out RaycastHit hit, 100, 1) ? hit.point : Vector3.zero;
    }

    /*
    private void UpdateRoadMesh2()
    {
        vertexPath = new VertexPath(path, RoadObject.transform, .3f, RoadSpacing);

        Vector3[] verts = new Vector3[vertexPath.PointsCount * 2];
        Vector2[] uvs = new Vector2[verts.Length];

        int trisCount = 2 * (vertexPath.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] tris = new int[trisCount * 3];
        
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < vertexPath.PointsCount; i++)
        {
            Vector3 position = GetTerrainPosition(vertexPath.GetPoint(i)) + Vector3.up * OffsetY;
            Vector3 localRight = vertexPath.GetNormal(i);
            verts[vertIndex] = position - localRight * RoadWidth * .5f;
            verts[vertIndex + 1] = position + localRight * RoadWidth * .5f;

            float completionPercent = i / (float)(vertexPath.PointsCount - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);

            uvs[vertIndex + 0] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < vertexPath.PointsCount - 1 || path.IsClosed)
            {
                tris[triIndex + 0] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    */
}
