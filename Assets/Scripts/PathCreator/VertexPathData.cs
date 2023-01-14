using UnityEngine;

public class VertexPathData : MonoBehaviour
{
    private VertexPath vertexPath = null;

    public void Initialize(VertexPath vertexPath)
    {
        this.vertexPath = vertexPath;
    }

    public Vector3 GetClosestPoint(Vector3 point)
    {
        return vertexPath.GetClosestPointOnPath(point);
    }
}
