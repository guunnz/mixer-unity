using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetMonstersExample;

public class SelectedComboData
{
    public bool passive;
    public string name;
    public string description;
    public string damage;
    public string shield;
    public string energy;
    public TooltipType[] tooltips;
}

public class FakeMonsterComboManager : MonoBehaviour
{
    public List<VanillaMonsterGraphic> TeamGraphics = new List<VanillaMonsterGraphic>();
    public List<MonsterPartGraphic> BodyPartGraphics = new List<MonsterPartGraphic>();
    public List<MonsterClassGraphic> MonsterClassGraphics = new List<MonsterClassGraphic>();
    public VanillaMonsterGraphic MonsterGraphicCombo;
    public TextMeshProUGUI AbilityNameText;
    public TextMeshProUGUI AbilityDescriptionText;
    public TextMeshProUGUI MonsterNameText;
    public TextMeshProUGUI ShieldAbilityText;
    public TextMeshProUGUI AttackAbilityText;
    public MonsterBodyPartsManager skillList;
    public Image HornBodyPart;
    public Image MouthBodyPart;
    public Image BackBodyPart;
    public Image TailBodyPart;
    public Image monsterClassImage;
    private SelectedComboData data = new SelectedComboData();
    public AbilityDescriptionTooltip AbilityDescriptionTooltip;
    public Button ButtonHornBodyPart;
    public Button ButtonMouthBodyPart;
    public Button ButtonBackBodyPart;
    public Button ButtonTailBodyPart;

    public GameObject HornBodyPartOrderImage;
    public GameObject MouthBodyPartOrderImage;
    public GameObject BackBodyPartOrderImage;
    public GameObject TailBodyPartOrderImage;
    public GameObject PassiveGO;
    public TextMeshProUGUI HornBodyPartOrderText;
    public TextMeshProUGUI MouthBodyPartOrderText;
    public TextMeshProUGUI BackBodyPartOrderText;
    public TextMeshProUGUI TailBodyPartOrderText;
    public TextMeshProUGUI EnergyText;
    public GameObject EnergyObject;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI MoraleText;
    public TextMeshProUGUI SkillText;
    public FakeMonstersManager monstersManager;
    private GetMonstersExample.Monster currentSelectedMonster;
    public Sprite SelectedSprite;
    public Sprite DeselectedSprite;

    public void SetUI()
    {
        PassiveGO.SetActive(data.passive);
        AttackAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        ShieldAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        EnergyObject.SetActive(!data.passive);
        EnergyText.text = data.energy;
        AbilityNameText.text = MonsterGeneUtils.SpaceCamelCase(data.name);
        AbilityDescriptionText.text = data.description;
        ShieldAbilityText.text = data.shield;
        AttackAbilityText.text = data.damage;


        AbilityDescriptionTooltip.SetTooltips(data.tooltips);
    }

    public void SetUI(SelectedComboData data)
    {
        PassiveGO.SetActive(data.passive);
        AttackAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        ShieldAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        EnergyObject.SetActive(!data.passive);
        EnergyText.text = data.energy;
        AbilityNameText.text = MonsterGeneUtils.SpaceCamelCase(data.name);
        AbilityDescriptionText.text = data.description;
        ShieldAbilityText.text = data.shield;
        AttackAbilityText.text = data.damage;

        AbilityDescriptionTooltip.SetTooltips(data.tooltips);
    }

