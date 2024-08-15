using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonsController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject newGameMenu;
    public GameObject InGameMenu;
    public GameObject optionsButton;

    public Slider VolumeSlider;

    private void Start()
    {
        VolumeSlider.value = AudioListener.volume;
        VolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;

    }

    public void NewGameButton()
    {
        newGameMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMainMenuFromnNewGame()
    {
        newGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OptionsOpen()
    {
        optionsMenu.SetActive(true);
        optionsButton.SetActive(false);
    }

    public void OptionsClose()
    {
        optionsMenu.SetActive(false);
        optionsButton.SetActive(true);
    }


    public bool fullscreenMode = true;

    public void ToggleFullscreen()
    {
        Screen.fullScreen = fullscreenMode;
        fullscreenMode = !fullscreenMode;
    }

    public void BackToMainMenuFromOptions()
    {
        optionsMenu.SetActive(false);
        optionsButton.SetActive(true);
        newGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }


}
