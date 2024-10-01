using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHider : MonoBehaviour
{
    private List<Canvas> activeCanvases = new List<Canvas>();
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
    }

    // Function to hide all active canvases
    public void HideCanvases()
    {
        // Find all active canvases at the start and store them in a list
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        activeCanvases.Clear();
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
    }
}
