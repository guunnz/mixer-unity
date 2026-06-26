using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamItemUI : MonoBehaviour
{
    private const string MonsterSlotsRootName = "Monsters";
    private const string MonsterSlotBaseName = "Monster";

    private static readonly Vector2 FirstMonsterSlotPosition = new Vector2(-34.5f, -44.3f);
    private static readonly Vector2 MonsterSlotSpacing = new Vector2(100f, 0f);
    private static readonly Vector2 MonsterSlotSize = new Vector2(90f, 75f);
    private static readonly Vector2 MonsterSlotPivot = new Vector2(0.47659394f, 0.114409454f);

    public List<SpriteLand> spriteLandList = new List<SpriteLand>();
    public Image PlotGraphics;
    public TextMeshProUGUI PlotText;
    public TextMeshProUGUI TeamName;
    public Image SelectedImage;

    public Sprite SelectedSprite;
    public Sprite UnselectedSprite;
    public List<VanillaMonsterGraphic> monsterGraphics = new List<VanillaMonsterGraphic>();
    private Button button;
    public MonsterTeam currentTeam;
    public TeamSelectorUI teamSelectorUI;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(delegate { SelectTeam(true, currentTeam); });
    }

    public void SetTeamGraphics(MonsterTeam monsterTeam)
    {
        currentTeam = monsterTeam;
        TeamName.text = monsterTeam.TeamName;
        PlotGraphics.sprite = spriteLandList.Single(x => x.landType == monsterTeam.landType).landSprite;
        GetMonstersExample.Land land = AccountManager.userLands.results.Single(x => x.token_id == monsterTeam.landTokenId);
        PlotText.text =
            $"{LandManager.CapitalizeFirstLetter(monsterTeam.landType.ToString())} Plot ({land.row},{land.col})";
        for (int i = 0; i < monsterTeam.MonsterIds.Count; i++)
        {
            VanillaMonsterGraphic graphic = EnsureGraphic(i);
            graphic.SetMonster(monsterTeam.MonsterIds[i]);
            graphic.startingAnimation = "action/idle/normal";
            graphic.Initialize(true);
        }

        ClearUnusedGraphics(monsterTeam.MonsterIds.Count);
    }

    public void SelectTeam(bool select, MonsterTeam monsterTeam)
    {
        if (select)
        {
            TeamManager.instance.currentTeam = monsterTeam;
            PlayerPrefs.SetString(PlayerPrefsValues.MonsterTeamSelected + RunManagerSingleton.instance.user_wallet_address, monsterTeam.TeamName);

            TeamName.text = monsterTeam.TeamName;
            StartCoroutine(SelectTeamCoroutine(monsterTeam));

            SelectedImage.sprite = SelectedSprite;
            teamSelectorUI.RefreshUI();
        }
        else
        {
            SelectedImage.sprite = UnselectedSprite;
        }
    }

    IEnumerator SelectTeamCoroutine(MonsterTeam monsterTeam)
    {
        if (FakeMonstersManager.instance.instantiatedMonsters.Select(x => x.monster).ToList()
            .All(x => monsterTeam.MonsterIds.Contains(x)))
        {
            for (int i = 0; i < monsterTeam.MonsterIds.Count; i++)
            {
                var monster = monsterTeam.MonsterIds[i];
                FakeMonstersManager.instance.PositionCharacterOnTile(monster.id, monsterTeam.position[i]);
            }
        }
        else
        {
            FakeMonstersManager.instance.ClearAllMonsters();
            FakeLandManager.Instance.ChooseFakeLand(TeamManager.instance.currentTeam.landTokenId);
            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < monsterTeam.MonsterIds.Count; i++)
            {
                var monster = monsterTeam.MonsterIds[i];
                FakeMonstersManager.instance.ChooseMonster(monster);
                FakeMonstersManager.instance.PositionCharacterOnTile(monster.id, monsterTeam.position[i]);
            }
        }
    }

    private VanillaMonsterGraphic EnsureGraphic(int index)
    {
        while (monsterGraphics.Count <= index)
            monsterGraphics.Add(null);

        Transform slot = EnsureMonsterSlot(index);
        monsterGraphics[index] = VanillaMonsterGraphic.EnsureCenteredChild(slot, monsterGraphics[index]);

        return monsterGraphics[index];
    }

    private Transform EnsureMonsterSlot(int index)
    {
        Transform slotsRoot = EnsureMonsterSlotsRoot();
        string slotName = GetMonsterSlotName(index);
        Transform slot = slotsRoot.Find(slotName);

        if (slot == null)
        {
            GameObject slotGo = new GameObject(slotName, typeof(RectTransform), typeof(CanvasRenderer));
            slot = slotGo.transform;
            slot.SetParent(slotsRoot, false);
        }

        PrepareMonsterSlot((RectTransform)slot, index);
        return slot;
    }

    private Transform EnsureMonsterSlotsRoot()
    {
        Transform slotsRoot = transform.Find(MonsterSlotsRootName);
        if (slotsRoot != null)
            return slotsRoot;

        GameObject rootGo = new GameObject(MonsterSlotsRootName, typeof(RectTransform));
        RectTransform rootRect = rootGo.GetComponent<RectTransform>();
        rootRect.SetParent(transform, false);
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(100f, 100f);
        return rootRect;
    }

    private void PrepareMonsterSlot(RectTransform slot, int index)
    {
        slot.localScale = Vector3.one;
        slot.localRotation = Quaternion.identity;
        slot.anchorMin = new Vector2(0.5f, 0.5f);
        slot.anchorMax = new Vector2(0.5f, 0.5f);
        slot.pivot = MonsterSlotPivot;
        slot.anchoredPosition = FirstMonsterSlotPosition + MonsterSlotSpacing * index;
        slot.sizeDelta = MonsterSlotSize;
    }

    private string GetMonsterSlotName(int index)
    {
        return index == 0 ? MonsterSlotBaseName : $"{MonsterSlotBaseName} ({index})";
    }

    private void ClearUnusedGraphics(int usedCount)
    {
        for (int i = usedCount; i < monsterGraphics.Count; i++)
        {
            if (monsterGraphics[i] != null)
                monsterGraphics[i].Clear();
        }
    }
}