    public void LoadUI()
    {
        for (int i = 0; i < monstersManager.instantiatedMonsters.Count; i++)
        {
            string localMonsterId = monstersManager.instantiatedMonsters[i].monster.id;
            VanillaMonsterGraphic localGraphic = EnsureTeamGraphic(i);

            localGraphic.SetMonster(monstersManager.instantiatedMonsters[i].monster);
            localGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            localGraphic.Initialize(true);

            var parent = localGraphic.transform.parent;
            parent.GetComponent<Button>().onClick.RemoveAllListeners();

            parent.GetComponent<Button>().onClick.AddListener(() => { SelectMonster(localMonsterId, parent); });
        }

        ButtonHornBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Horn); });
        ButtonMouthBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Mouth); });
        ButtonBackBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Back); });
        ButtonTailBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Tail); });

        for (int i = monstersManager.instantiatedMonsters.Count - 1; i >= 0; i--)
        {
            SelectMonster(monstersManager.instantiatedMonsters[i].monster.id, EnsureTeamGraphic(i).transform.parent);
        }
    }

    public void BodyPartHover(BodyPart part)
    {
        GetMonstersExample.Part bodyPartToSelect =
            currentSelectedMonster.parts.Single(x => x.BodyPart == part);
        MonsterBodyPart ability = skillList.monsterBodyParts
    .Single(x =>
        x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
        x.skillName == bodyPartToSelect.SkillName);

        SelectedComboData data = new SelectedComboData();
        data.name = bodyPartToSelect.name;
        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.tooltips = ability.tooltipTypes;
        SetUI(data);
    }

    public void BodyPartStopHover()
    {
        SetUI();
    }


    public void ChoosePart(BodyPart part)
    {
        GetMonstersExample.Part bodyPartToReplace =
            currentSelectedMonster.parts.FirstOrDefault(x => x.selected && x.order == 1);

        GetMonstersExample.Part bodyPartToSelect =
            currentSelectedMonster.parts.Single(x => x.BodyPart == part);

        // if (currentSelectedMonster.parts.Where(x => x.selected).Contains(bodyPartToSelect))
        // {
        //     bodyPartToReplace = null;
        // }

        if (bodyPartToSelect.order == currentSelectedMonster.maxBodyPartAmount)
        {
            return;
        }

        int amountSelected = currentSelectedMonster.parts.Count(x => x.selected);

        if (bodyPartToReplace != null && amountSelected >= currentSelectedMonster.maxBodyPartAmount)
        {
            bodyPartToReplace.order = 1;
            bodyPartToReplace.selected = false;
        }

        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);
        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        int passivesAdded = 0;

        foreach (var partObj in currentSelectedMonster.parts.Where(x => x.selected && x != bodyPartToSelect))
        {

            bool isPassive = skillList.monsterBodyParts
            .FirstOrDefault(x => x.skillName == partObj.SkillName).isPassive;

            if (partObj.order > 1)
            {
                partObj.order -= 1;
            }

            if (isPassive)
                passivesAdded++;

            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }
        }

        // Find the maximum order among selected parts
        int maxOrder = currentSelectedMonster.parts
            .Where(x => x.selected)
            .Select(x => (int?)x.order)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        // Set the selected part as the last order
        bodyPartToSelect.selected = true;
        bodyPartToSelect.order = maxOrder + 1;

        AbilityNameText.text = MonsterGeneUtils.SpaceCamelCase(bodyPartToSelect.name);
        MonsterBodyPart ability = skillList.monsterBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);
        AbilityDescriptionText.text = ability.description;

        switch (bodyPartToSelect.BodyPart)
        {
            case BodyPart.Back:
                BackBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                TailBodyPartOrderImage.SetActive(true);
                break;
        }

        if (ability.isPassive)
        {
            AttackAbilityText.transform.parent.gameObject.SetActive(false);
            ShieldAbilityText.transform.parent.gameObject.SetActive(false);
            EnergyObject.SetActive(false);
            PassiveGO.SetActive(true);
        }
        else
        {
            EnergyText.text = ability.energy.ToString();
            EnergyObject.SetActive(true);
            PassiveGO.SetActive(false);
            AttackAbilityText.transform.parent.gameObject.SetActive(true);
            ShieldAbilityText.transform.parent.gameObject.SetActive(true);
        }

        AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        ShieldAbilityText.text = ability.shield.ToString();
        AttackAbilityText.text = ability.damage.ToString();

        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.tooltips = ability.tooltipTypes;
        data.name = bodyPartToSelect.name;
    }

    public void ChoosePartOnlyDo()
    {
        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);

        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        int passivesAdded = 0;

        foreach (var partObj in currentSelectedMonster.parts.Where(x => x.selected).OrderBy(x => x.order))
        {
            // Check if the part is a passive
            bool isPassive = skillList.monsterBodyParts
                .FirstOrDefault(x => x.skillName == partObj.SkillName && x.bodyPart == partObj.BodyPart).isPassive;

            // Adjust the order based on passives logic
            string displayOrder = isPassive ? "P" : (partObj.order - passivesAdded + "°");

            // Activate and update the UI elements according to the body part type
            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }

            // Increment passives count if current part is passive
            if (isPassive) passivesAdded++;

            // Update ability details
            AbilityNameText.text = MonsterGeneUtils.SpaceCamelCase(partObj.name);
            MonsterBodyPart ability = skillList.monsterBodyParts.Single(x =>
                x.bodyPart == partObj.BodyPart && partObj.partClass == x.bodyPartClass &&
                x.skillName == partObj.SkillName);
            AbilityDescriptionText.text = ability.description;
            ShieldAbilityText.text = ability.shield.ToString();
            AttackAbilityText.text = ability.damage.ToString();

            // Update ability state based on passive
            if (ability.isPassive)
            {
                AttackAbilityText.transform.parent.gameObject.SetActive(false);
                ShieldAbilityText.transform.parent.gameObject.SetActive(false);
                EnergyObject.SetActive(false);
                PassiveGO.SetActive(true);
            }
            else
            {
                EnergyText.text = ability.energy.ToString();
                EnergyObject.SetActive(true);
                PassiveGO.SetActive(false);
                AttackAbilityText.transform.parent.gameObject.SetActive(true);
                ShieldAbilityText.transform.parent.gameObject.SetActive(true);
            }
            data.description = ability.description;
            data.energy = ability.energy.ToString();
            data.damage = ability.damage.ToString();
            data.shield = ability.shield.ToString();
            data.passive = ability.isPassive;
            data.tooltips = ability.tooltipTypes;
            data.name = partObj.name;
            AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        }
    }

    private void ApplyMonsterPartImage(Image image, GetMonstersExample.Monster monster, BodyPart bodyPart)
    {
        GetMonstersExample.Part part = monster.parts.Single(x => x.BodyPart == bodyPart);
        MonsterBodyPart ability = skillList.monsterBodyParts.FirstOrDefault(x =>
            x.bodyPart == bodyPart &&
            x.bodyPartClass == part.partClass &&
            x.skillName == part.SkillName);

        if (ability != null)
            VanillaMonsterIconUtility.ApplyBodyPart(image, ability);
        else
            VanillaMonsterIconUtility.ApplyBodyPart(image, bodyPart, part.partClass);
    }

    public void SelectMonster(string monsterId, Transform parent)
    {
        foreach (var skeletonGraphic in TeamGraphics.Where(x => x != null))
        {
            skeletonGraphic.transform.parent.GetComponent<Image>().sprite = DeselectedSprite;
        }
        SFXManager.instance.PlaySFX(SFXType.UIButtonTap, 0.12f, true);
        parent.GetComponent<Image>().sprite = SelectedSprite;
        GetMonstersExample.Monster monster = AccountManager.userMonsters.results.Single(x => x.id == monsterId);

        VanillaMonsterIconUtility.ApplyClass(monsterClassImage, monster.monsterClass, MonsterClassGraphics);
        HealthText.text = monster.stats.hp.ToString();
        SpeedText.text = monster.stats.speed.ToString();
        SkillText.text = monster.stats.skill.ToString();
        MoraleText.text = monster.stats.morale.ToString();
        MonsterNameText.text = monster.name;

        currentSelectedMonster = monster;
        ApplyMonsterPartImage(HornBodyPart, monster, BodyPart.Horn);
        ApplyMonsterPartImage(MouthBodyPart, monster, BodyPart.Mouth);
        ApplyMonsterPartImage(BackBodyPart, monster, BodyPart.Back);
        ApplyMonsterPartImage(TailBodyPart, monster, BodyPart.Tail);

        currentSelectedMonster = monster;
        VanillaMonsterGraphic comboGraphic = EnsureComboGraphic();
        comboGraphic.SetMonster(monster);
        comboGraphic.startingAnimation = "action/idle/normal";
        comboGraphic.Initialize(true);

        if (monster.parts.Any(x => x.selected))
        {
            ChoosePartOnlyDo();
        }
        else
        {
            ChoosePart(BodyPart.Horn);
            ChoosePart(BodyPart.Mouth);
        }
    }

    private VanillaMonsterGraphic EnsureTeamGraphic(int index)
    {
        while (TeamGraphics.Count <= index)
            TeamGraphics.Add(null);

        if (TeamGraphics[index] != null)
            return TeamGraphics[index];

        GameObject slot = new GameObject("Fake Team Monster Slot " + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.SetParent(transform, false);
        slotRect.sizeDelta = new Vector2(170f, 130f);

        GameObject graphicGo = new GameObject("Vanilla Monster Graphic", typeof(RectTransform));
        RectTransform graphicRect = graphicGo.GetComponent<RectTransform>();
        graphicRect.SetParent(slotRect, false);
        graphicRect.anchorMin = Vector2.zero;
        graphicRect.anchorMax = Vector2.one;
        graphicRect.offsetMin = Vector2.zero;
        graphicRect.offsetMax = Vector2.zero;

        TeamGraphics[index] = VanillaMonsterGraphic.Ensure(graphicGo);
        return TeamGraphics[index];
    }

    private VanillaMonsterGraphic EnsureComboGraphic()
    {
        if (MonsterGraphicCombo != null)
            return MonsterGraphicCombo;

        GameObject graphicGo = new GameObject("Vanilla Fake Combo Monster Graphic", typeof(RectTransform));
        RectTransform graphicRect = graphicGo.GetComponent<RectTransform>();
        graphicRect.SetParent(transform, false);
        graphicRect.sizeDelta = new Vector2(220f, 160f);
        MonsterGraphicCombo = VanillaMonsterGraphic.Ensure(graphicGo);
        return MonsterGraphicCombo;
    }
}
