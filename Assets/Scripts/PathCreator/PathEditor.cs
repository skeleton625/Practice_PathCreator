using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.UI;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator pathCreator = null;
    private Path path;

    private const float segmentSelectDistanceThreshold = .1f;
    private int segmentSelectedIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();   // Start Check path status
        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(pathCreator, "Create path");
            pathCreator.CreatePath();
            path = pathCreator.path;
        }

        bool toggleClosed = GUILayout.Toggle(path.IsClosed, "Toggle closed");
        if (toggleClosed != path.IsClosed)
        {
            Undo.RecordObject(pathCreator, "Toggle closed path");
            path.IsClosed = toggleClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(pathCreator, "Toggle auto set controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck()) // IF path is changed, Repaint All Scenes
            SceneView.RepaintAll();
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    private void Input()
    {
        Event guiEvent = Event.current;
        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition), out RaycastHit hit, int.MaxValue, -1))
        {
            if (!guiEvent.shift) return;

            // Left Mouse Button Input + Left Shift Key Input
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                if (!segmentSelectedIndex.Equals(-1))
                {
                    Undo.RecordObject(pathCreator, "Split Segment");
                    path.SplitSegment(hit.point, segmentSelectedIndex);
                    segmentSelectedIndex = -1;
                }
                else if (!path.IsClosed)
                {
                    Undo.RecordObject(pathCreator, "Add Segment");
                    path.AddSegment(hit.point);
                }
            }
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                float minDistToAnchor = pathCreator.anchorDiameter * .5f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < path.PointsCount; i += 3)
                {
                    float dist = (hit.point - path[i]).magnitude;
                    if (dist < minDistToAnchor)
                    {
                        minDistToAnchor = dist;
                        closestAnchorIndex = i;
                    }
                }

                if (!closestAnchorIndex.Equals(-1))
                {
                    Undo.RecordObject(pathCreator, "Remove Segment");
                    path.RemoveSegment(closestAnchorIndex);
                }
            }

            if (guiEvent.type == EventType.MouseMove)
            {
                float minDistToSegment = segmentSelectDistanceThreshold;
                int newSegmentSelectedIndex = -1;
                for (int i = 0; i < path.SegmentsCount; ++i)
                {
                    Vector3[] points = path.GetPointsInSegment(i);
                    float dist = HandleUtility.DistancePointBezier(hit.point, points[0], points[3], points[1], points[2]);
                    if (dist < minDistToSegment)
                    {
                        minDistToSegment = dist;
                        newSegmentSelectedIndex = i;
                    }
                }

                if (!newSegmentSelectedIndex.Equals(segmentSelectedIndex))
                {
                    segmentSelectedIndex = newSegmentSelectedIndex;
                    HandleUtility.Repaint();
                }
            }

        }
    }

    private void Draw()
    {
        for (int i = 0; i < path.SegmentsCount; ++i)
        {
            Vector3[] points = path.GetPointsInSegment(i);

            if (!path.AutoSetControlPoints && pathCreator.displayControlPoints)
            {
                Handles.color = pathCreator.controlLineColor;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }

            Color segmentColor = (Event.current.shift && i.Equals(segmentSelectedIndex)) ? pathCreator.sgementSelectedColor : pathCreator.segmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
        }

        int scale = 3;
        if (!path.AutoSetControlPoints && pathCreator.displayControlPoints) scale = 1;

        for (int i = 0; i < path.PointsCount; i += scale)
        {
            float handleSize = 1f;
            if ((i % 3).Equals(0))
            {
                Handles.color = pathCreator.anchorColor;
                handleSize = pathCreator.anchorDiameter;
            }
            else
            {
                Handles.color = pathCreator.controlColor;
                handleSize = pathCreator.controlDiameter;
            }

            Vector3 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, handleSize, Vector3.zero, Handles.SphereHandleCap);
            newPos.y = 0;
            if (!newPos.Equals(path[i]))
            {
                Undo.RecordObject(pathCreator, "Move point"); // Record Change point data
                path.MovePoints(i, newPos);
            }
        }
    }

    private void OnEnable()
    {
        pathCreator = (PathCreator)target;
        if (pathCreator.path == null)
            pathCreator.CreatePath();

        pathCreator.bezierCreated -= ResetState;
        pathCreator.bezierCreated += ResetState;

        path = pathCreator.path;
    }

    private void ResetState()
    {
        segmentSelectedIndex = -1;
        path = pathCreator.path;

        SceneView.RepaintAll();
    }
}
