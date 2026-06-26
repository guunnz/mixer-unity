using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIListMonsterForView : MonoBehaviour
{
    public List<MonsterClassGraphic> monsterClassGraphics = new List<MonsterClassGraphic>();
    public GetMonstersExample.Monster monster;
    public Image selectedImage;
    public Image monsterClassImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public MonstersView monstersView;
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
        monstersView.SetMonsterSelected(this.monster, monster.monsterClass);
    }

    public void Refresh(bool resetMonster = true)
    {
        if (monster == null)
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
        if (monstersView.selectedMonster != monster.id)
        {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            selectedImage.color = new Color(1, 1, 1, 0.3f);
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            monstersView.SetMonsterStats(this.monster);
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
