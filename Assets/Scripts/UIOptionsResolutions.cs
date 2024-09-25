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
        int currentResolutionIndex = 0;  // Default to first resolution if current not found

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            string option = resolution.width + "x" + resolution.height;
            resolutionOptions.Add(new TMP_Dropdown.OptionData(option));

            // Check if the current iteration resolution matches the screen's resolution
            if (resolution.width == Screen.currentResolution.width &&
                resolution.height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;  // Set to current resolution
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
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
