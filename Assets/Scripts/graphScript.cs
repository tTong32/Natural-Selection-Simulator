using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 20f, height = 7f;
    float xTranslate = 0f, yTranslate = 0f;
    float lineWidth = 0.1f;
    // these data points act as queues

    Queue<int> statListInt = new Queue<int>();
    int maxValueInt = 0;
    Queue<float> statListFloat = new Queue<float>();
    float maxValueFloat = 0f;

    List<GameObject> points = new List<GameObject>();
    List<TMPro.TextMeshPro> yLabels = new List<TMPro.TextMeshPro>();

    public GameObject graphScene;
    public sceneSwitcher sceneSwitch;
    public GameObject pointPrefab;
    public GameObject axisPrefab;
    public GameObject axisLabelPrefab;
    public GameObject axisTickPrefab;

    void Start()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        // Initialize the axis
        GameObject xAxis = Instantiate(axisPrefab, new Vector3(xTranslate, yTranslate, 0), Quaternion.identity, this.transform);
        xAxis.transform.localScale = new Vector3(width, 0.1f, 1f);
        checkScene(xAxis);

        GameObject yAxis = Instantiate(axisPrefab, new Vector3(-10f + xTranslate, yTranslate + height / 2, 0), Quaternion.identity, this.transform);
        yAxis.transform.localScale = new Vector3(0.1f, height, 1f);
        checkScene(yAxis);
        
        DrawYAxisLabels(5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateData(int data)
    {
        maxValueInt = manageQueue(statListInt, maxValueInt, data);
        UpdateGraph();
    }

    public void UpdateData(float data)
    {
        maxValueFloat = manageQueue(statListFloat, maxValueFloat, data);
        UpdateGraph();
    }

    void UpdateGraph()
    {
        foreach (var p in points) Destroy(p);
        points.Clear();

        int count = Mathf.Min(numPoints, statListInt.Count);
        if (count > 1)
        {
            lineRenderer.positionCount = count;
            int i = 0;
            foreach (int yValue in statListInt)
            {
                float x = -10f + xTranslate + (float)i / (count - 1) * width;
                float y = yTranslate + (float)yValue / maxValueInt * height;  // Adjust scaling as needed
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));

                GameObject point = Instantiate(pointPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                checkScene(point);
                points.Add(point);
                i++;
                if (i >= count) break;
            }
        }
        else
        {
            GameObject point = Instantiate(pointPrefab, new Vector3(-10 + xTranslate, height + yTranslate, 0), Quaternion.identity, this.transform);
            checkScene(point);
            points.Add(point);
        }

        updateTicks();
    }

    void DrawYAxisLabels(int numLabels)
    {
        float labelInterval = height / (numLabels - 1);
        for (int i = 0; i < numLabels; i++)
        {
            float y = yTranslate + i * labelInterval;
            float x = -10.5f + xTranslate;
        
            GameObject label = Instantiate(axisLabelPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
            checkScene(label);
            TMPro.TextMeshPro text = label.GetComponent<TMPro.TextMeshPro>();
            text.text = $"{i * (maxValueInt / (numLabels - 1))}";
            text.color = Color.black;
            
            yLabels.Add(text);

            if (i != 0)
            {
                GameObject tick = Instantiate(axisTickPrefab, new Vector3(-10f + xTranslate, y, 0), Quaternion.identity, this.transform);
                checkScene(tick);
            }
        }
    }

    void updateTicks()
    {
        int n = yLabels.Count;
        for (int i = 0; i < n; i++)
        {
            yLabels[i].text = $"{i * (maxValueInt / (n - 1))}";
        }
    }

    float manageQueue(Queue<float> dataQueue, float maxValueInt, float value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValueInt) maxValueInt = value;
        return maxValueInt;
    }
    int manageQueue(Queue<int> dataQueue, int maxValueInt, int value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValueInt) maxValueInt = value;
        return maxValueInt;
    }

    void checkScene(GameObject obj)
    {
        if (sceneSwitch.currentScene != "graph")
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
            else
            {
                var tmp = obj.GetComponent<TMPro.TextMeshPro>();
                if (tmp != null)
                    tmp.enabled = false;
            }
        }
    }
}
