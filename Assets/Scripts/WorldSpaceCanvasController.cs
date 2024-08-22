using UnityEngine;

public class PreciseWorldSpaceCanvasController : MonoBehaviour
{
    public Camera targetCamera;
    public float planeDistance = 1.0f; // Distance from the camera to place the canvas

    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        targetCamera = Camera.main;
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = targetCamera;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();

        // Set the size and position relative to the camera
        float width = targetCamera.pixelWidth;
        float height = targetCamera.pixelHeight;
        float depth = 1000f; // The depth range in which UI elements are active
        canvas.sortingLayerName = "Cursor";
        canvas.sortingOrder = 100;
        // Adjust the canvas size based on camera settings
        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.localScale = new Vector3(0.002737f, 0.002737f, 0.002737f);

        // Position the canvas to fill the camera view
        rectTransform.position = targetCamera.transform.position + targetCamera.transform.forward * planeDistance;
        rectTransform.LookAt(rectTransform.position + targetCamera.transform.rotation * Vector3.forward,
                             targetCamera.transform.rotation * Vector3.up);

    }

    void Update()
    {
        // Optionally, update the positioning in real-time if needed (for moving cameras)
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = targetCamera.transform.position + targetCamera.transform.forward * planeDistance;
        rectTransform.LookAt(rectTransform.position + targetCamera.transform.rotation * Vector3.forward,
                             targetCamera.transform.rotation * Vector3.up);
    }
}
