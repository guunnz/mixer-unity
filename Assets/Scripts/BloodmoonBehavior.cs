using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BloodmoonBehavior : MonoBehaviour
{
    public RectTransform rectTransform;
    public Team GoodTeam;
    public Team BadTeam;


    void OnEnable()
    {
        DOTween.Init();

        // Sequence to chain animations
        Sequence mySequence = DOTween.Sequence();

        // Animate to left -85
        mySequence.Append(rectTransform.DOAnchorPosX(-85, .7f).From(new Vector2(-1315, rectTransform.anchoredPosition.y)));

        // Delay between animations
        mySequence.AppendInterval(0.5f);  // 0.5 second delay

        // Animate to left 600
        mySequence.Append(rectTransform.DOAnchorPosX(600, .4f));

        // Optionally, add a callback at the end
        mySequence.OnComplete(() => Debug.Log("Animation Completed!"));
        StartCoroutine(DamageBloodmoon());
    }


    IEnumerator DamageBloodmoon()
    {
        float percentage = 0.01f;
        while (!GoodTeam.battleEnded && !BadTeam.battleEnded)
        {
            GoodTeam.GetCharacters().ForEach(x => x.axieIngameStats.currentHP -= x.axieIngameStats.HP * percentage);

            percentage *= 1.5f;
            yield return new WaitForSeconds(1);
        }
    }
}