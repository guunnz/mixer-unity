using UnityEngine;
using UnityEngine.UI;

public class ForceResolutionAndUI : MonoBehaviour
{
    // Set your target aspect ratio (width / height), e.g., 16:9 for 1920x1080
    public float targetAspect = 16f / 9f;

    private Camera mainCamera;
    private Canvas[] allCanvases;
    // Target aspect ratio (1200 / 1920 = 0.625)
    private float targetAspectRatio = 1200f / 1920f;

    // Tolerance for comparing floating point values
    public float tolerance = 0.01f;

    // Function that checks if the current aspect ratio matches the target
    public bool IsTargetAspectRatio()
    {
        // Get the current screen aspect ratio
        float screenAspectRatio = (float)Screen.width / (float)Screen.height;

        // Check if the current aspect ratio is within the tolerance range of the target aspect ratio
        if (Mathf.Abs(screenAspectRatio - targetAspectRatio) < tolerance)
        {
            return true;
        }

        return false;
    }
    void Start()
    {
        // Get the main camera
        mainCamera = Camera.main;

        // Find all canvases in the scene, including disabled ones
        allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();

        // Adjust both camera and UI aspect ratios
        AdjustCameraAspect();
        AdjustUIAspect();
    }

    void AdjustCameraAspect()
    {
        // Get the current screen aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        // Calculate the scaling factor based on the difference between current and target aspect ratio
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f) // Letterbox (add black bars top/bottom)
        {
            Rect rect = mainCamera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            mainCamera.rect = rect;
        }
        else // Pillarbox (add black bars left/right)
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = mainCamera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            mainCamera.rect = rect;
        }
    }

    void AdjustUIAspect()
    {
        // Get the current screen aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        // Calculate the scaling factor based on the difference between current and target aspect ratio
        float scaleHeight = windowAspect / targetAspect;

        // Loop through all canvases in the scene
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                continue; // Skip WorldSpace canvases and any missing references

            // Check if the canvas has a CanvasScaler and is using Scale With Screen Size
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();

                if (scaleHeight < 1.0f) // Letterbox (black bars top/bottom)
                {
                    canvasRect.localScale = new Vector3(1.0f, scaleHeight, 1.0f);
                }
                else // Pillarbox (black bars left/right)
                {
                    float scaleWidth = 1.0f / scaleHeight;
                    canvasRect.localScale = new Vector3(scaleWidth, 1.0f, 1.0f);
                }
            }
        }
    }

    void Update()
    {
        // Recalculate aspect ratio adjustments if the screen size changes
        AdjustCameraAspect();
        AdjustUIAspect();
    }
}
