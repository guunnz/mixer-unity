using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using Spine.Unity;
using System.Linq;
using UnityEngine.UI;
using Game;

public class AxieStatsTooltip : MonoBehaviour
{
    private bool enabled;
    public RectTransform Container;
    private AxieController axieToCheck;
    public SkeletonGraphic axieGraphic;
    public TextMeshProUGUI HPText;
    public TextMeshProUGUI EnergyText;
    public TextMeshProUGUI Range;
    public Image axieHP;
    public Image axieEnergy;

    public void Enable(AxieController controller)
    {
        var axie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == controller.AxieId.ToString());

        if (axie != null)
        {
            axieGraphic.skeletonDataAsset = axie.skeletonDataAsset;
            axieGraphic.material = axie.skeletonDataAssetMaterial;
        }
        else
        {
            if (controller.skeletonDataAsset == null)
            {
                var axieGraphics = AxieSpawner.Instance.ProcessMixer(controller.Genes, controller.AxieId.ToString());
                controller.skeletonDataAsset = axieGraphics.Key;
                controller.skeletonMaterial = axieGraphics.Value;
            }
            axieGraphic.skeletonDataAsset = controller.skeletonDataAsset;
            axieGraphic.material = controller.skeletonMaterial;
        }

        axieGraphic.startingAnimation = "action/idle/normal";
        axieGraphic.startingLoop = true;
        axieGraphic.Initialize(true);
        Range.text = controller.Range.ToString();
        enabled = true;
        axieToCheck = controller;
        Container.DOAnchorPosX(-75, 0.5f);
        axieToCheck.statsManagerUI.Selected.SetActive(true);
    }

    private void Disable()
    {
        enabled = false;
        axieToCheck.statsManagerUI.Selected.SetActive(false);
        Container.anchoredPosition = new Vector2(80, Container.anchoredPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Disable();
                return;
            }
            var ingameStats = axieToCheck.axieIngameStats;
            HPText.text = $"{Math.Round(ingameStats.currentHP, 0)} / {ingameStats.HP}";
            EnergyText.text = $"{Math.Round(ingameStats.CurrentEnergy, 2)} / {ingameStats.totalComboCost}";
            axieEnergy.fillAmount = (ingameStats.CurrentEnergy) / ingameStats.MaxEnergy;
            axieHP.fillAmount = ingameStats.currentHP / ingameStats.HP;
            return;
        }
        if (Container.anchoredPosition.x == -75)
            Disable();
    }
}
