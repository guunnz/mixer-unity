using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIListAxie : MonoBehaviour
{
    public List<AxieClassGraphic> axieClassGraphics = new List<AxieClassGraphic>();
    public GetAxiesExample.Axie axie;
    public Image selectedImage;
    public Image axieClassImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public FakeAxiesManager fakeAxiesManager;
    public SkeletonGraphic skeletonGraphic;
    private Button button;
    public TeamBuilderManager teamBuilderManager;
    internal bool selected => selectedImage.sprite == selectedSprite;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectAxie);
    }

    private void SelectAxie()
    {
        SkeletonDataAsset dataAsset = fakeAxiesManager.ChooseAxie(axie);

        if (dataAsset != null)
        {
            skeletonGraphic.skeletonDataAsset = dataAsset;
            skeletonGraphic.material = axie.skeletonDataAssetMaterial;
            skeletonGraphic.Initialize(true);
            Refresh(false);
        }
        else
        {
            Refresh(false);
            skeletonGraphic.startingAnimation = "action/idle/normal";
            skeletonGraphic.Initialize(true);
            teamBuilderManager.SetOtherAxieSelected();
        }
    }

    public void Refresh(bool resetAxie = true)
    {
        if (axie == null)
        {
            skeletonGraphic.enabled = false;
        }
        else if (resetAxie)
        {
            skeletonGraphic.enabled = true;
            skeletonGraphic.skeletonDataAsset = fakeAxiesManager.GetAxieArt(axie);
            skeletonGraphic.material = axie.skeletonDataAssetMaterial;
            skeletonGraphic.startingAnimation = "action/idle/normal";
            skeletonGraphic.Initialize(true);
        }

        selectedImage.enabled = axie != null;
        axieClassImage.enabled = axie != null;
        if (axie == null)
        {
            this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            selectedImage.color = new Color(1, 1, 1, 0.3f);
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            axieClassImage.sprite =
                axieClassGraphics.Single(x => x.axieClass == axie.axieClass).axieClassSprite;
            if (fakeAxiesManager.instantiatedAxies.Any(x => x.axie != null && x.axie.id == this.axie.id))
            {
                if (skeletonGraphic.startingAnimation != "action/idle/random-04")
                {
                    skeletonGraphic.startingAnimation = "action/idle/random-04";
                    skeletonGraphic.Initialize(true);
                }


                this.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f);

                selectedImage.color = new Color(1, 1, 1, 1f);
                selectedImage.sprite = selectedSprite;

                teamBuilderManager.SetAxieSelected(axie, axieClassImage.sprite);
            }
            else
            {
                this.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
                selectedImage.color = new Color(1, 1, 1, 0.3f);
                selectedImage.sprite = unselectedSprite;
            }
        }
    }
}