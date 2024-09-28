using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MatchData
{
    public string username;
    public SkeletonDataAsset skeletonDataAsset;
    public Material skeletonmaterial;
    public int winIndex;
    public bool Win;
}

public class EndOfRunResults : MonoBehaviour
{
    public List<GameObject> GameobjectsToDisable;
    public List<Transform> FireworksPositions;
    public List<GameObject> FireworksPrefab;
    public Team GoodTeam;
    public GameObject MatchPrefab;
    public GameObject Container;
    public GameObject Items;

    private List<MatchData> MatchData = new List<MatchData>();
    public Transform MatchesParent;
    public TextMeshProUGUI ScoreText;

    public Image HP1;
    public Image HP2;
    public Image HP3;

    public SkeletonGraphic SkeletonGraphicCaptain;
    public TextMeshProUGUI CaptainText;

    static public EndOfRunResults Instance = new EndOfRunResults();

    private void Awake()
    {
        Instance = this;
    }

    public void SetMatchData(bool win)
    {
        MatchData matchdata = new MatchData();
        matchdata.username = CaptainText.text;
        matchdata.Win = win;
        matchdata.skeletonDataAsset = SkeletonGraphicCaptain.skeletonDataAsset;
        matchdata.skeletonmaterial = SkeletonGraphicCaptain.material;
        matchdata.winIndex = MatchData.Count + 1;
        MatchData.Add(matchdata);
    }
    public IEnumerator Fireworks()
    {
        while (true)
        {
            var position = FireworksPositions[Random.Range(0, FireworksPositions.Count)];
            var firework = Instantiate(FireworksPrefab[Random.Range(0, FireworksPrefab.Count)], position.position, Quaternion.identity, null);


            // Get the main particle system of the firework
            ParticleSystem mainParticleSystem = firework.GetComponent<ParticleSystem>();
            if (mainParticleSystem != null)
            {
                mainParticleSystem.Play();
            }

            // Get all child particle systems and play them
            foreach (var childParticleSystem in firework.GetComponentsInChildren<ParticleSystem>())
            {
                if (childParticleSystem != mainParticleSystem) // Skip the main one as it's already played
                {
                    childParticleSystem.Play();
                }
            }

            // Wait for random time before instantiating the next firework
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));

            // Start a coroutine to destroy the firework after it's done
            StartCoroutine(DestroyCoroutine(firework));
        }
    }


    public IEnumerator DestroyCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(2f);
        Destroy(obj);
    }

    public void ShowResults()
    {

        for (int i = 0; i < GameobjectsToDisable.Count; i++)
        {
            Destroy(GameobjectsToDisable[i]);
        }
        var wins = MatchData.Count(x => x.Win);
        ScoreText.text = wins.ToString();

        GoodTeam.GetCharactersAll().ForEach(x =>
        {
            if (wins == 12)
            {
                x.SkeletonAnim.AnimationName = "activity/victory-pose-back-flip";
            }
        });

        switch (MatchData.Count(x => !x.Win))
        {
            case 0:
                break;
            case 1:
                HP1.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                break;
            case 2:
                HP1.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                HP2.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                break;
            case 3:
                HP1.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                HP2.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                HP3.color = new Color(0.4622642f, 0.1118039f, 0.1024831f);
                break;
        }

        Container.SetActive(true);
        Items.SetActive(true);
        MatchData.ForEach(x => InstantiateMatch(x));
    }

    public void InstantiateMatch(MatchData matchData)
    {
        var uiItem = Instantiate(MatchPrefab, new Vector3(0, 0, 0), Quaternion.identity, MatchesParent);

        MatchResultUIItem matchResultUIItem = uiItem.GetComponent<MatchResultUIItem>();
        matchResultUIItem.SetMatch(matchData);
    }

    public void PlayAgain()
    {
        Loading.instance.EnableLoading();
        SceneManager.LoadScene(0);
    }
}
