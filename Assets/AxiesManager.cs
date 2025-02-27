using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AxieMixer.Unity;
using Game;
using SkyMavis.AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class AxiesManager : MonoBehaviour
{
    public GameObject UIPrefab;
    public Transform UIParent;
    public AxieSpawner axieSpawner;
    private int AmountSelected;
    private List<string> chosenAxies = new List<string>();
    public List<SkeletonGraphic> skeletonGraphics = new List<SkeletonGraphic>();
    public List<AxieController> axieControllers = new List<AxieController>();
    public Team myTeam;
    public GameObject CanvasMenu;
    public GameObject NewGameMenu;

    public void ShowMenuAxies(AxieTeam axieTeam)
    {
        if (axieTeam == null)
        {
            myTeam.characters.Clear();
            for (int i = 0; i < axieControllers.Count; i++)
            {
                Destroy(axieControllers[i].gameObject);
            }

            axieControllers.Clear();
            return;
        }

        StartCoroutine(IShowMenuAxies(axieTeam));
    }

    IEnumerator IShowMenuAxies(AxieTeam axieTeam)
    {
        LandManager.instance.ChooseLand(axieTeam.landTokenId);
        myTeam.characters.Clear();

        for (int i = 0; i < axieControllers.Count; i++)
        {
            Destroy(axieControllers[i].gameObject);
        }

        axieControllers.Clear();
        yield return new WaitForSeconds(0.1f);
        foreach (var axie in axieTeam.AxieIds)
        {
            AxieController axieController =
                axieSpawner.ProcessMixer(axie.id, axie.newGenes, false, axie.axieClass, axie.stats, false);

            axieController.ChangeMode(AxieMode.Menu);
            axieControllers.Add(axieController);
        }
    }

    public void SetAxiesBattleMode()
    {
        if (myTeam.GetCharactersAll().Count == 0)
        {
            NotificationErrorManager.instance.DoNotification("Please select a team");
            return;
        }

        CanvasMenu.SetActive(false);
        NewGameMenu.SetActive(true);
        MapManager.Instance.ToggleRectangles();
        foreach (var axieController in myTeam.GetCharactersAll())
        {
            axieController.ChangeMode(AxieMode.Battle);
        }
    }

    public void SetAxiesMenuMode()
    {
        MapManager.Instance.ToggleRectanglesFalse();
        foreach (var axieController in myTeam.GetCharactersAll())
        {
            axieController.ChangeMode(AxieMode.Menu);
        }
    }

    public void InitializeUIAxies()
    {
        foreach (var axie in AccountManager.userAxies.results)
        {
            GameObject UIItem = Instantiate(UIPrefab, UIParent);

            Axie2dBuilderResult builder = axieSpawner.SimpleProcessMixer(axie.id, axie.newGenes, true);
            SkeletonGraphic skeletonGraphic = UIItem.transform.GetChild(0).GetComponent<SkeletonGraphic>();
            skeletonGraphic.skeletonDataAsset = builder.skeletonDataAsset;
            axie.skeletonDataAsset = builder.skeletonDataAsset;
            axie.skeletonDataAssetMaterial = builder.sharedGraphicMaterial;
            skeletonGraphic.material = builder.sharedGraphicMaterial;
            skeletonGraphic.AnimationState.SetAnimation(0, "action/idle/normal", true);
            UIItem.GetComponent<Button>().onClick.AddListener(delegate { ChooseAxie(axie.id, builder); });
        }
    }

    private void ChooseAxie(string axieId, Axie2dBuilderResult builder)
    {
        if (chosenAxies.Any(x => x == axieId))
        {
            AmountSelected--;
            chosenAxies.Remove(axieId);
            SkeletonGraphic skeletonGraphic =
                skeletonGraphics.Single(x => x.skeletonDataAsset == builder.skeletonDataAsset);

            skeletonGraphic.skeletonDataAsset = null;
            skeletonGraphic.material = null;
            skeletonGraphic.transform.parent.GetComponent<Button>().onClick.RemoveAllListeners();
            skeletonGraphic.enabled = false;
        }
        else
        {
            if (AmountSelected >= 5)
            {
                return;
            }

            AmountSelected++;
            foreach (var skeletonGraphic in skeletonGraphics)
            {
                if (skeletonGraphic.skeletonDataAsset == null)
                {
                    skeletonGraphic.UpdateMode = UpdateMode.FullUpdate;
                    skeletonGraphic.enabled = true;
                    skeletonGraphic.skeletonDataAsset = builder.skeletonDataAsset;
                    skeletonGraphic.material = builder.sharedGraphicMaterial;
                    skeletonGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
                    skeletonGraphic.transform.parent.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ChooseAxie(axieId, builder);
                    });
                    skeletonGraphic.Initialize(true);
                    break;
                }
            }

            chosenAxies.Add(axieId);
        }
    }
}