using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIListAxie : MonoBehaviour
{
    public GetAxiesExample.Axie axie;
    public Image selectedImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public FakeAxiesManager fakeAxiesManager;
    public SkeletonGraphic skeletonGraphic;
    private Button button;
    public TeamBuilderManager teamBuilderManager;

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
        }

        Refresh(false);
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
            skeletonGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            skeletonGraphic.Initialize(true);
        }

        selectedImage.enabled = axie != null;
        if (axie == null)
        {
            selectedImage.sprite = unselectedSprite;
        }
        else
        {
            selectedImage.sprite =
                fakeAxiesManager.instantiatedAxies.Any(x => x.axie != null && x.axie.id == this.axie.id)
                    ? selectedSprite
                    : unselectedSprite;
        }
    }
}