using TMPro;
using UnityEngine;

public class TextTruncator : MonoBehaviour
{
    private TMP_Text textMeshPro;
    public int maxLength = 16; // Set your desired maximum length

    void Start()
    {
        // Ensure the TMP_Text component is assigned
        if (textMeshPro == null)
            textMeshPro = GetComponent<TMP_Text>();

        // Check if the text length exceeds the maximum length
        if (textMeshPro.text.Length > maxLength)
        {
            // Truncate the text and append an ellipsis
            textMeshPro.text = textMeshPro.text.Substring(0, maxLength) + "...";
        }
    }
}