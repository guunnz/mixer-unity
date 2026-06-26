using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIListMonsterForCaptain : MonoBehaviour
{
    public List<MonsterClassGraphic> monsterClassGraphics = new List<MonsterClassGraphic>();
    public GetMonstersExample.Monster monster;
    public Image selectedImage;
    public Image monsterClassImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public TeamCaptainManager teamCaptainManager;
    public VanillaMonsterGraphic monsterGraphic;
    private Button button;
    internal bool selected => selectedImage.sprite == selectedSprite;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectMonster);
    }

    public void SelectMonster()
    {
        VanillaMonsterGraphic graphic = EnsureGraphic();
        graphic.SetMonster(monster);
        graphic.Initialize(true);
        Refresh(false);
        teamCaptainManager.SetMonsterSelected(this.monster);
    }

    public void Refresh(bool resetMonster = true)
    {
        if (monster == null || monster.@class == "")
        {
            EnsureGraphic().enabled = false;
            selectedImage.enabled = monster != null;
            monsterClassImage.enabled = monster != null;
            return;
        }
        else if (resetMonster)
        {
            VanillaMonsterGraphic graphic = EnsureGraphic();
            graphic.enabled = true;
            graphic.SetMonster(monster);
            graphic.startingAnimation = "action/idle/normal";
            graphic.Initialize(true);
        }

        selectedImage.enabled = monster != null;
        monsterClassImage.enabled = monster != null;
        VanillaMonsterIconUtility.ApplyClass(monsterClassImage, monster.monsterClass, monsterClassGraphics);

        if (teamCaptainManager.selectedMonster != monster.id)
        {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            selectedImage.color = new Color(1, 1, 1, 0.3f);
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            selectedImage.color = new Color(1, 1, 1, 1f);
            selectedImage.sprite = selectedSprite;
        }
    }

    private VanillaMonsterGraphic EnsureGraphic()
    {
        monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(transform, monsterGraphic);
        return monsterGraphic;
    }
}
