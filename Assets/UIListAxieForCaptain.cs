using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIListAxieForCaptain : MonoBehaviour
{
    public List<AxieClassGraphic> axieClassGraphics = new List<AxieClassGraphic>();
    public GetAxiesExample.Axie axie;
    public Image selectedImage;
    public Image axieClassImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public TeamCaptainManager teamCaptainManager;
    public SkeletonGraphic skeletonGraphic;
    private Button button;
    internal bool selected => selectedImage.sprite == selectedSprite;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectAxie);
    }

    public void SelectAxie()
    {
        SkeletonDataAsset dataAsset = axie.skeletonDataAsset;

        if (dataAsset != null)
        {
            skeletonGraphic.skeletonDataAsset = dataAsset;
            skeletonGraphic.material = axie.skeletonDataAssetMaterial;
            skeletonGraphic.Initialize(true);
            Refresh(false);

            teamCaptainManager.SetAxieSelected(this.axie);
        }
        else
        {
            Refresh(false);
            skeletonGraphic.startingAnimation = "action/idle/normal";
            skeletonGraphic.Initialize(true);
            teamCaptainManager.SetAxieSelected(this.axie);
        }
    }

    public void Refresh(bool resetAxie = true)
    {
        if (axie == null || axie.@class == "")
        {
            skeletonGraphic.enabled = false;
            selectedImage.enabled = axie != null;
            axieClassImage.enabled = axie != null;
            return;
        }
        else if (resetAxie)
        {
            skeletonGraphic.enabled = true;
            skeletonGraphic.skeletonDataAsset = axie.skeletonDataAsset;
            skeletonGraphic.material = axie.skeletonDataAssetMaterial;
            skeletonGraphic.startingAnimation = "action/idle/normal";
            skeletonGraphic.Initialize(true);
        }

        selectedImage.enabled = axie != null;
        axieClassImage.enabled = axie != null;
        axieClassImage.sprite =
               axieClassGraphics.Single(x => x.axieClass == axie.axieClass).axieClassSprite;

        if (teamCaptainManager.selectedAxie != axie.id)
        {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            selectedImage.color = new Color(1, 1, 1, 0.3f);
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            selectedImage.color = new Color(1, 1, 1, 1f);
            selectedImage.sprite = selectedSprite;
        }
    }
}
