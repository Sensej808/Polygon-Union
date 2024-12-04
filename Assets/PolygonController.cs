using NUnit.Framework;
using System.Drawing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Xml.Schema;

public class PolygonController : MonoBehaviour
{
    public GameObject polygonPref;
    public GameObject pointPref;
    public List<GameObject> polygons;
    public List<GameObject> processed_points;
    public int index = -1;
    public int prev_index = -1;

    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            // Создаём Raycast, который будет проверять наличие объектов в месте, где мы кликаем
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            // Если есть пересечение с каким-либо объектом (например, точкой), не создаём новую точку
            if (hit.collider != null)
            {
                Debug.Log("Point too close to an existing point. Cannot create.");
            }
            else
            {
                Debug.Log("Create point");
                GameObject vertex = Instantiate(pointPref, worldPos, Quaternion.identity);
                processed_points.Add(vertex);
                vertex.transform.SetParent(this.transform);
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreatePolygon(processed_points);
        }
        if (Input.GetKeyDown(KeyCode.I) && prev_index != -1 && index != -1 && index != prev_index)
        {
            //var ps = UnionPolygons(prev_index, index);
            var verts = UnionPoints(index, prev_index);
            CreatePolygon(verts);
        }
    }

    public void CreatePolygon(List<GameObject> vertexes)
    {
        GameObject polygon = Instantiate(polygonPref, this.transform);
        polygon.GetComponent<lineController>().SetUpLines(vertexes);
        polygon.name = "Polygon" + (polygons.Count + 1).ToString();
        polygon.transform.SetParent(this.transform);
        for (int i = 0; i < vertexes.Count; i++)
        {
            vertexes[i].GetComponent<PointController>().complete_polygon = true;
            vertexes[i].GetComponent<PointController>().polygon_index = polygons.Count;
            vertexes[i].transform.SetParent(polygon.transform);
        }
        vertexes.Clear();
        
        polygons.Add(polygon);
    }

    public List<GameObject> UnionPoints(int index_first, int index_second)
    {
        List<GameObject> res_points = new List<GameObject>();
        LineRenderer lr1 = polygons[index_first].GetComponent<LineRenderer>();
        LineRenderer lr2 = polygons[index_second].GetComponent<LineRenderer>();
        for (int i = 0; i < lr1.positionCount - 1; i++)
        {
            if(!IsInsidePolygon(lr1.GetPosition(i), polygons[index_second]))
                res_points.Add(Instantiate(pointPref, lr1.GetPosition(i), Quaternion.identity));
        }
        for (int i = 0; i < lr2.positionCount - 1; i++)
        {
            if (!IsInsidePolygon(lr2.GetPosition(i), polygons[index_first]))
                res_points.Add(Instantiate(pointPref, lr2.GetPosition(i), Quaternion.identity));
        }
        for (int i = 0; i < lr1.positionCount - 1; i++)
        {
            for (int j = 0; j < lr2.positionCount - 1; j++)
            {
                var cross = GetIntersect(lr1.GetPosition(i), lr1.GetPosition(i + 1), lr2.GetPosition(j), lr2.GetPosition(j + 1));
                if (cross.HasValue)
                {
                    res_points.Add(Instantiate(pointPref, cross.Value, Quaternion.identity));
                }
            }
        }
        Vector2 center = CalculateCentroid(res_points);

        res_points.Sort((a, b) => CompareAngles(a.transform.position, b.transform.position, center));
        Debug.Log("Res points: " + res_points.Count);
        return res_points;
    }

    float AngleFromCenter(Vector2 point, Vector2 center)
    {
        return Mathf.Atan2(point.y - center.y, point.x - center.x) * Mathf.Rad2Deg;
    }

    int CompareAngles(Vector2 a, Vector2 b, Vector2 center)
    {
        float angleA = AngleFromCenter(a, center);
        float angleB = AngleFromCenter(b, center);

        // Для сортировки по часовой стрелке
        if (angleA > angleB) return -1; // Если A "ближе" по часовой стрелке
        if (angleA < angleB) return 1;  // Если B "ближе" по часовой стрелке
        return 0; // Если углы одинаковые
    }
    Vector2 CalculateCentroid(List<GameObject> points)
    {
        Vector2 centroid = Vector2.zero;
        foreach (var point in points)
        {
            centroid += (Vector2)point.transform.position;
        }
        return centroid / points.Count;
    }
    public List<GameObject> UnionPolygons(int  index_first, int index_second)
    {
        int t = 0;
        List<GameObject> union = new List<GameObject>();
        int ind = 0;
        LineRenderer lr1 = polygons[index_first].GetComponent<LineRenderer>();
        LineRenderer lr2 = polygons[index_second].GetComponent<LineRenderer>();
        while (IsInsidePolygon(lr1.GetPosition(ind), polygons[index_second])){
            ind++;
            if(ind == polygons[index_first].transform.childCount)
            {
                return union;
            }
           
        }
        union.Add(Instantiate(pointPref, lr1.GetPosition(ind),Quaternion.identity));
        Vector2 current = lr1.GetPosition(ind);
        bool on_first = true; //check if we go on the first polygon
        do
        {
            if (on_first)
            {
                for (int i = 0; i < lr2.positionCount - 1; i++)
                {
                    var cross = GetIntersect(current, lr1.GetPosition(ind + 1), lr2.GetPosition(i), lr2.GetPosition(i + 1));
                    if (cross.HasValue)
                    {
                        current = cross.Value;
                        on_first = false;
                        ind = i;
                        union.Add(Instantiate(pointPref, cross.Value, Quaternion.identity));
                        break;
                    }
                }
                if (on_first)
                {
                    ind++;
                    current = lr1.GetPosition(ind);
                    union.Add(Instantiate(pointPref, lr1.GetPosition(ind), Quaternion.identity));
                }
            }
            else
            {
                for (int i = 0; i < lr1.positionCount - 1; i++)
                {
                    var cross = GetIntersect(current, lr2.GetPosition(ind + 1), lr1.GetPosition(i), lr1.GetPosition(i + 1));
                    if (cross.HasValue)
                    {
                        current = cross.Value;
                        on_first = true;
                        ind = i;
                        union.Add(Instantiate(pointPref, cross.Value, Quaternion.identity));
                        break;
                    }
                }
                if (!on_first)
                {
                    ind++;
                    current = lr2.GetPosition(ind);
                    union.Add(Instantiate(pointPref, lr2.GetPosition(ind), Quaternion.identity));
                }
            }
            t++;
        } while ((Vector2)union[0].transform.position != current && t < 100);
        return union;

    }
    public Vector2? GetIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (denominator == 0)
        {
            return null;
        }

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
        {
            float intersectionX = p1.x + ua * (p2.x - p1.x);
            float intersectionY = p1.y + ua * (p2.y - p1.y);
            return new Vector2(intersectionX, intersectionY);
        }

        return null; 
    }

    public bool IsInsidePolygon(Vector2 point, GameObject p)
    {
        int crossings = 0;
        int n = p.transform.childCount;
        var lr = p.GetComponent<LineRenderer>();

        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = lr.GetPosition(i);
            Vector2 p2 = lr.GetPosition((i + 1) % n);

            if ((p1.y > point.y) != (p2.y > point.y))
            {
                float intersectionX = (float)(p2.x - p1.x) * (point.y - p1.y) / (p2.y - p1.y) + p1.x;
                if (point.x < intersectionX)
                {
                    crossings++;
                }
            }
        }

        return (crossings % 2) == 1;
    }
}
