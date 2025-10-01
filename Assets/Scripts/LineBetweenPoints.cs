using UnityEngine;

[ExecuteInEditMode]
public class LineBetweenPoints : MonoBehaviour
{
    [SerializeField] private Transform pt1;
    [SerializeField] private Transform pt2;

    LineRenderer lineRenderer;

    // Update is called once per frame
    void Update()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        else
        {
            if ((pt1) && (pt2))
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, pt1.position);
                lineRenderer.SetPosition(1, pt2.position);
            }
        }
    }
}
