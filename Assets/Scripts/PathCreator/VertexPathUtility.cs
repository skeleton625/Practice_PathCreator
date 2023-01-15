using System.Collections.Generic;
using UnityEngine;

public static class VertexPathUtility
{
    private static float maxAngleError = .3f;

    public static PathSplitData SplitBezierPathByAngleError(Path path, float minVertexDist, float accuracy)
    {
        PathSplitData splitData = new PathSplitData();

        splitData.vertices.Add(path[0]);
        splitData.tangents.Add(BezierUtility.EvaluateCurveDerivative(path.GetPointsInSegment(0), 0).normalized);
        splitData.cumulativeLength.Add(0);
        splitData.anchorVertexMap.Add(0);
        splitData.minMax.AddValue(path[0]);

        Vector3 prevPointOnPath = path[0];
        Vector3 lastAddedPoint = path[0];

        float currentPathLength = 0;
        float distSinceLastVertex = 0;

        for (int segmentIndex = 0; segmentIndex < path.SegmentsCount; segmentIndex++)
        {
            Vector3[] segmentPoitns = path.GetPointsInSegment(segmentIndex);
            float estimatedSegmentLength = BezierUtility.EstimateCurveLength(segmentPoitns);
            int divisions = Mathf.CeilToInt(estimatedSegmentLength * accuracy);
            float increment = 1f / divisions;

            for (float t = increment; t <= 1; t += increment)
            {
                bool isLastPointOnPath = (t + increment > 1 && segmentIndex.Equals(path.SegmentsCount - 1));
                if (isLastPointOnPath) t = 1;

                Vector3 pointOnPath = BezierUtility.EvaluateCurve(segmentPoitns, t);
                Vector3 nextPointOnPath = BezierUtility.EvaluateCurve(segmentPoitns, t + increment);

                float localAngle = 180 - MathUtility.MinAngle(prevPointOnPath, pointOnPath, nextPointOnPath);
                float angleFromPrevVertex = 180 - MathUtility.MinAngle(lastAddedPoint, pointOnPath, nextPointOnPath);
                float angleError = Mathf.Max(localAngle, angleFromPrevVertex);

                if ((angleError > maxAngleError && distSinceLastVertex >= minVertexDist) || isLastPointOnPath)
                {
                    currentPathLength += (lastAddedPoint - pointOnPath).magnitude;
                    splitData.cumulativeLength.Add(currentPathLength);
                    splitData.vertices.Add(pointOnPath);
                    splitData.tangents.Add(BezierUtility.EvaluateCurveDerivative(segmentPoitns, t).normalized);
                    splitData.minMax.AddValue(pointOnPath);
                    distSinceLastVertex = 0;
                    lastAddedPoint = pointOnPath;
                }
                else
                {
                    distSinceLastVertex += (pointOnPath - prevPointOnPath).magnitude;
                }
                prevPointOnPath = pointOnPath;
            }
            splitData.anchorVertexMap.Add(splitData.vertices.Count - 1);
        }
        return splitData;
    }

    public class PathSplitData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> tangents = new List<Vector3>();
        public List<float> cumulativeLength = new List<float>();
        public List<int> anchorVertexMap = new List<int>();
        public MinMax3D minMax = new MinMax3D();
    }
}
