using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] private PathCreator pathCreator = null;

    [SerializeField] private float pointSpacing = .1f;
    [SerializeField] private float pointResolution = 1f;

    private List<GameObject> preSpacedPointsList = null;

    private void Start()
    {
        preSpacedPointsList = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, -1))
                pathCreator.path.AddSegment(hit.point);
        }

        if (Input.GetMouseButtonDown(2))
        {
            if (preSpacedPointsList.Count > 0)
            {
                for (int i = 0; i < preSpacedPointsList.Count; ++i)
                    Destroy(preSpacedPointsList[i]);
                preSpacedPointsList.Clear();
            }

            var pointList = pathCreator.path.CalculateEvenlySpacedPoints(pointSpacing, pointResolution);

            for (int i = 0; i < pointList.Length; ++i)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.position = pointList[i];
                point.transform.localScale = Vector3.one * pointSpacing * .5f;
                preSpacedPointsList.Add(point);
            }
        }
    }
}
