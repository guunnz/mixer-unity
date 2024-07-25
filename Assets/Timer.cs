using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    //
    private void OnEnable()
    {
        elapsedTime = 0;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.FloorToInt(elapsedTime);

        timerText.text = string.Format("{0}", seconds);
    }
}
