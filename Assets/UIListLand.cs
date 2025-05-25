using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class SpriteLand
{
    public LandType landType;
    public Sprite landSprite;
}



public class UIListLand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<SpriteLand> spriteLandList = new List<SpriteLand>();
    public GetAxiesExample.Land land;
    public TeamBuilderManager teamBuilderManager;
    public Image landImage;
    public Image selectedImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public FakeLandManager fakeLandManager;
    public Image freeRotation;
    private Button button;
    private bool free;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectLand);

        if (land == null || land.locked)
        {
            landImage.color = new Color(0.4f, 0.4f, 0.4f, 1);
        }

        if (land != null && land.row == "0" && land.col == "0")
        {
            free = true;
            freeRotation.enabled = true;
        }
    }

    private void SelectLand()
    {
        if (land == null || land.locked)
        {
            return;
        }
        fakeLandManager.ChooseFakeLand(land.token_id);
        teamBuilderManager.SetLandUI();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (free)
        {
            TooltipManagerSingleton.instance.EnableTooltip(TooltipType.FreeLand);
        }
        TooltipManagerSingleton.instance.EnableTooltip((TooltipType)Enum.Parse(typeof(TooltipType), land.LandTypeEnum.ToString(), true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (free)
        {
            TooltipManagerSingleton.instance.DisableTooltip(TooltipType.FreeLand);
        }
        TooltipManagerSingleton.instance.DisableTooltip((TooltipType)Enum.Parse(typeof(TooltipType), land.LandTypeEnum.ToString(), true));
    }

    public void Refresh()
    {
        if (land == null)
        {
            landImage.enabled = false;
        }
        else
        {
            landImage.enabled = true;
            landImage.sprite = spriteLandList.Single(x => x.landType == land.LandTypeEnum).landSprite;
        }

        selectedImage.enabled = land != null;
        selectedImage.sprite = land == null || fakeLandManager.currentSelectedLandId != land.token_id
            ? unselectedSprite
            : selectedSprite;
    }
}