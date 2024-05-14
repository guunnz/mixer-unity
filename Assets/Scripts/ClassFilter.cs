using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassFilter : MonoBehaviour
{
    public AxieClass filter;
    public TeamBuilderManager teamBuilderManager;
    private Button button;
    public TextMeshProUGUI ClassText;
    public Image ClassImage;
    private bool selected => ClassImage.color.a >= 1;

    private void Awake()
    {
        button = GetComponent<Button>();


        button.onClick.AddListener(DoClass);
        teamBuilderManager.axieClassfilterClearedEvent += DoDisabledGraphics;
    }

    private void OnDestroy()
    {
        teamBuilderManager.axieClassfilterClearedEvent -= DoDisabledGraphics;
    }

    private void DoDisabledGraphics()
    {
        ClassText.color = new Color(ClassText.color.r, ClassText.color.g, ClassText.color.b, 0.5f);
        ClassImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void DoClass()
    {
        teamBuilderManager.ToggleClassFilter(filter);
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