using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class MonstersManager : MonoBehaviour
{
    public GameObject UIPrefab;
    public Transform UIParent;
    public MonsterSpawner monsterSpawner;
    private int AmountSelected;
    private List<string> chosenMonsters = new List<string>();
    public List<VanillaMonsterGraphic> monsterGraphics = new List<VanillaMonsterGraphic>();
    public List<MonsterController> monsterControllers = new List<MonsterController>();
    public Team myTeam;
    public GameObject CanvasMenu;
    public GameObject NewGameMenu;

    public void ShowMenuMonsters(MonsterTeam monsterTeam)
    {
        if (monsterTeam == null)
        {
            myTeam.characters.Clear();
            for (int i = 0; i < monsterControllers.Count; i++)
            {
                Destroy(monsterControllers[i].gameObject);
            }

            monsterControllers.Clear();
            return;
        }

        StartCoroutine(IShowMenuMonsters(monsterTeam));
    }

    IEnumerator IShowMenuMonsters(MonsterTeam monsterTeam)
    {
        LandManager.instance.ChooseLand(monsterTeam.landTokenId);
        myTeam.characters.Clear();

        for (int i = 0; i < monsterControllers.Count; i++)
        {
            Destroy(monsterControllers[i].gameObject);
        }

        monsterControllers.Clear();
        yield return new WaitForSeconds(0.1f);
        foreach (var monster in monsterTeam.MonsterIds)
        {
            MonsterController monsterController =
                monsterSpawner.ProcessMixer(monster.id, monster.newGenes, false, monster.monsterClass, monster.stats, false);

            monsterController.ChangeMode(MonsterMode.Menu);
            monsterControllers.Add(monsterController);
        }
    }

    public void SetMonstersBattleMode()
    {
        if (myTeam.GetCharactersAll().Count == 0)
        {
            NotificationErrorManager.instance.DoNotification("Please select a team");
            return;
        }

        CanvasMenu.SetActive(false);
        NewGameMenu.SetActive(true);
        MapManager.Instance.ToggleRectangles();
        foreach (var monsterController in myTeam.GetCharactersAll())
        {
            monsterController.ChangeMode(MonsterMode.Battle);
        }
    }

    public void SetMonstersMenuMode()
    {
        MapManager.Instance.ToggleRectanglesFalse();
        foreach (var monsterController in myTeam.GetCharactersAll())
        {
            monsterController.ChangeMode(MonsterMode.Menu);
        }
    }

    public void InitializeUIMonsters()
    {
        foreach (var monster in AccountManager.userMonsters.results)
        {
            GameObject UIItem = Instantiate(UIPrefab, UIParent);

            MonsterVisualDescriptor descriptor = monsterSpawner.SimpleProcessMixer(monster.id, monster.newGenes, false);
            monster.visualDescriptor = descriptor;
            VanillaMonsterGraphic monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(UIItem.transform);
            monsterGraphic.SetDescriptor(descriptor);
            monsterGraphic.Initialize(true);
            UIItem.GetComponent<Button>().onClick.AddListener(delegate { ChooseMonster(monster.id, descriptor); });
        }
    }

    private void ChooseMonster(string monsterId, MonsterVisualDescriptor descriptor)
    {
        if (chosenMonsters.Any(x => x == monsterId))
        {
            AmountSelected--;
            chosenMonsters.Remove(monsterId);
            VanillaMonsterGraphic monsterGraphic =
                monsterGraphics.Single(x => x.Descriptor != null && x.Descriptor.MonsterId == descriptor.MonsterId);

            monsterGraphic.Clear();
            monsterGraphic.transform.parent.GetComponent<Button>().onClick.RemoveAllListeners();
            monsterGraphic.enabled = false;
        }
        else
        {
            if (AmountSelected >= 5)
            {
                return;
            }

            AmountSelected++;
            foreach (var monsterGraphic in monsterGraphics)
            {
                if (monsterGraphic.Descriptor == null)
                {
                    monsterGraphic.enabled = true;
                    monsterGraphic.SetDescriptor(descriptor);
                    monsterGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
                    monsterGraphic.transform.parent.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ChooseMonster(monsterId, descriptor);
                    });
                    monsterGraphic.Initialize(true);
                    break;
                }
            }

            chosenMonsters.Add(monsterId);
        }
    }
}
