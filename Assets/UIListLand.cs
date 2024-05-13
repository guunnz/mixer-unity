using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpriteLand
{
    public LandType landType;
    public Sprite landSprite;
}

public class UIListLand : MonoBehaviour
{
    public List<SpriteLand> spriteLandList = new List<SpriteLand>();
    public GetAxiesExample.Land land;
    public TeamBuilderManager teamBuilderManager;
    public Image landImage;
    public Image selectedImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public FakeLandManager fakeLandManager;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectLand);
    }

    private void SelectLand()
    {
        fakeLandManager.ChooseFakeLand(land.tokenId);
        teamBuilderManager.SetLandUI();
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
        selectedImage.sprite = land == null || fakeLandManager.currentSelectedLandId != land.tokenId
            ? unselectedSprite
            : selectedSprite;
    }
}