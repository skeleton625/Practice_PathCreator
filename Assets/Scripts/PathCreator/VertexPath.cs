using UnityEngine;

public class VertexPath
{
    // In this case, Path and VertexPaht's Space is Only XZ Space

    public readonly bool isClosedLoop;
    public readonly Vector3[] localPoints;
    public readonly Vector3[] localTangents;
    public readonly Vector3[] localNormals;

    public readonly float[] times;
    public readonly float length;
    public readonly float[] cumulativeLengthAtEachVertex;
    public readonly Bounds bounds;
    public readonly Vector3 up;

    private const int accuracy = 10;
    private const float minVertexSpacing = .01f;

    private Transform transform;

    public int PointsCount { get => localPoints.Length; }

    public VertexPath(Path path, Transform transform, float maxAngleError = .3f, float minVertexDist = 0f) :
        this(path, VertexPathUtility.SplitBezierPathByAngleError(path, maxAngleError, minVertexDist, accuracy), transform) { }

    public VertexPath (Path path, VertexPathUtility.PathSplitData pathSplitData, Transform transform)
    {
        this.transform = transform;
        isClosedLoop = path.IsClosed;

        int vertsCount = pathSplitData.vertices.Count;
        length = pathSplitData.cumulativeLength[vertsCount - 1];

        localPoints = new Vector3[vertsCount];
        localNormals = new Vector3[vertsCount];
        localTangents = new Vector3[vertsCount];
        cumulativeLengthAtEachVertex = new float[vertsCount];
        times = new float[vertsCount];
        bounds = new Bounds((pathSplitData.minMax.Min + pathSplitData.minMax.Max) / 2, pathSplitData.minMax.Max - pathSplitData.minMax.Min);
        up = (bounds.size.z > bounds.size.y) ? Vector3.up : -Vector3.forward;

        for (int i = 0; i < localPoints.Length; i++)
        {
            localPoints[i] = pathSplitData.vertices[i];
            localTangents[i] = pathSplitData.tangents[i];
            cumulativeLengthAtEachVertex[i] = pathSplitData.cumulativeLength[i];
            times[i] = cumulativeLengthAtEachVertex[i] / length;

            localNormals[i] = -Vector3.Cross(localTangents[i], up);
        }
    }

    public Vector3 GetTangent(int index)
    {
        return MathUtility.TransformDirection(localTangents[index], transform);
    }

    public Vector3 GetNormal(int index)
    {
        return MathUtility.TransformDirection(localNormals[index], transform);
    }

    public Vector3 GetPoint(int index)
    {
        return MathUtility.TransformPoint(localPoints[index], transform);
    }
}
