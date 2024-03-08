using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResolutionMenu : MonoBehaviour
{
	public TMP_Dropdown resolutionDropdown;

	private Resolution[] resolutions;

	void Start()
	{
		// Obtiene todas las resoluciones disponibles y las almacena en el arreglo
		resolutions = Screen.resolutions;

		// Limpia las opciones actuales del Dropdown
		resolutionDropdown.ClearOptions();

		// Lista para almacenar las opciones de resolución en formato string
		List<TMP_Dropdown.OptionData> resolutionOptions = new List<TMP_Dropdown.OptionData>();

		// Itera a través de las resoluciones y las agrega a la lista
		foreach (var resolution in resolutions)
		{
			string option = resolution.width + "x" + resolution.height;
			resolutionOptions.Add(new TMP_Dropdown.OptionData(option));
		}

		// Agrega las opciones al Dropdown
		resolutionDropdown.options = resolutionOptions;

		// Configura la resolución actual del juego como opción seleccionada
		resolutionDropdown.value = GetCurrentResolutionIndex();
		resolutionDropdown.RefreshShownValue();
	}

	// Método para aplicar la resolución seleccionada
	public void SetResolution(int resolutionIndex)
	{
		Resolution resolution = resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	// Método para obtener el índice de la resolución actual
	private int GetCurrentResolutionIndex()
	{
		Resolution currentResolution = Screen.currentResolution;

		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == currentResolution.width &&
			    resolutions[i].height == currentResolution.height)
			{
				return i;
			}
		}

		return 0;
	}
}
