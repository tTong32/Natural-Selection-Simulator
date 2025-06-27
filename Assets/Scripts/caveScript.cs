using UnityEngine;

public class caveScript : MonoBehaviour
{
    // for now, a cave of scale 1 can fit 1f radiuses
    // scale is the average between x and y
    public float sizeCondition;

    void Start()
    {
        // Set the sizeCondition based on the scale of the cave
        sizeCondition = (transform.localScale.x + transform.localScale.y) / 2f; // Assuming uniform scaling for x and y
    }
}
