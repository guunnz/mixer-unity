using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIListMonster : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<MonsterClassGraphic> monsterClassGraphics = new List<MonsterClassGraphic>();
    public GetMonstersExample.Monster monster;
    public Image selectedImage;
    public Image monsterClassImage;
    public GameObject freeRotation;
    public GameObject info;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public FakeMonstersManager fakeMonstersManager;
    public VanillaMonsterGraphic monsterGraphic;
    private Button button;
    public TeamBuilderManager teamBuilderManager;
    public MonstersView monstersView;
    internal bool selected => selectedImage.sprite == selectedSprite;
    private bool free;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectMonster);
    }

    public void DoInfo()
    {
        monstersView.SelectMonsterById(monster.id);
    }

    public void SelectMonster()
    {
        if (monster == null)
        {
            Refresh(false);
            return;
        }

        MonsterVisualDescriptor descriptor = fakeMonstersManager.ChooseMonster(monster);

        VanillaMonsterGraphic graphic = EnsureGraphic();
        if (descriptor != null)
        {
            graphic.SetDescriptor(descriptor);
            graphic.Initialize(true);
            Refresh(false);
        }
        else
        {
            Refresh(false);
            graphic.startingAnimation = "action/idle/normal";
            graphic.Initialize(true);
        }
    }

    public void Refresh(bool resetMonster = true)
    {
        if (monster == null)
        {
            freeRotation.SetActive(false);
            info.SetActive(false);
            VanillaMonsterGraphic graphic = EnsureGraphic();
            graphic.Clear();
            graphic.enabled = false;
        }
        else if (resetMonster)
        {
            VanillaMonsterGraphic graphic = EnsureGraphic();
            graphic.enabled = true;
            graphic.SetDescriptor(fakeMonstersManager.GetMonsterArt(monster));
            graphic.startingAnimation = "action/idle/normal";
            graphic.Initialize(true);
        }

        selectedImage.enabled = monster != null;
        monsterClassImage.enabled = monster != null;
        if (monster == null)
        {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            selectedImage.color = new Color(1, 1, 1, 0.3f);
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            VanillaMonsterIconUtility.ApplyClass(monsterClassImage, monster.monsterClass, monsterClassGraphics);

            if (monster.f2p)
            {
                freeRotation.SetActive(true);
                free = true;
            }
            else
            {
                freeRotation.SetActive(false);
                free = false;
            }

            if (fakeMonstersManager.instantiatedMonsters.Any(x => x.monster != null && x.monster.id == this.monster.id))
            {
                VanillaMonsterGraphic graphic = EnsureGraphic();
                if (graphic.startingAnimation != "action/idle/random-04")
                {
                    graphic.startingAnimation = "action/idle/random-04";
                    graphic.Initialize(true);
                }


                this.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f);

                selectedImage.color = new Color(1, 1, 1, 1f);
                selectedImage.sprite = selectedSprite;

                teamBuilderManager.SetMonsterSelected(monster, monster.monsterClass);
            }
            else
            {
                this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
                selectedImage.color = new Color(1, 1, 1, 0.3f);
                selectedImage.sprite = unselectedSprite;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (monster == null)
            return;

        if (free)
        {
            TooltipManagerSingleton.instance.EnableTooltip(TooltipType.FreeMonster);
        }

        teamBuilderManager.SetMonsterStats(this.monster);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (monster == null)
            return;

        if (free)
        {
            TooltipManagerSingleton.instance.DisableTooltip(TooltipType.FreeMonster);
        }

        teamBuilderManager.DisableMonsterStats();
    }

    private VanillaMonsterGraphic EnsureGraphic()
    {
        monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(transform, monsterGraphic);
        return monsterGraphic;
    }
}
