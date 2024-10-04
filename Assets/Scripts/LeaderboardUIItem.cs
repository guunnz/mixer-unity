using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;
using Spine.Unity;

public class LeaderboardUIItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI avgWinsText;
    public TextMeshProUGUI eloText;
    public TextMeshProUGUI rankText;
    public SkeletonGraphic Captain;

    public void SetUsername(string username)
    {
        if (usernameText != null)
            usernameText.text = username.Length <= 16 ? username : username.Substring(0, 16);
    }

    public void SetAvgWins(float avgWins)
    {
        if (avgWinsText != null)
            avgWinsText.text = "Avg Wins: " + avgWins.ToString();
    }

    public void SetElo(int elo)
    {
        if (eloText != null)
            eloText.text = elo.ToString();
    }

    public void SetRanking(int rank)
    {
      

        if (rank == 1)
        {
            rankText.text ="<color=\"yellow\">"+ rank + "°";
        }
        else if (rank == 2)
        {
            rankText.text = "<color=#C0C0C0>" + rank + "°";
        }
        else if (rank == 3)
        {
            rankText.text = "<color=#cd7f32>" + rank + "°";
        }
        else
        {
            rankText.text = rank + "°";
        }
    }

    public void SetCaptainGraphics(string axieId, string axieCaptainGenes)
    {
        var builderResult = AxieSpawner.Instance.SimpleProcessMixer(axieId, axieCaptainGenes, true);

        Captain.skeletonDataAsset = builderResult.skeletonDataAsset;
        Captain.material = builderResult.sharedGraphicMaterial;
        Captain.startingAnimation = "action/idle/normal";
        Captain.Initialize(true);
    }
}
