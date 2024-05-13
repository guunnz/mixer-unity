using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Spine.Unity;
using UnityEngine;

public class FakeAxiesManager : MonoBehaviour
{
    public List<FakeAxieController> instantiatedAxies = new List<FakeAxieController>();
    public AxieSpawner axieSpawner;
    public FakeMapManager mapManager;
    public FakeLandManager fakeLandManager;

    IEnumerator Start()
    {
        mapManager.GenerateFakeMap();
        InitializeAxies();
        while (AccountManager.userLands == null)
        {
            yield return null;
        }

        fakeLandManager.ChooseFakeLand(AccountManager.userLands.results[0].tokenId);
    }

    private void InitializeAxies()
    {
        int index = 0;
        foreach (var fakeAxieController in instantiatedAxies)
        {
            Vector2Int? gridLocation = null;
            FakeOverlayTile startingTile = fakeAxieController.standingOnTile;

            while (startingTile == null || startingTile.occupied)
            {
                if (gridLocation == null)
                {
                    gridLocation = new Vector2Int(0, index);
                }

                startingTile = mapManager.map[gridLocation.Value];
                if (startingTile.occupied)
                {
                    gridLocation = null;
                    index++;
                }
            }

            PositionCharacterOnTile(fakeAxieController, startingTile);
        }
    }

    private void PositionCharacterOnTile(FakeAxieController character, FakeOverlayTile tile)
    {
        character.transform.position = tile.transform.position;
        tile.occupied = true;
        tile.currentOccupier = character;
        character.standingOnTile = tile;
    }

    public void ClearAllAxies()
    {
        foreach (var fakeAxieController in instantiatedAxies)
        {
            fakeAxieController.renderer.enabled = false;
        }
    }

    public void RemoveAxie(string AxieId)
    {
        var fakeAxieController = instantiatedAxies.Single(x => x.axie.id == AxieId);
        fakeAxieController.renderer.enabled = false;
        fakeAxieController.axie = null;
    }

    public SkeletonDataAsset GetAxieArt(GetAxiesExample.Axie axie)
    {
        SkeletonDataAsset skeletonDataAsset = axieSpawner.ProcessMixer(axie);
        return skeletonDataAsset;
    }

    public SkeletonDataAsset ChooseAxie(GetAxiesExample.Axie axie)
    {
        var axieControllerAlreadySelected =
            instantiatedAxies.FirstOrDefault(x => x.axie != null && x.axie.id == axie.id);
        if (axieControllerAlreadySelected != null)
        {
            RemoveAxie(axieControllerAlreadySelected.axie.id);
            return null;
        }

        if (instantiatedAxies.Any(x => x.renderer.enabled == false))
        {
            SkeletonDataAsset skeletonDataAsset = axieSpawner.ProcessMixer(axie);
            var fakeAxieController =
                instantiatedAxies.FirstOrDefault(x => x.renderer.enabled == false);

            fakeAxieController.skeletonAnim.skeletonDataAsset = skeletonDataAsset;
            fakeAxieController.axie = axie;
            fakeAxieController.skeletonAnim.AnimationName = "action/idle/normal";
            fakeAxieController.skeletonAnim.loop = true;
            fakeAxieController.renderer.enabled = true;
            fakeAxieController.skeletonAnim.Initialize(true);
            return skeletonDataAsset;
        }
        else
        {
            Debug.Log("Full Axies");
            return null;
        }
    }
}