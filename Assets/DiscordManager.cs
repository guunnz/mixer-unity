using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscordManager : MonoBehaviour
{
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() => { UpdateGame(); });
    }

    public void UpdateGame()
    {
        Application.OpenURL("https://discord.gg/K4pGMeT2fF");
    }
}
