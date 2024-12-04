using System.Collections.Generic;
using UnityEngine;

public class PolygonDrawer : MonoBehaviour
{
    private List<List<Vector3>> polygons = new List<List<Vector3>>();  // ������ ���� ���������
    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); // ������ ���� LineRenderer
    private List<Vector3> currentPolygonPoints = new List<Vector3>();  // ������� �������, ������� ��������������

    private const float VertexClickRadius = 0.3f;  // ������ ��� ������ �������
    private Vector3? selectedVertex = null;  // ��������� ������� ��� �����������
    private int selectedPolygonIndex = -1;    // ������ �������� � ��������� ��������
    private int selectedVertexIndex = -1;     // ������ ������� � ��������

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;  // ��������� Z-���������� 0, ����� ����� ���� �� ���������

        if (Input.GetMouseButtonDown(0))  // ����� ���� ����
        {
            if (TrySelectVertex(mousePosition))  // ���� �������� �� �������, �������� �����������
            {
                selectedVertex = mousePosition;  // ���������� �������� ������� �������
            }
            else
            {
                // ���� �� �������� �� �������, ������� ����� �����
                currentPolygonPoints.Add(mousePosition);

                // ���� ��� ������ �������, ������ ���, ����� ������ ��������� ������� �������
                if (currentPolygonPoints.Count == 1 && lineRenderers.Count == 0)
                {
                    DrawPolygon(currentPolygonPoints);  // ���������� ����� �������
                }
                else
                {
                    // ��������� ������ ��������� ���������� ������� (�������������� ��� �������)
                    UpdatePolygon(lineRenderers.Count - 1, currentPolygonPoints);
                }
            }
        }

        if (Input.GetMouseButton(0) && selectedVertex.HasValue)  // ����� ���� ������� (�������������)
        {
            // ���� ������� �������, ��������� �� ������� �� �������� ����
            Vector3 offset = mousePosition - selectedVertex.Value;
            MoveVertex(offset);
            selectedVertex = mousePosition;  // ��������� ������� �������
        }

        if (Input.GetMouseButtonUp(0))  // ���������� ����� ������ ����
        {
            if (selectedVertex.HasValue)
            {
                selectedVertex = null;  // ��������� �������
            }
        }

        // �������� �������� (���� �����)
        if (Input.GetKeyDown(KeyCode.C) && currentPolygonPoints.Count > 2)  // �������� "C", ����� ������� �������
        {
            currentPolygonPoints.Add(currentPolygonPoints[0]);  // ��������� ������ ����� � �����, ����� �������� �������
            polygons.Add(new List<Vector3>(currentPolygonPoints));  // ��������� ������� � ������
            UpdatePolygon(lineRenderers.Count - 1, currentPolygonPoints);  // ��������� ��������� LineRenderer
            currentPolygonPoints.Clear();  // ������� ������� �����
        }
    }

    // ������ ������� � �������������� LineRenderer
    void DrawPolygon(List<Vector3> polygonPoints)
    {
        LineRenderer lineRenderer = new GameObject("Polygon").AddComponent<LineRenderer>();
        lineRenderers.Add(lineRenderer);
        lineRenderer.positionCount = polygonPoints.Count;
        lineRenderer.SetPositions(polygonPoints.ToArray());
        lineRenderer.widthMultiplier = 0.1f;  // ������ �����
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
    }

    // ��������� ��������� �������� ����� ��������� �������
    void UpdatePolygon(int polygonIndex, List<Vector3> polygonPoints)
    {
        if (polygonIndex >= 0 && polygonIndex < lineRenderers.Count)
        {
            LineRenderer lineRenderer = lineRenderers[polygonIndex];
            lineRenderer.positionCount = polygonPoints.Count;
            lineRenderer.SetPositions(polygonPoints.ToArray());
        }
    }

    // �������� ������� ������� ��� �����������
    bool TrySelectVertex(Vector3 mousePosition)
    {
        for (int i = 0; i < polygons.Count; i++)
        {
            List<Vector3> polygon = polygons[i];
            for (int j = 0; j < polygon.Count; j++)
            {
                Vector3 vertex = polygon[j];
                if (Vector3.Distance(mousePosition, vertex) < VertexClickRadius)
                {
                    selectedPolygonIndex = i;
                    selectedVertexIndex = j;
                    return true;  // ������� ������� ��� �����������
                }
            }
        }
        return false;  // ������� �� �������, �� ������� �������
    }

    // ���������� �������, �������� �������� ����
    void MoveVertex(Vector3 offset)
    {
        if (selectedPolygonIndex != -1 && selectedVertexIndex != -1)
        {
            List<Vector3> polygon = polygons[selectedPolygonIndex];
            polygon[selectedVertexIndex] += offset;  // ���������� ��������� �������

            // ��������� LineRenderer
            UpdatePolygon(selectedPolygonIndex, polygon);
        }
    }
}
