using UnityEngine;
using System.Collections.Generic;

public class shelterScript : MonoBehaviour
{
    // for now, a cave of scale 1 can fit 1f radiuses
    // scale is the average between x and y
    public float sizeCondition;
    float maxCapacity;
    public float capacity;
    public List<blobScript> blobsInside;

    void Start()
    {
        // Set the sizeCondition based on the scale of the shelter
        sizeCondition = (transform.localScale.x + transform.localScale.y) / 2 * 0.64f;
        maxCapacity = sizeCondition * Random.Range(1f, 4f);
        capacity = maxCapacity;
        blobsInside = new List<blobScript>();
    }

    public void enter(float size, blobScript blob)
    {
        capacity -= size;
        blobsInside.Add(blob);
    }

    public void exit(float size, blobScript blob)
    {
        capacity += size;
        if (capacity > maxCapacity) capacity = maxCapacity; // Ensure capacity does not exceed maxCapacity
        blobsInside.Remove(blob);
    }
}
