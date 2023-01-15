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
        road.GetComponent<MeshCollider>().enabled = true;
        mesh = new Mesh();
    }

    public void UpdateRoadMesh(Path path)
    {
        preVertexPath = new VertexPath(path, RoadSpacing, RoadWidth, OffsetY);

        Vector3[] vertices = new Vector3[preVertexPath.PointsCount * 2];
        Vector2[] UVs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int trisLength = 2 * (preVertexPath.PointsCount - 1) + (path.IsClosed ? 2 : 0);
        int[] roadTriangles = new int[trisLength * 3];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < preVertexPath.PointsCount; i++)
        {
            Vector3 localUp = Vector3.Cross(preVertexPath.GetTangent(i), preVertexPath.GetNormal(i));

            vertices[vertIndex + 0] = preVertexPath.GetSidePointA(i);
            vertices[vertIndex + 1] = preVertexPath.GetSidePointB(i);
            UVs[vertIndex + 0] = new Vector2(0, preVertexPath.times[i]);
            UVs[vertIndex + 1] = new Vector2(1, preVertexPath.times[i]);
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;

            if (i < preVertexPath.PointsCount - 1 || path.IsClosed)
            {
                roadTriangles[triIndex + 0] = vertIndex;
                roadTriangles[triIndex + 1] = (vertIndex + 2) % vertices.Length;
                roadTriangles[triIndex + 2] = vertIndex + 1;

                roadTriangles[triIndex + 3] = vertIndex + 1;
                roadTriangles[triIndex + 4] = (vertIndex + 2) % vertices.Length;
                roadTriangles[triIndex + 5] = (vertIndex + 3) % vertices.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.normals = normals;
        mesh.SetTriangles(roadTriangles, 0);
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
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
