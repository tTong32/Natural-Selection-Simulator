using UnityEngine;

public class cameraScript : MonoBehaviour
{

    public float moveSpeed = 5.0f;
    public float rotationSpeed = 2.0f;
    public bool useWASD = true; // Allows switching between WASD and arrow keys

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // Determine input source based on useWASD flag
        float horizontalInput, verticalInput;

        if (useWASD)
        {
            horizontalInput = Input.GetAxis("Horizontal"); // A and D keys
            verticalInput = Input.GetAxis("Vertical");   // W and S keys
        }
        else
        {
            // Arrow keys
            horizontalInput = Input.GetAxis("HorizontalArrow");
            verticalInput = Input.GetAxis("VerticalArrow");
        }


        // Camera movement
        transform.Translate(new Vector3(horizontalInput * moveSpeed * Time.deltaTime,
                                        0,
                                        verticalInput * moveSpeed * Time.deltaTime));
        */
    }
}
