using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ResolutionMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public GameObject FullScreen;

    private Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> resolutionOptions = new List<TMP_Dropdown.OptionData>();
        int currentResolutionIndex = 0;
        string savedResolution = PlayerPrefs.GetString("SavedResolution", "");

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            string option = $"{resolution.width}x{resolution.height} @ {resolution.refreshRate}Hz";

            resolutionOptions.Add(new TMP_Dropdown.OptionData(option));

            // Save the resolution option with the refresh rate
            if (option == savedResolution)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Attach the SetResolution method to the dropdown's onValueChanged event
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

#if UNITY_ANDROID || UNITY_IOS
        resolutionDropdown.gameObject.SetActive(false);
        FullScreen.SetActive(false);
#endif
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        // Corrected the method call to use Screen.fullScreenMode instead of Screen.fullScreen
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, resolution.refreshRate);
        // Save the selected resolution with the refresh rate
        string option = $"{resolution.width}x{resolution.height} @ {resolution.refreshRate}Hz";
        PlayerPrefs.SetString("SavedResolution", option);
        PlayerPrefs.Save();
    }

}
