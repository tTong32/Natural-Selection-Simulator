using UnityEngine;

public class graphScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LineRenderer lineRenderer;
    int numPoints = 100;
    float width = 10f, height = 5f;
    void Start()
    {
        lineRenderer.positionCount = numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            float x = (float)i / (numPoints - 1) * width;
            float y = Mathf.Sin(x); // Example: sine wave
            lineRenderer.SetPosition(i, new Vector3(x, y * height, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
