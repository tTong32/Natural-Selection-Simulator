using UnityEngine;
using System.Collections.Generic;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    public float width = 10f, height = 0.5f;
    public float lineWidth = 0.1f;
    // these data points act as queues
    Queue<int> numBlobs = new Queue<int>();
    int numBlobsMax = 0f;

    void Start()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
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
        int count = Mathf.Min(numPoints, numBlobs.Count);
        if (count > 1)
        {
            lineRenderer.positionCount = count;
            int i = 0;
            foreach (int yValue in numBlobs)
            {
                float x = (float)i / (count - 1) * width;
                float y = (float)yValue * height;  // Adjust scaling as needed
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
                i++;
                if (i >= count) break;
            }
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
}
