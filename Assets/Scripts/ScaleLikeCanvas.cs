using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleLikeCanvas : MonoBehaviour
{
    private int lastVerticalResolution;

    private void Start()
    {
        lastVerticalResolution = Screen.height;

        this.transform.localScale = new Vector3(1 * (lastVerticalResolution / 1080), 1 * (lastVerticalResolution / 1080), 1 * (lastVerticalResolution / 1080));
    }
    void Update()
    {
        if (lastVerticalResolution != Screen.height)
        {
            this.transform.localScale = new Vector3(1 * (lastVerticalResolution / 1080), 1 * (lastVerticalResolution / 1080), 1 * (lastVerticalResolution / 1080));
        }
    }
}
