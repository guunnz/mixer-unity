using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizedTexts", menuName = "Localization/Localized Texts")]
public class LocalizedTexts : ScriptableObject
{
    public string languageName;
    [Header("Main Menu")]
    public string[] menuTexts;

    [Header("New Game")]
    public string[] newGameTexts;

    [Header("Options Menu")]
    public string[] optionsMenuTexts;

    [Header( "In Game" )]
    public string[] ingText;

    // Agrega más arrays para otros textos que necesites
}
