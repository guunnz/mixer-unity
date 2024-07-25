using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsFilter : MonoBehaviour
{
    public TeamBuilderManager.AxieStatFilter filter;
    public TeamBuilderManager teamBuilderManager;
    private Button button;
    public TextMeshProUGUI ClassText;
    public Image ClassImage;
    private bool selected => ClassImage.color.a >= 1;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(DoStat);
        teamBuilderManager.statsfilterClearedEvent += DoDisabledGraphics;
        teamBuilderManager.contraryfilterClearedEvent += DoDisabledGraphicsContrary;
    }

    private void OnDestroy()
    {
        teamBuilderManager.statsfilterClearedEvent -= DoDisabledGraphics;
        teamBuilderManager.contraryfilterClearedEvent -= DoDisabledGraphicsContrary;
    }

    private void DoDisabledGraphicsContrary(TeamBuilderManager.AxieStatFilter filter)
    {
        if (filter == this.filter)
        {
            ClassText.color = new Color(ClassText.color.r, ClassText.color.g, ClassText.color.b, 0.5f);
            ClassImage.color = new Color(1, 1, 1, 0.5f);
        }
    }

    private void DoDisabledGraphics()
    {
        ClassText.color = new Color(ClassText.color.r, ClassText.color.g, ClassText.color.b, 0.5f);
        ClassImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void DoStat()
    {
        teamBuilderManager.ToggleStatsFilter(filter);
        if (selected)
        {
            ClassText.color = new Color(ClassText.color.r, ClassText.color.g, ClassText.color.b, 0.5f);
            ClassImage.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            ClassText.color = new Color(ClassText.color.r, ClassText.color.g, ClassText.color.b, 1f);
            ClassImage.color = new Color(1, 1, 1, 1f);
        }
    }
}