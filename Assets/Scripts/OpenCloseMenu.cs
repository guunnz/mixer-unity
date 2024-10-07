using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseMenu : MonoBehaviour
{
#if UNITY_STANDALONE
    public GameObject close;
    public GameObject blocker;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            close.SetActive(!close.activeSelf);
            blocker.SetActive(!blocker.activeSelf);
        }
    }
#endif
}
