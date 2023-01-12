using UnityEngine;

public static class BezierUtility
{
    public static Vector3 EvaluateCurveDerivative(Vector3[] points, float t)
    {
        t = Mathf.Clamp01(t);
        float reverseT = 1 - t;
        return 3 * reverseT * reverseT * (points[1] - points[0]) + 
               6 * reverseT * t * (points[2] - points[1]) +
               3 * t * t * (points[3] - points[2]);
    }

    public static Vector3 EvaluateCurve(Vector3[] points, float t)
    {
        t = Mathf.Clamp01(t);
        float reverseT = 1 - t;
        return reverseT * reverseT * reverseT * points[0] +
               3 * reverseT * reverseT * t * points[1] +
               3 * reverseT * t * t * points[2] +
               t * t * t * points[3];
    }

    public static float EstimateCurveLength(Vector3[] points)
    {
        float controlNetLength = (points[0] - points[1]).magnitude + (points[1] - points[2]).magnitude + (points[2] - points[3]).magnitude;
        return (points[0] - points[3]).magnitude + controlNetLength / 2f; // estimatedCurveLength
    }
}
