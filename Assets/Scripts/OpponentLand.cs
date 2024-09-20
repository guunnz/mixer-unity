using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpponentLand : MonoBehaviour
{
    private GameObject currentSpawnedLand;
    public Transform landParent;
    private MaterialTipColorChanger[] landSquares;
    [SerializeField] private int indexChoosing = 0;
    [SerializeField] public LandType currentLandType = LandType.savannah;
    internal string currentSelectedLandId;
    [SerializeField] private List<SpawnableLand> spawnableLands;
    static public OpponentLand Instance;
    public FakeMapManager fakeMapManager;

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

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
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

            landSquares = fakeMapManager.materialTipColorChangerList.ToArray();
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
            SetLayerRecursively(currentSpawnedLand.transform, "LandEnemy");
            currentSpawnedLand.layer = LayerMask.NameToLayer("LandEnemy");
        }
        else
        {
            landSquares = fakeMapManager.materialTipColorChangerList.ToArray();
            RunManagerSingleton.instance.landType = currentLandType;
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
            SetLayerRecursively(currentSpawnedLand.transform, "LandEnemy");
            currentSpawnedLand.layer = LayerMask.NameToLayer("LandEnemy");
        }
    }

    public void ChooseFakeLand(LandType landType)
    {
        GetAxiesExample.Land land = null;

        currentLandType = landType;
        landSquares = fakeMapManager.materialTipColorChangerList.ToArray();
        RunManagerSingleton.instance.landType = currentLandType;
        for (int i = 0; i < landSquares.Length; i++)
        {
            MaterialTipColorChanger materialTipColorChanger = landSquares[i];
            materialTipColorChanger.landType = currentLandType;
            string tokenIdSubstring =
                AccountManager.userLands.results[indexChoosing].tokenId.Substring(0, 10);
            int tokenIdInt = int.Parse(tokenIdSubstring);
            materialTipColorChanger.SetRandomSeed(tokenIdInt / (i + 1));
            materialTipColorChanger.colorAlreadySet = false;
        }

        if (currentSpawnedLand != null)
        {
            Destroy(currentSpawnedLand);
        }

        currentSpawnedLand = Instantiate(spawnableLands.Single(x => x.landType == currentLandType).landPrefab,
            landParent);
        SetLayerRecursively(currentSpawnedLand.transform, "LandEnemy");
        currentSpawnedLand.layer = LayerMask.NameToLayer("LandEnemy");

    }
    void SetLayerRecursively(Transform root, string layerName)
    {
        // Set the layer of the current root
        root.gameObject.layer = LayerMask.NameToLayer(layerName);

        // Iterate over all children and apply the function recursively
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layerName);
        }
    }

}