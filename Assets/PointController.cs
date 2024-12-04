using UnityEngine;

public class PointController : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool complete_polygon = false;
    public int polygon_index = -1;
    private void OnMouseDown()
    {
        if (complete_polygon)
        {
            gameObject.transform.parent.parent.GetComponentInParent<PolygonController>().prev_index = gameObject.transform.parent.parent.GetComponentInParent<PolygonController>().index;
            gameObject.transform.parent.parent.GetComponentInParent<PolygonController>().index = polygon_index;
        }
    }

    private void OnMouseDrag()
    {
        this.transform.position = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
