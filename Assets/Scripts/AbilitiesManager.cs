using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static GetMonstersExample;

[System.Serializable]
public class MonsterPartGraphic
{
    public BodyPart bodyPart;
    public MonsterClass monsterClass;
    public Sprite bodyPartSprite;
}

[System.Serializable]
public class MonsterClassGraphic
{
    public MonsterClass monsterClass;
    public Sprite monsterClassSprite;
}

public class AbilitiesManager : MonoBehaviour
{
    private const string LegacyTeamGraphicName = "SkeletonTeam";

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
    public AbilityDescriptionTooltip AbilityDescriptionTooltip;
    public Image HornBodyPart;
    public Image MouthBodyPart;
    public Image BackBodyPart;
    public Image TailBodyPart;
    public Image monsterClassImage;
    public GameObject PassiveGO;

    public Button ButtonHornBodyPart;
    public Button ButtonMouthBodyPart;
    public Button ButtonBackBodyPart;
    public Button ButtonTailBodyPart;
    public GameObject Tutorial;

    public GameObject HornBodyPartOrderImage;
    public GameObject MouthBodyPartOrderImage;
    public GameObject BackBodyPartOrderImage;
    public GameObject TailBodyPartOrderImage;
    public TextMeshProUGUI HornBodyPartOrderText;
    public TextMeshProUGUI MouthBodyPartOrderText;
    public TextMeshProUGUI BackBodyPartOrderText;
    public TextMeshProUGUI TailBodyPartOrderText;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI MoraleText;
    public TextMeshProUGUI SkillText;

    public TextMeshProUGUI EnergyText;
    public GameObject EnergyObject;

