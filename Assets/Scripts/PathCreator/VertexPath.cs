using UnityEngine;

public class VertexPath
{
    // In this case, Path and VertexPaht's Space is Only XZ Space

    public readonly bool isClosedLoop;
    public readonly Vector3[] localPoints;
    public readonly Vector3[] localTangents;
    public readonly Vector3[] localNormals;
    public Vector3[] localSidePointsA;
    public Vector3[] localSidePointsB;

    public readonly float[] times;
    public readonly float length;
    public readonly float[] cumulativeLengthAtEachVertex;
    public readonly Bounds bounds;
    public readonly Vector3 up;

    private const int accuracy = 10;

    public int PointsCount { get => localPoints.Length; }

    public VertexPath(Path path, float minVertexDist = 0f, float roadWidth = 1f, float offsetY = 0f) :
        this(path, VertexPathUtility.SplitBezierPathByAngleError(path, minVertexDist, accuracy), roadWidth, offsetY) { }

    public VertexPath (Path path, VertexPathUtility.PathSplitData pathSplitData, float width, float offsetY)
    {
        isClosedLoop = path.IsClosed;

        int vertsCount = pathSplitData.vertices.Count;
        length = pathSplitData.cumulativeLength[vertsCount - 1];

        localPoints = new Vector3[vertsCount];
        localNormals = new Vector3[vertsCount];
        localTangents = new Vector3[vertsCount];
        localSidePointsA = new Vector3[vertsCount];
        localSidePointsB = new Vector3[vertsCount];
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
            localSidePointsA[i] = localPoints[i] - localNormals[i] * width;
            localSidePointsB[i] = localPoints[i] + localNormals[i] * width;
            localSidePointsA[i].y = GetTerrainPosition(localSidePointsA[i]).y + offsetY;
            localSidePointsB[i].y = GetTerrainPosition(localSidePointsB[i]).y + offsetY;
        }
    }

    public Vector3 GetTangent(int index)
    {
        return localTangents[index];
    }

    public Vector3 GetNormal(int index)
    {
        return localNormals[index];
    }

    public Vector3 GetPoint(int index)
    {
        return localPoints[index];
    }

    public Vector3 GetSidePointA(int index)
    {
        return localSidePointsA[index];
    }

    public Vector3 GetSidePointB(int index)
    {
        return localSidePointsB[index];
    }

    public Vector3 GetClosestPointOnPath(Vector3 point)
    {
        TimeOnPathData data = CalcuateClosestPointOnPathData(point);

        Vector3 pointA = Vector3.Lerp(localSidePointsA[data.previousIndex], localSidePointsA[data.nextIndex], data.percentBetweenIndices);
        Vector3 pointB = Vector3.Lerp(localSidePointsB[data.previousIndex], localSidePointsB[data.nextIndex], data.percentBetweenIndices);
        return (point - pointA).sqrMagnitude < (point - pointB).sqrMagnitude ? pointA : pointB;
    }

    private Vector3 GetTerrainPosition(Vector3 point)
    {
        point.y = 100;
        return Physics.Raycast(point, Vector3.down, out RaycastHit hit, 100, 1) ? hit.point : Vector3.zero;
    }

    private TimeOnPathData CalcuateClosestPointOnPathData(Vector3 point)
    {
        float minSqrDist = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        int closestSegmentIndexA = 0;
        int closestSegmentIndexB = 0;

        for (int i = 0; i < localPoints.Length; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= localPoints.Length)
            {
                if (isClosedLoop)
                    nextIndex %= localPoints.Length;
                else
                    break;
            }

            Vector3 closestPointOnSegment = MathUtility.ClosestPointOnLineSegment(point, GetPoint(i), GetPoint(nextIndex));
            float sqrDist = (point - closestPointOnSegment).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestPoint = closestPointOnSegment;
                closestSegmentIndexA = i;
                closestSegmentIndexB = nextIndex;
            }
        }

        float closestSegmentLength = (GetPoint(closestSegmentIndexA) - GetPoint(closestSegmentIndexB)).magnitude;
        float t = (closestPoint - GetPoint(closestSegmentIndexA)).magnitude / closestSegmentLength;
        return new TimeOnPathData(closestSegmentIndexA, closestSegmentIndexB, t);

    }

    public struct TimeOnPathData
    {
        public readonly int previousIndex;
        public readonly int nextIndex;
        public readonly float percentBetweenIndices;

        public TimeOnPathData(int prev, int next, float percent)
        {
            previousIndex = prev;
            nextIndex = next;
            percentBetweenIndices = percent;
        }
    }
}
