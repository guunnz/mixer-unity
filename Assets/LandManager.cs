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
    public MapManager mapManager;
    [SerializeField] private List<SpawnableLand> spawnableLands;
    private GameObject currentSpawnedLand;
    private LandType currentLandType;
    [SerializeField] private TextMeshProUGUI landTypeName;
    [SerializeField] private TextMeshProUGUI landCoordinates;

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
            currentLandType = AccountManager.userLands.results.Single(x => x.tokenId == tokenId).LandTypeEnum;
        }

        RunManagerSingleton.instance.landType = currentLandType;
        for (int i = 0; i < landSquares.Length; i++)
        {
            MaterialTipColorChanger materialTipColorChanger = landSquares[i];
            materialTipColorChanger.landType = currentLandType;
            materialTipColorChanger.SetRandomSeed(
                int.Parse(AccountManager.userLands.results[indexChoosing].tokenId) / (i + 1));
            materialTipColorChanger.colorAlreadySet = false;
        }

        landTypeName.text = CapitalizeFirstLetter(currentLandType.ToString());
        landCoordinates.text =
            $"({AccountManager.userLands.results[indexChoosing].col},{AccountManager.userLands.results[indexChoosing].row})";
        if (currentSpawnedLand != null)
        {
            Destroy(currentSpawnedLand);
        }

        currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab);
    }

    public string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}