    public MonstersManager monstersManager;
    private GetMonstersExample.Monster currentSelectedMonster;
    private SelectedComboData data = new SelectedComboData();
    public Sprite SelectedSprite;
    public Sprite DeselectedSprite;
    static public AbilitiesManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void LoadUI()
    {
        for (int i = 0; i < TeamManager.instance.currentTeam.MonsterIds.Count; i++)
        {
            string localMonsterId = TeamManager.instance.currentTeam.MonsterIds[i].id;
            VanillaMonsterGraphic localGraphic = EnsureTeamGraphic(i);

            localGraphic.SetMonster(TeamManager.instance.currentTeam.MonsterIds[i]);
            localGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            localGraphic.Initialize(true);

            var parent = localGraphic.transform.parent;
            parent.GetComponent<Button>().onClick.RemoveAllListeners();
            parent.GetComponent<Button>().onClick.AddListener(() => { SelectMonster(localMonsterId, parent); });
            SelectMonster(TeamManager.instance.currentTeam.MonsterIds[i].id, localGraphic.transform.parent);

        }
        SelectMonster(TeamManager.instance.currentTeam.MonsterIds[0].id, EnsureTeamGraphic(0).transform.parent);

        ButtonHornBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Horn); });
        ButtonMouthBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Mouth); });
        ButtonBackBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Back); });
        ButtonTailBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Tail); });

        if (PlayerPrefs.GetInt("Tutorial") == 0)
        {
            Tutorial.SetActive(true);
            PlayerPrefs.SetInt("Tutorial", 1);
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

        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.name = bodyPartToSelect.name;
        data.tooltips = ability.tooltipTypes;
        SetUI(data);
    }
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
    public void BodyPartStopHover()
    {
        SetUI();
    }
    public void ChoosePart(BodyPart part)
    {
        GetMonstersExample.Part bodyPartToReplace =
            currentSelectedMonster.parts.Where(y => y.selected).OrderBy(x => x.order).FirstOrDefault();

        GetMonstersExample.Part bodyPartToSelect =
            currentSelectedMonster.parts.Single(x => x.BodyPart == part);

        if (bodyPartToSelect.selected)
        {
            bodyPartToReplace = null;
        }

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

        int passivesAdded = 0;

        foreach (var partObj in currentSelectedMonster.parts.OrderBy(x => x.order).Where(x => x.selected && x != bodyPartToSelect))
        {
            bool isPassive = skillList.monsterBodyParts
            .FirstOrDefault(x => x.skillName == partObj.SkillName).isPassive;

            if (partObj.order != 1 && amountSelected >= currentSelectedMonster.maxBodyPartAmount)
            {
                partObj.order -= 1;
            }

            if (isPassive)
                passivesAdded++;

            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = isPassive ? "P" : (partObj.order - passivesAdded + "°");
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = isPassive ? "P" : (partObj.order - passivesAdded + "°");
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = (isPassive ? "P" : partObj.order - passivesAdded + "°");
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = isPassive ? "P" : (partObj.order - passivesAdded + "°");
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

        AbilityNameText.text = MonsterGeneUtils.SpaceCamelCase(bodyPartToSelect.name);
        MonsterBodyPart ability = skillList.monsterBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);

        AbilityDescriptionText.text = ability.description;
        // Set the selected part as the last orde
        bodyPartToSelect.selected = true;
        bodyPartToSelect.order = maxOrder + 1;

        bool isPassiveSelect = skillList.monsterBodyParts
       .FirstOrDefault(x => x.skillName == bodyPartToSelect.SkillName).isPassive;

        switch (bodyPartToSelect.BodyPart)
        {
            case BodyPart.Back:
                BackBodyPartOrderText.text = isPassiveSelect ? "P" : (bodyPartToSelect.order - passivesAdded + "°");
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = isPassiveSelect ? "P" : (bodyPartToSelect.order - passivesAdded + "°");
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = isPassiveSelect ? "P" : (bodyPartToSelect.order - passivesAdded + "°");
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = isPassiveSelect ? "P" : (bodyPartToSelect.order - passivesAdded + "°");
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

        ShieldAbilityText.text = ability.shield.ToString();
        AttackAbilityText.text = ability.damage.ToString();

        var MonsterSelecteds = currentSelectedMonster.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();

        monstersManager.monsterControllers.Single(x => x.MonsterId.ToString() == currentSelectedMonster.id).monsterSkillController.SetMonsterSkills(MonsterSelecteds.Select(x => x.SkillName).ToList(),
                MonsterSelecteds.Select(x => x.BodyPart).ToList());

        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Mouth))
        {
            ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Back))
        {
            ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Tail))
        {
            ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Horn))
        {
            ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.name = bodyPartToSelect.name;
        data.tooltips = ability.tooltipTypes;
    }

    public void ChoosePartOnlyDo()
    {
        // Deactivate all body part order images initially
        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);

        int passivesAdded = 0;



        // Iterate over selected parts
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
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
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

        // Sort and update the button sprites for the body parts
        var MonsterSelecteds = currentSelectedMonster.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();

        monstersManager.monsterControllers.Single(x => x.MonsterId.ToString() == currentSelectedMonster.id).monsterSkillController.SetMonsterSkills(MonsterSelecteds.Select(x => x.SkillName).ToList(),
                MonsterSelecteds.Select(x => x.BodyPart).ToList());
        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Mouth))
            ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Back))
            ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Tail))
            ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (MonsterSelecteds.Any(x => x.BodyPart == BodyPart.Horn))
            ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;

        if (currentSelectedMonster.maxBodyPartAmount > currentSelectedMonster.parts.Count(x => x.selected))
        {
            ChoosePart(currentSelectedMonster.parts.FirstOrDefault(x => !x.selected).BodyPart);
        }
    }

    public Sprite GetSkillSprite(MonsterBodyPart skill)
    {
        return VanillaMonsterIconUtility.GetAbilitySprite(skill);
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

        parent.GetComponent<Image>().sprite = SelectedSprite;

        SFXManager.instance.PlaySFX(SFXType.UIButtonTap, 0.12f, true);

        GetMonstersExample.Monster monster = AccountManager.userMonsters.results.Single(x => x.id == monsterId);

        if (RunManagerSingleton.instance.goodTeam.GetCharactersAll().Count > 0)
        {
            MonsterController monsterFromTeam = RunManagerSingleton.instance.goodTeam.GetCharactersAll().Single(x => x.MonsterId.ToString() == monsterId);
            HealthText.text = monsterFromTeam.stats.hp.ToString();
            SpeedText.text = monsterFromTeam.stats.speed.ToString();
            SkillText.text = monsterFromTeam.stats.skill.ToString();
            MoraleText.text = monsterFromTeam.stats.morale.ToString();
        }
        else
        {
            HealthText.text = monster.stats.hp.ToString();
            SpeedText.text = monster.stats.speed.ToString();
            SkillText.text = monster.stats.skill.ToString();
            MoraleText.text = monster.stats.morale.ToString();
        }
        VanillaMonsterIconUtility.ApplyClass(monsterClassImage, monster.monsterClass, MonsterClassGraphics);

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
            monster.parts = TeamManager.instance.currentTeam.MonsterIds.Single(x => x.id == monsterId).parts;

            if (monster.parts.Count(x => x.selected) == 0)
            {
                ChoosePart(BodyPart.Horn);
                ChoosePart(BodyPart.Mouth);
            }
            else
            {
                ChoosePartOnlyDo();
            }
        }
    }

    private VanillaMonsterGraphic EnsureTeamGraphic(int index)
    {
        while (TeamGraphics.Count <= index)
            TeamGraphics.Add(null);

        if (TeamGraphics[index] != null)
        {
            PrepareTeamGraphic(TeamGraphics[index]);
            return TeamGraphics[index];
        }

        TeamGraphics[index] = FindLegacyTeamGraphic(index);
        if (TeamGraphics[index] != null)
        {
            PrepareTeamGraphic(TeamGraphics[index]);
            return TeamGraphics[index];
        }

        GameObject slot = new GameObject("Team Monster Slot " + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button));
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
        PrepareTeamGraphic(TeamGraphics[index]);
        return TeamGraphics[index];
    }

    private VanillaMonsterGraphic FindLegacyTeamGraphic(int index)
    {
        GameObject legacyObject = GameObject.Find(LegacyTeamGraphicName + (index + 1));
        if (legacyObject == null)
            return null;

        return VanillaMonsterGraphic.Ensure(legacyObject);
    }

    private void PrepareTeamGraphic(VanillaMonsterGraphic graphic)
    {
        if (graphic == null)
            return;

        RectTransform rect = graphic.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.SetAsLastSibling();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            if (rect.parent is RectTransform parentRect)
            {
                Vector2 parentSize = parentRect.rect.size;
                if (parentSize.x <= 1f || parentSize.y <= 1f)
                    parentSize = parentRect.sizeDelta;
                if (parentSize.x <= 1f || parentSize.y <= 1f)
                    parentSize = new Vector2(90f, 90f);

                rect.sizeDelta = parentSize;
            }
        }

        graphic.CenterInParent();
    }

    private VanillaMonsterGraphic EnsureComboGraphic()
    {
        if (MonsterGraphicCombo != null)
            return MonsterGraphicCombo;

        GameObject graphicGo = new GameObject("Vanilla Combo Monster Graphic", typeof(RectTransform));
        RectTransform graphicRect = graphicGo.GetComponent<RectTransform>();
        graphicRect.SetParent(transform, false);
        graphicRect.sizeDelta = new Vector2(220f, 160f);
        MonsterGraphicCombo = VanillaMonsterGraphic.Ensure(graphicGo);
        return MonsterGraphicCombo;
    }
}
