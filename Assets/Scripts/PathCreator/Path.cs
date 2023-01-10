using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    private List<Vector3> points = null;

    public Vector3 this[int index]
    {
        get => points[index];
    }

    public int PointsCount
    {
        get => points.Count;
    }

    public int SegmentsCount
    {
        get => (points.Count - 4) / 3 + 1;
    }

    public Path(Vector3 centerPoint)
    {
        points = new List<Vector3>
        {
            centerPoint + new Vector3(-1, 0, 0),
            centerPoint + new Vector3(-1, 0, 1) * .5f,
            centerPoint + new Vector3(1, 0, -1) * .5f,
            centerPoint + new Vector3(1, 0, 0)
        };
    }

    public void InsertSegment(Vector3 anchorPos)
    {
        int lastIndex = points.Count - 1;
        points.Add(points[lastIndex] * 2 - points[lastIndex - 1]); // Prev anchor's new Point
        points.Add((points[lastIndex] + anchorPos) * .5f); // Pre anchor's new Point
        points.Add(anchorPos); // Pre anchor position
    }

    public void MovePoints(int index, Vector3 newPos)
    {
        if ((index % 3).Equals(0)) // IF index is Anchor Point, same move control points
        {
            Vector3 deltaMove = newPos - points[index];
            if (index + 1 < points.Count)
                points[index + 1] += deltaMove;
            if (index - 1 > -1)
                points[index - 1] += deltaMove;
        }
        else
        {
            // IF next point is Anchor point
            bool nextPointIsAnchor = ((index + 1) % 3).Equals(0);
            int correspondingControlIndex = nextPointIsAnchor ? index + 2 : index - 2;
            int anchorIndex = nextPointIsAnchor ? index + 1 : index - 1;

            if (correspondingControlIndex > -1 && correspondingControlIndex < points.Count)
            {
                float dist = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                Vector3 newDir = (points[anchorIndex] - newPos).normalized;
                points[correspondingControlIndex] = points[anchorIndex] + newDir * dist;
            }
        }    

        points[index] = newPos;
    }

    public Vector3[] GetPointsInSegment(int anchorIndex)
    {
        int index = anchorIndex * 3;
        return new Vector3[] { points[index], points[index + 1], points[index + 2], points[index + 3] };
    }
}
