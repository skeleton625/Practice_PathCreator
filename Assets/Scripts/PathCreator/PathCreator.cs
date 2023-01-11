using UnityEngine;

public class PathCreator : MonoBehaviour
{
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

    public void CreatePath()
    {
        path = new Path(transform.position);
    }
}
