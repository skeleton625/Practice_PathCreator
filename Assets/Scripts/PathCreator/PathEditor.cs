using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator pathCreator = null;
    Path path;

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
            // Left Mouse Button Input + Left Shift Key Input
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                Undo.RecordObject(pathCreator, "Insert Segment");
                path.InsertSegment(hit.point);
            }
        }
    }

    private void Draw()
    {
        for (int i = 0; i < path.SegmentsCount; ++i)
        {
            Vector3[] points = path.GetPointsInSegment(i);

            Handles.color = Color.yellow;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        Handles.color = Color.red;
        for (int i = 0; i < path.PointsCount; ++i)
        {
            Vector3 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .1f, Vector3.zero, Handles.SphereHandleCap);
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

        path = pathCreator.path;
    }

}
