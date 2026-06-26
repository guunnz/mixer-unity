using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VFXPlayer : MonoBehaviour
{
    public VFXTestLauncher launcher;
    public VanillaMonsterVisual myMonster;
    public VanillaMonsterVisual Olek;
    public Image PlayImage;

    private MonsterVisualDescriptor currentDescriptor = MonsterVisualDescriptor.Default();

    private void Awake()
    {
        EnsurePreviewActors();
        SetActorVisibility(false);
        SetPlayOverlayVisible(true, true);
    }

    private void OnDisable()
    {
        StopVFX();
    }

    public void PlayMonsterVFX(SkillName skillName, BodyPart bodyPart, MonsterVisualDescriptor descriptor)
    {
        currentDescriptor = descriptor ?? MonsterVisualDescriptor.Default();
        EnsurePreviewActors();
        ApplyDescriptor();

        if (launcher == null)
            return;

        launcher.StopSkillPreview();
        SetPlayOverlayVisible(false, false);
        SetActorVisibility(true);

        launcher.playSkillCoroutine = StartCoroutine(launcher.PlaySkill(skillName, bodyPart));
    }

    public void SetUp(MonsterVisualDescriptor descriptor)
    {
        currentDescriptor = descriptor ?? MonsterVisualDescriptor.Default();
        EnsurePreviewActors();
        ApplyDescriptor();
        SetActorVisibility(true);
    }

    public void StopVFX()
    {
        if (launcher != null)
            launcher.StopSkillPreview();

        SetActorVisibility(false);
        SetPlayOverlayVisible(true, false);
    }

    private void EnsurePreviewActors()
    {
        if (launcher == null)
            launcher = GetComponentInChildren<VFXTestLauncher>(true);
        if (launcher == null)
            launcher = gameObject.AddComponent<VFXTestLauncher>();

        if (myMonster == null && launcher != null)
            myMonster = launcher.castFrom;
        if (Olek == null && launcher != null)
            Olek = launcher.target;

        if (myMonster == null)
            myMonster = EnsureActorOnExistingChild("MainMonster", currentDescriptor, new Vector3(-1.5f, 0f, 0f));

        if (Olek == null)
            Olek = EnsureActorOnExistingChild("OpponentMonster", MonsterVisualDescriptor.Default(MonsterClass.Plant), new Vector3(1.5f, 0f, 0f));

        if (myMonster != null)
            myMonster.SetFacing(false);
        if (Olek != null)
            Olek.SetFacing(true);

        if (launcher != null)
            launcher.SetActors(myMonster, Olek);
    }

    private VanillaMonsterVisual EnsureActorOnExistingChild(string childName, MonsterVisualDescriptor descriptor, Vector3 fallbackLocalPosition)
    {
        Transform child = transform.Find(childName);
        if (child != null)
            return VanillaMonsterVisual.Ensure(child.gameObject);

        VanillaMonsterVisual visual = VanillaMonsterVisual.Create(transform, descriptor);
        visual.transform.localPosition = fallbackLocalPosition;
        return visual;
    }

    private void ApplyDescriptor()
    {
        if (myMonster == null || Olek == null)
            return;

        myMonster.SetDescriptor(currentDescriptor ?? MonsterVisualDescriptor.Default());
        myMonster.Initialize(true);

        Olek.SetDescriptor(MonsterVisualDescriptor.Default(MonsterClass.Plant));
        Olek.Initialize(true);
    }

    private void SetActorVisibility(bool visible)
    {
        if (myMonster != null)
            myMonster.SetVisible(visible);
        if (Olek != null)
            Olek.SetVisible(visible);
    }

    private void SetPlayOverlayVisible(bool visible, bool immediate)
    {
        if (PlayImage == null)
            return;

        PlayImage.DOKill();
        Color color = PlayImage.color;
        color.a = visible ? 0.5f : 0f;

        if (immediate)
        {
            PlayImage.color = color;
            return;
        }

        PlayImage.DOColor(color, 0.5f);
    }
}
