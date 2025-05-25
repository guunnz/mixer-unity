using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpawnableLand
{
    public LandType landType;
    public GameObject landPrefab;
}

public class LandManager : MonoBehaviour
{
    [SerializeField] private int indexChoosing = 0;
    private MaterialTipColorChanger[] landSquares;
    public GameObject landParent;
    public MapManager mapManager;
    [SerializeField] private List<SpawnableLand> spawnableLands;
    private GameObject currentSpawnedLand;
    private LandType currentLandType;

    static public LandManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void InitializeLand()
    {
        mapManager.GenerateMap();
        landSquares = FindObjectsByType<MaterialTipColorChanger>(FindObjectsSortMode.InstanceID);
        ChooseLand();
    }

    public void ChooseLeft()
    {
        indexChoosing--;
        if (indexChoosing < 0)
        {
            indexChoosing = AccountManager.userLands.results.Length - 1;
        }

        if (indexChoosing >= AccountManager.userLands.results.Length)
        {
            indexChoosing = 0;
        }

        ChooseLand();
    }

    public void ChooseRight()
    {
        indexChoosing++;
        if (indexChoosing < 0)
        {
            indexChoosing = AccountManager.userLands.results.Length - 1;
        }

        if (indexChoosing >= AccountManager.userLands.results.Length)
        {
            indexChoosing = 0;
        }

        ChooseLand();
    }

    public void ChooseLand(string tokenId = null)
    {
        if (string.IsNullOrEmpty(tokenId))
        {
            currentLandType = AccountManager.userLands.results[indexChoosing].LandTypeEnum;
        }
        else
        {
            var land = AccountManager.userLands.results.FirstOrDefault(x => x.token_id == tokenId);
            if (land == null)
            {
                currentLandType = AccountManager.userLands.results[indexChoosing].LandTypeEnum;
            }
            else
            {
                currentLandType = land.LandTypeEnum;
            }
        }

        RunManagerSingleton.instance.landType = currentLandType;
        for (int i = 0; i < landSquares.Length; i++)
        {
            MaterialTipColorChanger materialTipColorChanger = landSquares[i];
            materialTipColorChanger.landType = currentLandType;
            string tokenIdSubstring =
                AccountManager.userLands.results[indexChoosing].token_id.ToString().Substring(0, 10);
            int tokenIdInt = int.Parse(tokenIdSubstring);
            materialTipColorChanger.SetRandomSeed(
                tokenIdInt / (i + 1));
            materialTipColorChanger.colorAlreadySet = false;
        }

        if (currentSpawnedLand != null)
        {
            Destroy(currentSpawnedLand);
        }

        currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
            landParent.transform);
    }

    static public string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}