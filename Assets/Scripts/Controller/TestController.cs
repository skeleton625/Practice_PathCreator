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

    }
}
