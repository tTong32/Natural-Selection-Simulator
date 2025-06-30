using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 10f, height = 7f;
    float lineWidth = 0.1f;
    // these data points act as queues
    Queue<int> numBlobs = new Queue<int>();
    int numBlobsMax = 0;
    public GameObject graphScene;
    public sceneSwitcher sceneSwitch;
    public GameObject pointPrefab;
    List<GameObject> points = new List<GameObject>();

    void Start()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
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
                float x = (float)i / (float)(count - 1) * (float)(width / count);
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
            GameObject point = Instantiate(pointPrefab, new Vector3(0, height, 0), Quaternion.identity, graphScene.transform);
            checkScene(point);
            points.Add(point);
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
            obj.GetComponent<Renderer>().enabled = false;
        }
    }
}
