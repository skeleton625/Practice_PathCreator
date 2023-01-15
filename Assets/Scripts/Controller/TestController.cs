using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestController : MonoBehaviour
{
    [Header("Road Setting")]
    [SerializeField] private PathCreator pathCreator = null;
    [SerializeField] private Transform TargetTransform = null;
    [SerializeField] private NavMeshSurface navMeshSurface = null;
    [Space(10)]

    private bool startGenerateRoad = false;
    private byte generateRoadType = 0;
    private int generateRoadIndex = 0;

    private int snappedHashCode = 0;
    private VertexPathData snappedData = null;

    private Camera mainCamera = null;

    private void Start()
    {
        startGenerateRoad = false;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Start Generate Road");
            startGenerateRoad = true;
        }

        if (startGenerateRoad)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000, -1)) return;

            switch (generateRoadType)
            {
                case 0:
                    TargetTransform.position = hit.point;

                    if (Input.GetMouseButtonDown(0))
                    {
                        pathCreator.path.MovePoints(0, hit.point);
                        pathCreator.path.MovePoints(3, hit.point);
                        generateRoadType = 1;
                        generateRoadIndex = 3;
                        pathCreator.SetActiveVisual(true);
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        ExitGenerateRoad();
                    }
                    break;
                case 1:
                    if (hit.transform.CompareTag("Road"))
                    {
                        if (snappedData == null)
                        {
                            snappedHashCode = hit.transform.GetHashCode();
                            snappedData = hit.transform.GetComponent<VertexPathData>();
                        }
                        else if (hit.transform.GetHashCode().Equals(snappedHashCode))
                            TargetTransform.position = snappedData.GetClosestPoint(hit.point);
                    }
                    else if (snappedData != null)
                    {
                        snappedHashCode = 0;
                        snappedData = null;
                    }
                    else
                        TargetTransform.position = hit.point;
                    pathCreator.path.MovePoints(generateRoadIndex, TargetTransform.position);

                    if (Input.GetMouseButtonDown(0))
                    {
                        pathCreator.path.AddSegment(hit.point);
                        generateRoadIndex += 3;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        if (generateRoadIndex > 3)
                        {
                            pathCreator.path.RemoveSegment(generateRoadIndex);
                            pathCreator.RefreshRoad();

                            pathCreator.GenerateRoad();
                            navMeshSurface.UpdateNavMesh();
                        }

                        ExitGenerateRoad();
                        break;
                    }
                    break;
            }
        }
    }

    private void ExitGenerateRoad()
    {
        Debug.Log("End Generate Road");
        startGenerateRoad = false;
        generateRoadType = 0;
        generateRoadIndex = 0;

        pathCreator.SetActiveVisual(false);
        pathCreator.CreatePath();
    }
}
