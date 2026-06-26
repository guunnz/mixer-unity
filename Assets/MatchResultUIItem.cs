using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultUIItem : MonoBehaviour
{
    public VanillaMonsterGraphic monsterGraphic;
    public TextMeshProUGUI username;
    public TextMeshProUGUI runNumber;
    public Sprite WinTick;
    public Sprite LoseSkull;
    public Image WinLoseImage;

    public void SetMatch(MatchData matchData)
    {
        VanillaMonsterGraphic graphic = EnsureGraphic();
        graphic.SetDescriptor(matchData.visualDescriptor);
        graphic.startingAnimation = "action/idle/normal";
        graphic.Initialize(true);
        WinLoseImage.sprite = matchData.Win ? WinTick : LoseSkull;
        username.text = matchData.username;
        runNumber.text = matchData.winIndex.ToString();
    }

    private VanillaMonsterGraphic EnsureGraphic()
    {
        monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(transform, monsterGraphic);
        return monsterGraphic;
    }
}
