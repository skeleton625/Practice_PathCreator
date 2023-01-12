using UnityEngine;

public static class MathUtility
{
    public static Vector3 TransformDirection(Vector3 p, Transform t)
    {
        LcokTransform original = LockTransformToSpace(t);
        Vector3 transformedPoint = t.TransformDirection(p);
        original.SetTransform(t);
        return transformedPoint;
    }

    public static Vector3 TransformPoint(Vector3 p, Transform t)
    {
        LcokTransform original = LockTransformToSpace(t);
        Vector3 transformedPoint = t.TransformPoint(p);
        original.SetTransform(t);
        return transformedPoint;
    }

    public static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Angle((a - b), (c - b));
    }

    private static LcokTransform LockTransformToSpace(Transform t)
    {
        LcokTransform original = new LcokTransform(t);
        t.eulerAngles = new Vector3(0, t.eulerAngles.y, 0);
        t.position = new Vector3(t.position.x, 0, t.position.z);

        float maxScale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
        t.localScale = Vector3.one * maxScale;
        return original;
    }

    private class LcokTransform
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;

        public LcokTransform(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            scale = t.localScale;
        }

        public void SetTransform(Transform t)
        {
            t.position = position;
            t.rotation = rotation;
            t.localScale = scale;
        }
    }
}
