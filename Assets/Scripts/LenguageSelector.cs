using TMPro;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
	public TMP_Dropdown languageDropdown;
	public LocalizedTexts[] localizedTexts; // Array que contiene los ScriptableObjects para cada idioma	
	void Start()
	{
		// Configura las opciones del Dropdown con los nombres de los idiomas
		string[] languageNames = new string[localizedTexts.Length];
		for (int i = 0; i < localizedTexts.Length; i++)
		{
			languageNames[i] = localizedTexts[i].languageName; // Suponiendo que tus ScriptableObjects tienen un campo 'languageName' que representa el nombre del idioma
		}
		languageDropdown.ClearOptions();
		languageDropdown.AddOptions(new System.Collections.Generic.List<string>(languageNames));
        
		int defaultLanguageIndex = PlayerPrefs.GetInt("Language",1); // Por ejemplo, si el idioma predeterminado es el primero en tu lista de idiomas
    
		// Establece el idioma inicial
		SetLanguage(defaultLanguageIndex); // Cambia esto al índice del idioma inicial que desees
	}

	// Método para cambiar el idioma
	public void SetLanguage(int index)
	{
		// Asegúrate de que el índice sea válido
		if (index >= 0 && index < localizedTexts.Length)
		{
			// Actualiza el ScriptableObject de texto localizado actual
			UITextController.Instance.localizedTexts = localizedTexts[index];
			PlayerPrefs.SetInt("Language", index);
			PlayerPrefs.Save(); // Guarda los cambios inmediatamente
            
			// Actualiza los textos en la UI
			UITextController.Instance.SetUITexts();
		}
	}
	
}
