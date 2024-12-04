using System.Collections.Generic;
using UnityEngine;

public class PolygonDrawer : MonoBehaviour
{
    private List<List<Vector3>> polygons = new List<List<Vector3>>();  // Список всех полигонов
    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); // Список всех LineRenderer
    private List<Vector3> currentPolygonPoints = new List<Vector3>();  // Текущий полигон, который отрисовывается

    private const float VertexClickRadius = 0.3f;  // Радиус для выбора вершины
    private Vector3? selectedVertex = null;  // Выбранная вершина для перемещения
    private int selectedPolygonIndex = -1;    // Индекс полигона с выбранной вершиной
    private int selectedVertexIndex = -1;     // Индекс вершины в полигоне

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;  // Оставляем Z-координату 0, чтобы точки были на плоскости

        if (Input.GetMouseButtonDown(0))  // Левый клик мыши
        {
            if (TrySelectVertex(mousePosition))  // Если кликнули на вершину, начинаем перемещение
            {
                selectedVertex = mousePosition;  // Запоминаем исходную позицию вершины
            }
            else
            {
                // Если не кликнули на вершину, создаем новую точку
                currentPolygonPoints.Add(mousePosition);

                // Если это первый полигон, рисуем его, иначе просто обновляем текущий полигон
                if (currentPolygonPoints.Count == 1 && lineRenderers.Count == 0)
                {
                    DrawPolygon(currentPolygonPoints);  // Нарисовать новый полигон
                }
                else
                {
                    // Обновляем только последний рисованный полигон (первоначальный или текущий)
                    UpdatePolygon(lineRenderers.Count - 1, currentPolygonPoints);
                }
            }
        }

        if (Input.GetMouseButton(0) && selectedVertex.HasValue)  // Левый клик удержан (перетягивание)
        {
            // Если вершина выбрана, обновляем ее позицию по движению мыши
            Vector3 offset = mousePosition - selectedVertex.Value;
            MoveVertex(offset);
            selectedVertex = mousePosition;  // Обновляем позицию вершины
        }

        if (Input.GetMouseButtonUp(0))  // Отпускание левой кнопки мыши
        {
            if (selectedVertex.HasValue)
            {
                selectedVertex = null;  // Отпускаем вершину
            }
        }

        // Закрытие полигона (если нужно)
        if (Input.GetKeyDown(KeyCode.C) && currentPolygonPoints.Count > 2)  // Нажимаем "C", чтобы закрыть полигон
        {
            currentPolygonPoints.Add(currentPolygonPoints[0]);  // Добавляем первую точку в конец, чтобы замкнуть полигон
            polygons.Add(new List<Vector3>(currentPolygonPoints));  // Добавляем полигон в список
            UpdatePolygon(lineRenderers.Count - 1, currentPolygonPoints);  // Обновляем последний LineRenderer
            currentPolygonPoints.Clear();  // Очищаем текущие точки
        }
    }

    // Рисует полигон с использованием LineRenderer
    void DrawPolygon(List<Vector3> polygonPoints)
    {
        LineRenderer lineRenderer = new GameObject("Polygon").AddComponent<LineRenderer>();
        lineRenderers.Add(lineRenderer);
        lineRenderer.positionCount = polygonPoints.Count;
        lineRenderer.SetPositions(polygonPoints.ToArray());
        lineRenderer.widthMultiplier = 0.1f;  // Ширина линии
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
    }

    // Обновляем отрисовку полигона после изменения вершины
    void UpdatePolygon(int polygonIndex, List<Vector3> polygonPoints)
    {
        if (polygonIndex >= 0 && polygonIndex < lineRenderers.Count)
        {
            LineRenderer lineRenderer = lineRenderers[polygonIndex];
            lineRenderer.positionCount = polygonPoints.Count;
            lineRenderer.SetPositions(polygonPoints.ToArray());
        }
    }

    // Пытаемся выбрать вершину для перемещения
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
                    return true;  // Вершина выбрана для перемещения
                }
            }
        }
        return false;  // Вершина не найдена, не удалось выбрать
    }

    // Перемещаем вершину, учитывая смещение мыши
    void MoveVertex(Vector3 offset)
    {
        if (selectedPolygonIndex != -1 && selectedVertexIndex != -1)
        {
            List<Vector3> polygon = polygons[selectedPolygonIndex];
            polygon[selectedVertexIndex] += offset;  // Перемещаем выбранную вершину

            // Обновляем LineRenderer
            UpdatePolygon(selectedPolygonIndex, polygon);
        }
    }
}
