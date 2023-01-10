using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public void CreatePath()
    {
        path = new Path(transform.position);
    }
}
