using UnityEngine;
using System.Collections.Generic;

public class lineController : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private List<GameObject> points = new List<GameObject>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetUpLines(List<GameObject> points)
    {
        
        lineRenderer.positionCount = points.Count + 1;
        this.points = new List<GameObject>(points);
    }

    private void Update()
    {
        
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].transform.position);
        }
        lineRenderer.SetPosition(points.Count, points[0].transform.position);
    }
}
