using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 20f, height = 7f;
    float lineWidth = 0.1f;
    // these data points act as queues

    Queue<int> numBlobs = new Queue<int>();
    int numBlobsMax = 0;

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
        GameObject xAxis = Instantiate(axisPrefab, new Vector3(0, 0, 0), Quaternion.identity, graphScene.transform);
        xAxis.transform.localScale = new Vector3(width, 0.1f, 1f);
        checkScene(xAxis);

        GameObject yAxis = Instantiate(axisPrefab, new Vector3(-10f, height / 2, 0), Quaternion.identity, graphScene.transform);
        yAxis.transform.localScale = new Vector3(0.1f, height, 1f);
        checkScene(yAxis);
        
        DrawYAxisLabels(5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateData(List<blobScript> blobList)
    {
        numBlobsMax = manageQueue(numBlobs, numBlobsMax, blobList.Count);

        UpdateGraph();
    }

    void UpdateGraph()
    {
        foreach (var p in points) Destroy(p);
        points.Clear();

        int count = Mathf.Min(numPoints, numBlobs.Count);
        if (count > 1)
        {
            lineRenderer.positionCount = count;
            int i = 0;
            foreach (int yValue in numBlobs)
            {
                float x = -10f + (float)i / (count - 1) * width;
                float y = (float)yValue / numBlobsMax * height;  // Adjust scaling as needed
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));

                GameObject point = Instantiate(pointPrefab, new Vector3(x, y, 0), Quaternion.identity, graphScene.transform);
                checkScene(point);
                points.Add(point);
                i++;
                if (i >= count) break;
            }
        }
        else
        {
            GameObject point = Instantiate(pointPrefab, new Vector3(-10, height, 0), Quaternion.identity, graphScene.transform);
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
            float y = i * labelInterval;
        
            GameObject label = Instantiate(axisLabelPrefab, new Vector3(-10.5f, y, 0), Quaternion.identity, graphScene.transform);
            checkScene(label);
            TMPro.TextMeshPro text = label.GetComponent<TMPro.TextMeshPro>();
            text.text = $"{i * (numBlobsMax / (numLabels - 1))}";
            text.color = Color.black;
            Color c = text.color;
            c.a = 1f;
            text.color = c;
            
            yLabels.Add(text);

            if (i != 0)
            {
                GameObject tick = Instantiate(axisTickPrefab, new Vector3(-10f, y, 0), Quaternion.identity, graphScene.transform);
                checkScene(tick);
            }
        }
    }

    void updateTicks()
    {
        int n = yLabels.Count;
        for (int i = 0; i < n; i++)
        {
            yLabels[i].text = $"{i * (numBlobsMax / (n - 1))}";
        }
    }

    float manageQueue(Queue<float> dataQueue, float maxValue, float value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValue) maxValue = value;
        return maxValue;
    }
    int manageQueue(Queue<int> dataQueue, int maxValue, int value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
        if (value > maxValue) maxValue = value;
        return maxValue;
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
