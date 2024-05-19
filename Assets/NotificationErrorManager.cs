using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationErrorManager : MonoBehaviour
{
    public GameObject Container;
    public TextMeshProUGUI Text;

    static public NotificationErrorManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void DoNotification(string text)
    {
        Container.SetActive(true);
        Text.text = text;
    }
}