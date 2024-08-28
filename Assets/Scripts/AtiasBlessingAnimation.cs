using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AtiasBlessingAnimation : MonoBehaviour
{
    public List<Image> BlessingsList = new List<Image>();
    public List<TextMeshProUGUI> BlessingsTextsList = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> BlessingsWhiteTextsList = new List<TextMeshProUGUI>();
    public List<Image> BlessingGlow;
    public GameObject BlessingGridLayoutGroup;
    public Image AtiaImage;

    private void DoBlessingScale(float scaleX, float scaleY, float delay)
    {
        foreach (var item in BlessingGlow)
        {
            item.transform.DOScale(new Vector3(scaleX, scaleY, scaleX), delay);
        }
    }

    public void DoAnim()
    {
        StartCoroutine(DoAnimation());
    }

    public IEnumerator DoAnimation()
    {
        BlessingsList.ForEach(x => x.color = new Color(1, 1, 1, 0));
        BlessingsTextsList.ForEach(x => x.color = new Color(1, 1, 1, 0));
        BlessingsWhiteTextsList.ForEach(x => x.color = new Color(1, 1, 1, 0));
        BlessingGlow.ForEach(x => x.DOColor(Color.white, 1f));

        var rectTransformAtia = AtiaImage.GetComponent<RectTransform>();
        AtiaImage.color = Color.white;
        rectTransformAtia.DOAnchorPosX(-485, 0);

        yield return new WaitForFixedUpdate();
        AtiaImage.gameObject.SetActive(true);
        rectTransformAtia.DOAnchorPosX(0, 0.4f);
        yield return new WaitForSeconds(.4f);

        rectTransformAtia.DOScale(new Vector3(1.5f, 1.5f, 12f), 0.3f);
        yield return new WaitForSeconds(0.29f);
        rectTransformAtia.DOScale(new Vector3(1f, 1f, 12f), .9f);
        AtiaImage.DOColor(Color.clear, .9f);

        yield return new WaitForSeconds(.4f);
        BlessingGridLayoutGroup.SetActive(true);
        DoBlessingScale(5, 5, 0.5f);

        foreach (var item in BlessingsTextsList)
        {
            item.DOColor(new Color(0.988f, 0.890f, 0.811f, 1), 0.5f);
        }

        foreach (var item in BlessingsWhiteTextsList)
        {
            item.DOColor(Color.white, 0.5f);
        }
        foreach (var item in BlessingsList)
        {
            item.DOColor(new Color(1f, 1f,1f,1), 1f);
        }

        yield return new WaitForSeconds(.5f);
        DoBlessingScale(5, .1f, 1f);
        BlessingGlow.ForEach(x => x.DOColor(new Color(1, 1, 1, 0f), 0.5f));
        BlessingGlow.ForEach(x => x.rectTransform.DOAnchorPosY(-250, .5f));
    }
}