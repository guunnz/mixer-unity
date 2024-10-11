using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHider : MonoBehaviour
{
    private List<Canvas> activeCanvases = new List<Canvas>();
    private List<Rectangle> activeRectangles = new List<Rectangle>();
    private bool toggling = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            PlayerPrefs.SetInt("Tutorial", 0);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (toggling)
            {
                HideCanvases();
            }
            else
            {
                ShowCanvases();
            }

            toggling = !toggling;
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string fileName = "Screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string filePath = System.IO.Path.Combine(directory, fileName);
        ScreenCapture.CaptureScreenshot(filePath);
        Debug.Log("Screenshot taken and saved to: " + filePath);
    }
    // Function to hide all active canvases
    public void HideCanvases()
    {
        // Find all active canvases at the start and store them in a list
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Rectangle[] rectangles = FindObjectsOfType<Rectangle>();
        activeCanvases.Clear();
        activeRectangles.Clear();

        foreach (Rectangle rectangle in rectangles)
        {
            if (rectangle.enabled) // Add only enabled canvases
            {
                activeRectangles.Add(rectangle);
            }
        }

        foreach (Rectangle canvas in activeRectangles)
        {
            canvas.enabled = false;
        }


        foreach (Canvas canvas in canvases)
        {
            if (canvas.enabled) // Add only enabled canvases
            {
                activeCanvases.Add(canvas);
            }
        }

        foreach (Canvas canvas in activeCanvases)
        {
            canvas.enabled = false;
        }
    }

    // Function to show all previously active canvases
    public void ShowCanvases()
    {
        foreach (Canvas canvas in activeCanvases)
        {
            canvas.enabled = true;
        }

        foreach (Rectangle canvas in activeRectangles)
        {
            canvas.enabled = true;
        }
    }
}
