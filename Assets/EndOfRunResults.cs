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
    public Team GoodTeam;
    public GameObject MatchPrefab;
    public GameObject Container;

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

    public void ShowResults()
    {
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
        SceneManager.LoadScene(0);
    }
}
