using UnityEngine;

public class PathCreator : MonoBehaviour
{
    public event System.Action bezierCreated;

    [Header("Path Create Setting")]
    [SerializeField] private RoadMeshCreator MeshCreator = null;
    [SerializeField] private bool AutoUpdate = false;

    [Header("Path Editor Setting")]
    public Color anchorColor = Color.red;
    public Color controlColor = Color.white;
    public Color controlLineColor = Color.yellow; 
    public Color segmentColor = Color.green;
    public Color sgementSelectedColor = Color.red;
    public float anchorDiameter = .1f;
    public float controlDiameter = .05f;
    public bool displayControlPoints = true;

    [HideInInspector] public Path path;

    private void Start()
    {
        CreatePath();
    }

    private void Update()
    {
        if (AutoUpdate)
            MeshCreator.UpdateRoadMesh(path);
    }

    public void CreatePath()
    {
        path = new Path(transform.position, true);
        if (bezierCreated != null)
            bezierCreated();
    }

    public void SetActiveVisual(bool isActive)
    {
        MeshCreator.SetActiveVisual(isActive);
        AutoUpdate = isActive;
    }

    public void RefreshRoad()
    {
        MeshCreator.UpdateRoadMesh(path);
    }

    public void GenerateRoad()
    {
        MeshCreator.GenerateRoad();
    }
}
