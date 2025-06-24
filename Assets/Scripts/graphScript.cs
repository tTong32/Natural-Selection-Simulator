using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 1f, height = 5f;
    // these data points act as queues
    Queue<int> numBlobs = new Queue<int>();
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateGraph()
    {
        int count = Mathf.Min(numPoints, numBlobs.Count);
        lineRenderer.positionCount = count;
        int i = 0;
        foreach (int yValue in numBlobs)
        {
            float x = (float)i / (numPoints - 1) * width;
            float y = (yValue / 10f) * height; // Adjust scaling as needed
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            i++;
            if (i >= count) break;
        }
    }

    public void UpdateData(List<blobScript> blobList)
    {
        manageQueue(numBlobs, blobList.Count);

        UpdateGraph();
    }

    void manageQueue(Queue<float> dataQueue, float value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
    }
    void manageQueue(Queue<int> dataQueue, int value)
    {
        if (dataQueue.Count >= numPoints) dataQueue.Dequeue(); // Remove the oldest value
        dataQueue.Enqueue(value);
    }
}
