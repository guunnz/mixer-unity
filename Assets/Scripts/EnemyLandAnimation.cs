using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class LandPortals
{
    public LandType portalType;
    public GameObject portalObject;
}


public class EnemyLandAnimation : MonoBehaviour
{
    public Team EnemyTeam;
    public GameObject BigWall;
    public List<LandPortals> LandPortals;
    public GameObject PortalSphere;
    public LandType animToTest;

    static public EnemyLandAnimation instance;

    private void Awake()
    {
        instance = this;
    }

    public void DoAnimation(LandType landType)
    {
        StartCoroutine(IDoAnimation(landType));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            DoAnimation(animToTest);
        }
    }

    public void StartBatchReset(LandType landType)
    {
        StartCoroutine(BatchResetColorCoroutine(landType));
    }

    private IEnumerator BatchResetColorCoroutine(LandType landType)
    {
        yield return new WaitForSeconds(1.5f);
        List<MaterialTipColorChanger> enemyTiles = MapManager.Instance.enemyTiles;
        int batchCount = 5;

        for (int i = 0; i < enemyTiles.Count; i += batchCount)
        {
            // Process up to 5 items at a time
            for (int j = i; j < i + batchCount && j < enemyTiles.Count; j++)
            {
                enemyTiles[j].ResetColor(landType);
            }

            // Wait for a specified delay before processing the next batch
            yield return new WaitForSeconds(.3f); // Delay of 1 second between batches
        }
    }
    private IEnumerator IDoAnimation(LandType landType)
    {
        BigWall.transform.position = new Vector3(12, 1.8f, 2.17f);
        PortalSphere.gameObject.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
        StartBatchReset(landType);
        OpponentLand.Instance.ChooseFakeLandMobile(landType);
#else
          OpponentLand.Instance.ChooseFakeLand(landType);
#endif
        var portal = LandPortals.FirstOrDefault(x => x.portalType == landType);
        portal.portalObject.gameObject.SetActive(true);
        PortalSphere.transform.position = portal.portalObject.transform.position;
        if (landType == LandType.genesis || landType == LandType.lunalanding || landType == LandType.arctic)
        {
            portal.portalObject.transform.DOScale(new Vector3(1.2f, 0.2f, 1.7f), 1f);
        }
        else
        {
            portal.portalObject.transform.DOScale(new Vector3(1, 1, 1), 1f);
        }

        yield return new WaitForSeconds(0.5f);
        PortalSphere.transform.DOScale(new Vector3(.6f, .6f, .6f), 1.25f);

        yield return new WaitForSeconds(1.25f);

        PortalSphere.transform.DOScale(new Vector3(10, 1, 5), 1);
        yield return new WaitForSeconds(0.2f);
        portal.portalObject.gameObject.SetActive(false);

        BigWall.transform.DOMoveX(3.77f, 1f);
        yield return new WaitForSeconds(1);
        PortalSphere.transform.localScale = Vector3.zero;
        if (EnemyTeam != null)
            EnemyTeam.GetCharactersAll().ForEach(x => SetLayerRecursively(x.transform, "Default"));
    }

    public void ResetAnimation()
    {
        MapManager.Instance.enemyTiles.ForEach(x => x.ResetColor(RunManagerSingleton.instance.landType));
        PortalSphere.gameObject.SetActive(false);
        BigWall.transform.DOMoveX(12f, 1f);
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
