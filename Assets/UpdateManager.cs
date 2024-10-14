using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateManager : MonoBehaviour
{
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() => { UpdateGame(); });
    }

    public void UpdateGame()
    {
        if (!string.IsNullOrEmpty(GetSessionIdFromCommandLineArgs()))
        {
            Application.Quit();
            return;
        }
#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("https://drive.google.com/file/d/1bel9CmN1Dx4EsmT0WSj_b6Ioa339MPVM/view?usp=drive_link");
#else
        Application.OpenURL("https://drive.google.com/file/d/1llgt6dqaI6hCF6_4ePoHQz2GFXguTQEk/view?usp=drive_link");
#endif
    }

    private string GetSessionIdFromCommandLineArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-sessionId" && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null; // Return null if no session ID is found
    }
}
