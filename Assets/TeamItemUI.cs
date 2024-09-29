using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamItemUI : MonoBehaviour
{
    public List<SpriteLand> spriteLandList = new List<SpriteLand>();
    public Image PlotGraphics;
    public TextMeshProUGUI PlotText;
    public TextMeshProUGUI TeamName;
    public Image SelectedImage;

    public Sprite SelectedSprite;
    public Sprite UnselectedSprite;
    public List<SkeletonGraphic> skeletonGraphics = new List<SkeletonGraphic>();
    private Button button;
    public AxieTeam currentTeam;
    public TeamSelectorUI teamSelectorUI;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(delegate { SelectTeam(true, currentTeam); });
    }

    public void SetTeamGraphics(AxieTeam axieTeam)
    {
        currentTeam = axieTeam;
        TeamName.text = axieTeam.TeamName;
        PlotGraphics.sprite = spriteLandList.Single(x => x.landType == axieTeam.landType).landSprite;
        GetAxiesExample.Land land = AccountManager.userLands.results.Single(x => x.tokenId == axieTeam.landTokenId);
        PlotText.text =
            $"{LandManager.CapitalizeFirstLetter(axieTeam.landType.ToString())} Plot ({land.row},{land.col})";
        for (int i = 0; i < axieTeam.AxieIds.Count; i++)
        {
            skeletonGraphics[i].skeletonDataAsset = axieTeam.AxieIds[i].skeletonDataAsset;
            skeletonGraphics[i].material = axieTeam.AxieIds[i].skeletonDataAssetMaterial;
            skeletonGraphics[i].startingAnimation = "action/idle/normal";
            skeletonGraphics[i].Initialize(true);
        }
    }

    public void SelectTeam(bool select, AxieTeam axieTeam)
    {
        if (select)
        {
            TeamManager.instance.currentTeam = axieTeam;
            PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected+ RunManagerSingleton.instance.user_wallet_address, axieTeam.TeamName);

            TeamName.text = axieTeam.TeamName;
            StartCoroutine(SelectTeamCoroutine(axieTeam));

            SelectedImage.sprite = SelectedSprite;
            teamSelectorUI.RefreshUI();
        }
        else
        {
            SelectedImage.sprite = UnselectedSprite;
        }
    }

    IEnumerator SelectTeamCoroutine(AxieTeam axieTeam)
    {
        if (FakeAxiesManager.instance.instantiatedAxies.Select(x => x.axie).ToList()
            .All(x => axieTeam.AxieIds.Contains(x)))
        {
            for (int i = 0; i < axieTeam.AxieIds.Count; i++)
            {
                var axie = axieTeam.AxieIds[i];
                FakeAxiesManager.instance.PositionCharacterOnTile(axie.id, axieTeam.position[i]);
            }
        }
        else
        {
            FakeAxiesManager.instance.ClearAllAxies();
            FakeLandManager.Instance.ChooseFakeLand(TeamManager.instance.currentTeam.landTokenId);
            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < axieTeam.AxieIds.Count; i++)
            {
                var axie = axieTeam.AxieIds[i];
                FakeAxiesManager.instance.ChooseAxie(axie);
                FakeAxiesManager.instance.PositionCharacterOnTile(axie.id, axieTeam.position[i]);
            }
        }
    }
}