using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultUIItem : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
    public TextMeshProUGUI username;
    public TextMeshProUGUI runNumber;
    public Sprite WinTick;
    public Sprite LoseSkull;
    public Image WinLoseImage;

    public void SetMatch(MatchData matchData)
    {
        skeletonGraphic.skeletonDataAsset = matchData.skeletonDataAsset;
        skeletonGraphic.material = matchData.skeletonmaterial;
        skeletonGraphic.startingAnimation = "action/idle/normal";
        skeletonGraphic.Initialize(true);
        WinLoseImage.sprite = matchData.Win ? WinTick : LoseSkull;
        username.text = matchData.username;
        runNumber.text = matchData.winIndex.ToString();
    }
}
