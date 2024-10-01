using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Version : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<TextMeshProUGUI>().text = Application.version;
    }
}
