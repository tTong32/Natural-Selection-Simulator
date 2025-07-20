using UnityEngine;

public class cameraScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [Header("Background Color")]
    Color graphColor, mainColor;

    float mainCameraSize = 15;
    float graphCameraSize = 10;
    float x = 0f, y = 0f;
    string scene = "main";

    public Camera cam;

    void Update()
    {
        if (scene == "main")
        {
            HandleMovement();
            HandleZoom();
        }
    }

    void HandleMovement()
    {
        // WASD / Arrows for X/Z
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Build world-space move vector
        Vector3 move = new Vector3(h, v, 0f) * moveSpeed;
        transform.Translate(move * Time.deltaTime, Space.World);
    }

    void HandleZoom()
    {
        // Mouse scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // Q/E key input: E = zoom in, Q = zoom out
        float keyZoom = 0f;
        if (Input.GetKey(KeyCode.E)) keyZoom = zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) keyZoom = -zoomSpeed * Time.deltaTime;

        float zoomDelta = scroll + keyZoom;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomDelta, minZoom, maxZoom);
    }

    public void setGraphCamera()
    {
        if (ColorUtility.TryParseHtmlString("#BABABA", out graphColor))
        {
            cam.backgroundColor = graphColor;
        }
        cam.orthographicSize = graphCameraSize;
        x = cam.transform.position.x;
        y = cam.transform.position.y;
        Vector3 pos = new Vector3(0f, 0f, 0f);
        transform.Translate(pos, Space.World);
        scene = "graph";
    }

    public void setMainCamera()
    {
        if (ColorUtility.TryParseHtmlString("#314D79", out mainColor))
        {
            GetComponent<Camera>().backgroundColor = mainColor;
        }
        cam.orthographicSize = mainCameraSize;
        Vector3 pos = new Vector3(x, y, 0f);
        transform.Translate(pos, Space.World);
        scene = "main";
    }
}

