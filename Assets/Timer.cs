using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    void Update()
    {

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.FloorToInt(FightManagerSingleton.Instance.SecondsOfFight);

        timerText.text = string.Format("{0}", seconds);
    }
}
