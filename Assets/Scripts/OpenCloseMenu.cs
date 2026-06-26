using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseMenu : MonoBehaviour
{
#if UNITY_STANDALONE
    public GameObject close;
    public GameObject blocker;
    private bool warnedMissingClose;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (close == null)
            {
                if (!warnedMissingClose)
                {
                    Debug.LogWarning("OpenCloseMenu is missing its close reference.", this);
                    warnedMissingClose = true;
                }

                return;
            }

            close.SetActive(!close.activeSelf);
            if (blocker != null)
                blocker.SetActive(!blocker.activeSelf);
        }
    }
#endif
}
