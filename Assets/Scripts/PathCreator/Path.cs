using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    private List<Vector3> points = null;
    [SerializeField, HideInInspector]
    private bool isClosed = false;
    [SerializeField, HideInInspector]
    private bool autoSetControlPoints = false;

    public Vector3 this[int index]
    {
        get => points[index];
    }

    public bool IsClosed
    {
        get => isClosed;
        set
        {
            if (isClosed != value)
            {
                isClosed = value;
                ToggleClosed();
            }
        }
    }

    public bool AutoSetControlPoints
    {
        get => autoSetControlPoints;
        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    public int PointsCount
    {
        get => points.Count;
    }

    public int AnchorPointsCount
    {
        get => isClosed ? points.Count / 3 : (points.Count + 2) / 3;
    }

    public int SegmentsCount
    {
        // Points count(Segment count)
        // Open : 4(1) .. 7(2) .. 10(3) ...
        // Close : 6(2) .. 9(3) .. 12(4) ...
        get => points.Count / 3;
    }

    public Path(Vector3 centerPoint, bool autoSetControlPoints = false)
    {
        points = new List<Vector3>
        {
            centerPoint + new Vector3(-1, 0, 0),
            centerPoint + new Vector3(-1, 0, 1) * .5f,
            centerPoint + new Vector3(1, 0, -1) * .5f,
            centerPoint + new Vector3(1, 0, 0)
        };

        AutoSetControlPoints = autoSetControlPoints;
    }

    #region Path Add, Remove point Functions
    public void AddSegment(Vector3 anchorPos)
    {
        int lastAnchorIndex = points.Count - 1;
        // Set position for new control to be mirror of its counterpart
        Vector3 secondControlForOldLastAnchorOffset = (points[lastAnchorIndex] - points[lastAnchorIndex - 1]);
        if (!autoSetControlPoints)
        {
            // Set position for new control to be aligned with its counterpart, but with a length of half the distance from prev to new anchor
            float dstPrevToNewAnchor = (points[lastAnchorIndex] - anchorPos).magnitude;
            secondControlForOldLastAnchorOffset = (points[lastAnchorIndex] - points[lastAnchorIndex - 1]).normalized * dstPrevToNewAnchor * .5f;
        }
        Vector3 secondControlForOldLastAnchor = points[lastAnchorIndex] + secondControlForOldLastAnchorOffset;
        Vector3 controlForNewAnchor = (anchorPos + secondControlForOldLastAnchor) * .5f;

        points.Add(secondControlForOldLastAnchor);
        points.Add(controlForNewAnchor);
        points.Add(anchorPos);

        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(points.Count - 1);
    }

    public void RemoveSegment(int anchorIndex)
    {
        if (SegmentsCount > 2 || !isClosed && SegmentsCount > 1)
        {
            if (anchorIndex.Equals(0))
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex.Equals(points.Count - 1) && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        else
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
    }

    public void MovePoints(int index, Vector3 newPos)
    {
        if (autoSetControlPoints && (index % 3).Equals(0))
        {
            points[index] = newPos;
            AutoSetAllAffectedControlPoints(index);
        }
        else if (!autoSetControlPoints)
        {
            if ((index % 3).Equals(0)) // IF index is Anchor Point, same move control points
            {
                Vector3 deltaMove = newPos - points[index];
                if (index + 1 < points.Count || isClosed)
                    points[GetLoopIndex(index + 1)] += deltaMove;
                if (index - 1 > -1 || isClosed)
                    points[GetLoopIndex(index - 1)] += deltaMove;
            }
            else
            {
                // IF next point is Anchor point
                bool nextPointIsAnchor = ((index + 1) % 3).Equals(0);
                int correspondingControlIndex = nextPointIsAnchor ? index + 2 : index - 2;
                int anchorIndex = nextPointIsAnchor ? index + 1 : index - 1;

                if (correspondingControlIndex > -1 && correspondingControlIndex < points.Count || isClosed)
                {
                    float dist = (points[GetLoopIndex(anchorIndex)] - points[GetLoopIndex(correspondingControlIndex)]).magnitude;
                    Vector3 newDir = (points[GetLoopIndex(anchorIndex)] - newPos).normalized;
                    points[GetLoopIndex(correspondingControlIndex)] = points[GetLoopIndex(anchorIndex)] + newDir * dist;
                }
            }
            points[index] = newPos;
        }
    }

    private void ToggleClosed()
    {
        if (isClosed)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]); // Last Anchor point, control point
            points.Add(points[0] * 2 - points[1]);  // First Anchor point, control point

            if (autoSetControlPoints)
            {
                AutoSetAnchorControlPoints(0); // First Anchor point
                AutoSetAnchorControlPoints(points.Count - 3); // Last Anchor point
            }
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2); // Remove closed points
            if (autoSetControlPoints)
                AutoSetStartAndEndControls();
        }
    }
    #endregion

    #region Path Auto Control Points Functions
    private void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        // IF modified anchor point, modify points surround updated anchor point
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i > -1 && i < points.Count || isClosed)
                AutoSetAnchorControlPoints(GetLoopIndex(i));
        }

        // IF affected Start, End Anchor point
        AutoSetStartAndEndControls();
    }

    private void AutoSetAllControlPoints()
    {
        if (AnchorPointsCount > 2)
        {
            // All Anchor point Except Start, End Anchor point
            for (int i = 0; i < points.Count; i += 3)
                AutoSetAnchorControlPoints(i);
        }

        // Start, End Anchor point
        AutoSetStartAndEndControls();
    }

    private void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighborDistances = new float[2];

        if (anchorIndex - 3 > -1 || isClosed) // Prev neighbor anchor point
        {
            Vector3 offset = points[GetLoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighborDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 > -1 || isClosed) // Next neighbor anchor point
        {
            Vector3 offset = points[GetLoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighborDistances[1] = -offset.magnitude; // prev anchor point is different direction from next anchor point
        }

        dir.Normalize();
        for (int i = 0; i < 2; ++i)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex > -1 && controlIndex < points.Count || isClosed)
                points[GetLoopIndex(controlIndex)] = anchorPos + dir * neighborDistances[i] * .3f;
        }
    }

    private void AutoSetStartAndEndControls()
    {
        if (isClosed)
        {
            // Handle case with only 2 anchor points separately, as will otherwise result in straight line ()
            if (AnchorPointsCount.Equals(2))
            {
                Vector3 dirAnchorAToB = (points[3] - points[0]).normalized;
                float dstBetweenAnchors = (points[0] - points[3]).magnitude;
                Vector3 perp = Vector3.Cross(dirAnchorAToB, Vector3.up);
                points[1] = points[0] + perp * dstBetweenAnchors / 2f;
                points[5] = points[0] - perp * dstBetweenAnchors / 2f;
                points[2] = points[3] + perp * dstBetweenAnchors / 2f;
                points[4] = points[3] - perp * dstBetweenAnchors / 2f;
            }
            else
            {
                AutoSetAnchorControlPoints(0);
                AutoSetAnchorControlPoints(points.Count - 3);
            }
        }
        else
        {
            // Handle case with 2 anchor points separately, as otherwise minor adjustments cause path to constantly flip
            if (AnchorPointsCount.Equals(2))
            {
                points[1] = points[0] + (points[3] - points[0]) * .25f;
                points[2] = points[3] + (points[0] - points[3]) * .25f;
            }
            else
            {
                points[1] = (points[0] + points[2]) * .5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
            }
        }
    }
    #endregion

    #region Path Get Point Functions
    public Vector3[] GetPointsInSegment(int anchorIndex)
    {
        int index = anchorIndex * 3;
        return new Vector3[] { points[index], points[index + 1], points[index + 2], points[GetLoopIndex(index + 3)] };
        // Open : 1 - Start Anchor point, 2 - Start Control point, 3 - End Control point, 4 - End Anchor point
        // Close : 1 - Start Anchor point, 2 - Start Control point, 3 - End Control point, 4 - End Anchor point(First Anchor point) 
    }

    private int GetLoopIndex(int index)
    {
        return (index + points.Count) % points.Count; // Only return points.Count below value
    }
    #endregion
}
