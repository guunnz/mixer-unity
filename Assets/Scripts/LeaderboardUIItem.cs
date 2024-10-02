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
    public SkeletonGraphic Captain;

    public void SetUsername(string username)
    {
        if (usernameText != null)
            usernameText.text = username;
    }

    public void SetAvgWins(int avgWins)
    {
        if (avgWinsText != null)
            avgWinsText.text = "Avg Wins: " + avgWins.ToString();
    }

    public void SetElo(int elo)
    {
        if (eloText != null)
            eloText.text = "Elo: " + elo.ToString();
    }

    public void SetCaptainGraphics(string axieId, string axieCaptainGenes)
    {
        var builderResult = AxieSpawner.Instance.SimpleProcessMixer(axieId, axieCaptainGenes, true);

        Captain.skeletonDataAsset = builderResult.skeletonDataAsset;
        Captain.material = builderResult.sharedGraphicMaterial;
        Captain.startingAnimation = "action/idle/normal/";
        Captain.Initialize(true);
    }
}
