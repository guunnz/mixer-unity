using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

[System.Serializable]
public class ButtonSFX
{
    public enum ButtonType
    {
        Tap,
        Cancel,
        Confirm
    }

    public ButtonType buttonType;
    public Button buttonObject;
    internal Guid buttonId = Guid.NewGuid();
}

public class ButtonsSFXManager : MonoBehaviour
{

    public List<ButtonSFX> buttons = new List<ButtonSFX>();

    private void Start()
    {
        buttons.ForEach(x => x.buttonObject.onClick.AddListener(() => PlaySound(x.buttonId)));
    }

    public void PlaySound(Guid buttonId)
    {
        ButtonSFX button = buttons.FirstOrDefault(x => x.buttonId == buttonId);


        switch (button.buttonType)
        {
            case ButtonSFX.ButtonType.Confirm:
                SFXManager.instance.PlaySFX(SFXType.UIButtonConfirm,0.12f);
                break;
            case ButtonSFX.ButtonType.Cancel:
                SFXManager.instance.PlaySFX(SFXType.UIButtonCancel, 0.12f);
                break;
            case ButtonSFX.ButtonType.Tap:
                SFXManager.instance.PlaySFX(SFXType.UIButtonTap, 0.12f);
                break;
            default:
                break;
        }
    }
}
