using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FakeLandManager : MonoBehaviour
{
    private GameObject currentSpawnedLand;
    public Transform landParent;
    private MaterialTipColorChanger[] landSquares;
    [SerializeField] private int indexChoosing = 0;
    internal LandType currentLandType = LandType.savannah;
    internal string currentSelectedLandId;
    [SerializeField] private List<SpawnableLand> spawnableLands;
    static public FakeLandManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    public void ChooseFakeLand(string tokenId = null)
    {
        GetAxiesExample.Land land = null;

        if (currentSpawnedLand == null)
        {
            currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
                landParent);

            if (string.IsNullOrEmpty(tokenId))
            {
                land = AccountManager.userLands.results[indexChoosing];
                currentLandType = land.LandTypeEnum;
            }
            else
            {
                land = AccountManager.userLands.results.Single(x => x.tokenId == tokenId);
                currentLandType = land.LandTypeEnum;
            }

            currentSelectedLandId = land.tokenId;
            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            RunManagerSingleton.instance.landType = currentLandType;
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                materialTipColorChanger.SetRandomSeed(
                    int.Parse(AccountManager.userLands.results[indexChoosing].tokenId) / (i + 1));
                materialTipColorChanger.colorAlreadySet = false;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                land = AccountManager.userLands.results[indexChoosing];
                currentLandType = land.LandTypeEnum;
            }
            else
            {
                land = AccountManager.userLands.results.Single(x => x.tokenId == tokenId);
                currentLandType = land.LandTypeEnum;
            }

            currentSelectedLandId = land.tokenId;
            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            RunManagerSingleton.instance.landType = currentLandType;
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                materialTipColorChanger.SetRandomSeed(
                    int.Parse(AccountManager.userLands.results[indexChoosing].tokenId) / (i + 1));
                materialTipColorChanger.colorAlreadySet = false;
            }

            if (currentSpawnedLand != null)
            {
                Destroy(currentSpawnedLand);
            }

            currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
                landParent);
        }
    }
}