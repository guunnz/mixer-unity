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
    [SerializeField] public LandType currentLandType = LandType.savannah;
    internal string currentSelectedLandId;
    [SerializeField] private List<SpawnableLand> spawnableLands;
    static public FakeLandManager Instance;

    [SerializeField] private bool InitializeOnStart;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (InitializeOnStart)
        {
            InstantiateLandType();
        }
    }

    public void InstantiateLandType()
    {
        GetAxiesExample.Land land = null;

        if (currentSpawnedLand == null)
        {
            currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
            landParent);

            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                string tokenIdSubstring = "11111111";
                int tokenIdInt = int.Parse(tokenIdSubstring);
                materialTipColorChanger.SetRandomSeed(
                    tokenIdInt / (i + 1));
                materialTipColorChanger.colorAlreadySet = false;
            }
        }
        else
        {
            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                string tokenIdSubstring = "111111111";
                int tokenIdInt = int.Parse(tokenIdSubstring);
                materialTipColorChanger.SetRandomSeed(tokenIdInt / (i + 1));
                materialTipColorChanger.colorAlreadySet = false;
                materialTipColorChanger.SetColor();
            }

            if (currentSpawnedLand != null)
            {
                Destroy(currentSpawnedLand);
            }

            currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
                landParent);
        }
    }

    public void ChooseFakeLand(string tokenId = null)
    {
        GetAxiesExample.Land land = null;

        if (currentSpawnedLand == null)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                land = AccountManager.userLands.results[indexChoosing];
                currentLandType = land.LandTypeEnum;
            }
            else
            {
                land = AccountManager.userLands.results.Single(x => x.token_id == tokenId);
                currentLandType = land.LandTypeEnum;
            }

            currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
            landParent);

            currentSelectedLandId = land.token_id;
            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                string tokenIdStr = AccountManager.userLands.results[indexChoosing].token_id;
                int tokenIdInt = LandSeedUtil.SeedFromTokenId(tokenIdStr);
                materialTipColorChanger.SetRandomSeed(
                    tokenIdInt / (i + 1));
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
                land = AccountManager.userLands.results.Single(x => x.token_id == tokenId);
                currentLandType = land.LandTypeEnum;
            }

            currentSelectedLandId = land.token_id;
            landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < landSquares.Length; i++)
            {
                MaterialTipColorChanger materialTipColorChanger = landSquares[i];
                materialTipColorChanger.landType = currentLandType;
                string tokenIdStr = AccountManager.userLands.results[indexChoosing].token_id;
                int tokenIdInt = LandSeedUtil.SeedFromTokenId(tokenIdStr);
                materialTipColorChanger.SetRandomSeed(tokenIdInt / (i + 1));
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