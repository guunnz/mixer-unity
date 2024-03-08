using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextController : MonoBehaviour
{
    public static UITextController Instance;
    public LocalizedTexts localizedTexts;
    // main Menu
    public TextMeshProUGUI menuNewGame;
    public TextMeshProUGUI menuContinue;
    public TextMeshProUGUI menuLeaderboard;
    public TextMeshProUGUI menuQuit;
    [Space(10)]
    //NewGame
    public TextMeshProUGUI titleTutorial;
    public TextMeshProUGUI descTutorial;
    public TextMeshProUGUI titleBack;
    public TextMeshProUGUI descBack;
    public TextMeshProUGUI titlePayRun;
    public TextMeshProUGUI descPayRun;
    public TextMeshProUGUI titleFreeRun;
    public TextMeshProUGUI descFreeRun;
    [Space( 10 )]
    //OptionsMenu
    public TextMeshProUGUI optResolution;
    public TextMeshProUGUI optGraphics;
    public TextMeshProUGUI optFullScreen;
    public TextMeshProUGUI optMusic;
    public TextMeshProUGUI optSFX;
    public TextMeshProUGUI optBackMenu;
    [Space( 10 )]
    //In Game Overlay
    public TextMeshProUGUI ingTeam;
    public TextMeshProUGUI ingFight;
    
    
    void Awake()
    {
        // Configura la instancia estática
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetUITexts();
    }

    public void SetUITexts()
    {
        // Configura los textos para el idioma actual
        menuNewGame.text = localizedTexts.menuTexts[0];
        menuContinue.text = localizedTexts.menuTexts[1];
        menuLeaderboard.text = localizedTexts.menuTexts[2];
        menuQuit.text = localizedTexts.menuTexts[3];

        titleTutorial.text = localizedTexts.newGameTexts[0];
        descTutorial.text = localizedTexts.newGameTexts[1];
        titleBack.text = localizedTexts.newGameTexts[2];
        descBack.text = localizedTexts.newGameTexts[3];
        titlePayRun.text = localizedTexts.newGameTexts[4];
        descPayRun.text = localizedTexts.newGameTexts[5];
        titleFreeRun.text = localizedTexts.newGameTexts[6];
        descFreeRun.text = localizedTexts.newGameTexts[7];

        optResolution.text = localizedTexts.optionsMenuTexts[0];
        optGraphics.text = localizedTexts.optionsMenuTexts[1];
        optFullScreen.text = localizedTexts.optionsMenuTexts[2];
        optMusic.text = localizedTexts.optionsMenuTexts[3];
        optSFX.text = localizedTexts.optionsMenuTexts[4];
        optBackMenu.text = localizedTexts.optionsMenuTexts[5];

        ingFight.text = localizedTexts.ingText[0];
        ingTeam.text = localizedTexts.ingText[1];
    }

    
    
}