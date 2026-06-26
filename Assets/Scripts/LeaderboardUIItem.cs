using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;

public class LeaderboardUIItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI avgWinsText;
    public TextMeshProUGUI eloText;
    public TextMeshProUGUI rankText;
    public VanillaMonsterGraphic Captain;

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
            rankText.text = "<color=\"yellow\">" + rank + ".";
        }
        else if (rank == 2)
        {
            rankText.text = "<color=#C0C0C0>" + rank + ".";
        }
        else if (rank == 3)
        {
            rankText.text = "<color=#cd7f32>" + rank + ".";
        }
        else
        {
            rankText.text = rank + ".";
        }
    }

    public void SetCaptainGraphics(string monsterId, string monsterCaptainGenes)
    {
        MonsterVisualDescriptor descriptor = MonsterSpawner.Instance.SimpleProcessMixer(monsterId, monsterCaptainGenes, false);
        VanillaMonsterGraphic captain = EnsureCaptain();
        captain.SetDescriptor(descriptor);
        captain.startingAnimation = "action/idle/normal";
        captain.Initialize(true);
    }

    private VanillaMonsterGraphic EnsureCaptain()
    {
        Captain = VanillaMonsterGraphic.EnsureCenteredChild(transform, Captain, "Captain Monster Graphic");
        return Captain;
    }
}
