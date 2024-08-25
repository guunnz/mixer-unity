using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIListAxieForView : MonoBehaviour
{
    public List<AxieClassGraphic> axieClassGraphics = new List<AxieClassGraphic>();
    public GetAxiesExample.Axie axie;
    public Image selectedImage;
    public Image axieClassImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public AxiesView axiesView;
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

            axiesView.SetAxieSelected(this.axie, this.axieClassImage.sprite);
        }
        else
        {
            Refresh(false);
            skeletonGraphic.startingAnimation = "action/idle/normal";
            skeletonGraphic.Initialize(true);
            axiesView.SetAxieSelected(this.axie, this.axieClassImage.sprite);
        }
    }

    public void Refresh(bool resetAxie = true)
    {
        if (axie == null)
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

        if (axiesView.selectedAxie != axie.id)
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
