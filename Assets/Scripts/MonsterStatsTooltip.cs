using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;
using Game;

public class MonsterStatsTooltip : MonoBehaviour
{
    private const string TooltipGraphicName = "Tooltip Monster Graphic";

    private bool tooltipEnabled;
    public RectTransform Container;
    private MonsterController monsterToCheck;
    public VanillaMonsterGraphic monsterGraphic;
    public TextMeshProUGUI HPText;
    public TextMeshProUGUI EnergyText;
    public TextMeshProUGUI Range;
    public Image monsterHP;
    public Image monsterEnergy;

    public void Enable(MonsterController controller)
    {
        if (controller == null)
            return;

        EnsureGraphic();

        GetMonstersExample.Monster monster = AccountManager.userMonsters?.results?
            .FirstOrDefault(x => x.id == controller.MonsterId.ToString());

        if (monsterGraphic != null && monster != null)
        {
            monsterGraphic.SetMonster(monster);
        }
        else if (monsterGraphic != null)
        {
            if (controller.visualDescriptor == null && MonsterSpawner.Instance != null)
                controller.visualDescriptor = MonsterSpawner.Instance.ProcessMixer(controller.Genes, controller.MonsterId.ToString());

            controller.visualDescriptor ??= MonsterVisualDescriptor.Default();
            monsterGraphic.SetDescriptor(controller.visualDescriptor);
        }

        if (monsterGraphic != null)
        {
            monsterGraphic.startingAnimation = "action/idle/normal";
            monsterGraphic.Initialize(true);
        }

        if (Range != null)
            Range.text = controller.Range.ToString();

        tooltipEnabled = true;
        monsterToCheck = controller;
        Container?.DOAnchorPosX(-75, 0.5f);
        monsterToCheck.statsManagerUI?.Selected?.DOColor(new Color(1, 1, 1, 0.7215686f), 0.5f);
    }

    private void Disable()
    {
        tooltipEnabled = false;
        monsterToCheck?.statsManagerUI?.Selected?.DOColor(new Color(1, 1, 1, 0), 0.5f);

        if (Container != null)
            Container.anchoredPosition = new Vector2(80, Container.anchoredPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (tooltipEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Disable();
                return;
            }
            if (monsterToCheck == null)
            {
                Disable();
                return;
            }

            var ingameStats = monsterToCheck.monsterIngameStats;
            if (ingameStats == null)
                return;

            if (HPText != null)
                HPText.text = $"{Math.Round(ingameStats.currentHP, 0)} / {ingameStats.maxHP}";
            if (EnergyText != null)
                EnergyText.text = $"{Math.Round(ingameStats.CurrentEnergy, 2)} / {ingameStats.totalComboCost}";
            if (monsterEnergy != null && ingameStats.MaxEnergy > 0f)
                monsterEnergy.fillAmount = ingameStats.CurrentEnergy / ingameStats.MaxEnergy;
            if (monsterHP != null && ingameStats.maxHP > 0f)
                monsterHP.fillAmount = ingameStats.currentHP / ingameStats.maxHP;
            return;
        }
        if (Container != null && Container.anchoredPosition.x == -75)
            Disable();
    }

    private void EnsureGraphic()
    {
        if (monsterGraphic != null)
        {
            monsterGraphic.CenterInParent();
            return;
        }

        Transform parent = Container != null ? Container : transform;
        monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(parent, monsterGraphic, TooltipGraphicName);
    }
}
