using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 16f, height = 9f;
    float xTranslate = 0f, yTranslate = 0f;
    float lineWidth = 0.1f;
    // these data points act as queues

    Queue<int> statListInt = new Queue<int>();
    int maxValueInt = 0;
    Queue<float> statListFloat = new Queue<float>();
    float maxValueFloat = 0f;

    List<GameObject> points = new List<GameObject>();
    List<TMPro.TextMeshPro> yLabels = new List<TMPro.TextMeshPro>();

    GameObject graphScene;
    sceneSwitcher sceneSwitch;
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

        // find gameObjects
        graphScene = GameObject.Find("GraphScene");
        sceneSwitch = GameObject.Find("SceneSwitcher").GetComponent<sceneSwitcher>();

        // Initialize the axis
        GameObject xAxis = Instantiate(axisPrefab, new Vector3(xTranslate - 2f, yTranslate, 0), Quaternion.identity, this.transform);
        xAxis.transform.localScale = new Vector3(width, 0.1f, 1f);

        GameObject yAxis = Instantiate(axisPrefab, new Vector3(-10f + xTranslate, yTranslate + height / 2, 0), Quaternion.identity, this.transform);
        yAxis.transform.localScale = new Vector3(0.1f, height, 1f);

        DrawYAxisLabels(5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateData(int data)
    {
        maxValueInt = manageQueue(statListInt, maxValueInt, data);
        UpdateGraphInt();
    }

    public void UpdateData(float data)
    {
        maxValueFloat = manageQueue(statListFloat, maxValueFloat, data);
        UpdateGraphFloat();
    }

    void UpdateGraphInt()
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
                points.Add(point);
                i++;
                if (i >= count) break;
            }
        }
        else
        {
            GameObject point = Instantiate(pointPrefab, new Vector3(-10 + xTranslate, height + yTranslate, 0), Quaternion.identity, this.transform);
            points.Add(point);
        }

        updateTicksInt();
    }

        void UpdateGraphFloat()
        {
            foreach (var p in points) Destroy(p);
            points.Clear();

            int count = Mathf.Min(numPoints, statListFloat.Count);
        if (count > 1)
        {
            lineRenderer.positionCount = count;
            int i = 0;
            foreach (float yValue in statListFloat)
            {
                float x = -10f + xTranslate + (float)i / (count - 1) * width;
                float y = yTranslate + (float)yValue / maxValueFloat * height;  // Adjust scaling as needed
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));

                GameObject point = Instantiate(pointPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                points.Add(point);
                i++;
                if (i >= count) break;
            }
        }
        else
        {
            GameObject point = Instantiate(pointPrefab, new Vector3(-10 + xTranslate, height + yTranslate, 0), Quaternion.identity, this.transform);
            points.Add(point);
        }

            updateTicksFloat();
        }

    void DrawYAxisLabels(int numLabels)
    {
        float labelInterval = height / (numLabels - 1);
        for (int i = 0; i < numLabels; i++)
        {
            float y = yTranslate + i * labelInterval;
            float x = -10.5f + xTranslate;

            GameObject label = Instantiate(axisLabelPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
            TMPro.TextMeshPro text = label.GetComponent<TMPro.TextMeshPro>();
            text.text = $"{i * (maxValueInt / (numLabels - 1))}";
            text.color = Color.black;

            yLabels.Add(text);

            if (i != 0)
            {
                GameObject tick = Instantiate(axisTickPrefab, new Vector3(-10f + xTranslate, y, 0), Quaternion.identity, this.transform);
            }
        }
    }

    void updateTicksInt()
    {
        int n = yLabels.Count;
        for (int i = 0; i < n; i++)
        {
            yLabels[i].text = $"{i * (maxValueInt / (n - 1))}";
        }
    }

    void updateTicksFloat()
    {
        int n = yLabels.Count;
        for (int i = 0; i < n; i++)
        {
            yLabels[i].text = $"{i * (maxValueFloat / (float)(n - 1)):0.000}";
        }
    }

    float manageQueue(Queue<float> dataQueue, float maxValueFloat, float value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValueFloat) maxValueFloat = value;
        return maxValueFloat;
    }
    int manageQueue(Queue<int> dataQueue, int maxValueInt, int value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValueInt) maxValueInt = value;
        return maxValueInt;
    }

    public void setCenter(float x, float y)
    {
        xTranslate = x;
        yTranslate = y;
    }
}
