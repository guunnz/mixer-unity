using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionFixer : MonoBehaviour
{
    // Target aspect ratio (1200 / 1920 = 0.625)
    private float targetAspectRatio = 1200f / 1920f;

    // Tolerance for comparing floating point values
    public float tolerance = 0.05f;
    public CanvasScaler canvasScaler;
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

    private void Start()
    {
        if (IsTargetAspectRatio())
        {
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, 468);
        }
    }
}
