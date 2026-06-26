using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class VanillaMonsterVisual : MonoBehaviour
{
    private static readonly Vector3 FrontFootBasePosition = new Vector3(-0.3f, -0.37f, -0.02f);
    private static readonly Vector3 BackFootBasePosition = new Vector3(0.34f, -0.37f, 0.02f);
    private static readonly Vector3 FootBaseScale = new Vector3(0.34f, 0.13f, 1f);
    private static readonly Vector3 AttackFlashBaseScale = new Vector3(0.28f, 0.28f, 1f);

    private readonly Dictionary<BodyPart, Transform> partAnchors = new Dictionary<BodyPart, Transform>();
    private readonly Dictionary<Transform, Vector3> anchorBasePositions = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<BodyPart, SpriteRenderer> partRenderers = new Dictionary<BodyPart, SpriteRenderer>();
    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    private Transform visualRoot;
    private SpriteRenderer bodyRenderer;
    private SpriteRenderer bellyRenderer;
    private SpriteRenderer faceRenderer;
    private SpriteRenderer bodyHighlightRenderer;
    private SpriteRenderer eyeRenderer;
    private SpriteRenderer eyeShineRenderer;
    private SpriteRenderer shadowRenderer;
    private SpriteRenderer frontFootRenderer;
    private SpriteRenderer backFootRenderer;
    private SpriteRenderer attackFlashRenderer;
    private MonsterVisualDescriptor descriptor = MonsterVisualDescriptor.Default();
    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale = MonsterScale.WorldVector;
    private MonsterVisualState currentState = MonsterVisualState.Idle;
    private bool facingPositiveX = true;
    private bool posePlaying;

    public bool loop = true;
    public float timeScale = 1f;

    public string AnimationName
    {
        get { return MonsterVisualStateMapper.ToLegacyName(currentState); }
        set { Play(MonsterVisualStateMapper.FromLegacyName(value), loop); }
    }

    public MonsterVisualState CurrentState => currentState;
    public bool FacingPositiveX => facingPositiveX;
    public Renderer MainRenderer => bodyRenderer;
    public Transform BodyAnchor { get; private set; }
    public Transform OverheadAnchor { get; private set; }

    private void Awake()
    {
        EnsureBuilt();
    }

    private void OnDisable()
    {
        KillPoseTweens();
    }

    public static VanillaMonsterVisual Create(Transform parent, MonsterVisualDescriptor descriptor)
    {
        GameObject go = new GameObject("Vanilla Monster Visual");
        go.transform.SetParent(parent, false);
        VanillaMonsterVisual visual = go.AddComponent<VanillaMonsterVisual>();
        visual.SetDescriptor(descriptor);
        return visual;
    }

    public static VanillaMonsterVisual Ensure(GameObject go)
    {
        VanillaMonsterVisual visual = go.GetComponent<VanillaMonsterVisual>();
        if (visual == null)
            visual = go.AddComponent<VanillaMonsterVisual>();

        visual.EnsureBuilt();
        return visual;
    }

    public void SetDescriptor(MonsterVisualDescriptor newDescriptor)
    {
        EnsureBuilt();
        descriptor = newDescriptor ?? MonsterVisualDescriptor.Default();

        Color main = MonsterClassPalette.Main(descriptor.Class);
        Color accent = MonsterClassPalette.Accent(descriptor.Class);
        Color shadow = MonsterClassPalette.Shadow(descriptor.Class);

        ApplyPartSprite(bodyRenderer, "body", VanillaSpriteShape.Ellipse, main);
        ApplyPartSprite(bellyRenderer, "belly", VanillaSpriteShape.Ellipse, accent);
        ApplyPartSprite(faceRenderer, "face", VanillaSpriteShape.Circle, Color.Lerp(accent, Color.white, 0.25f));
        Color highlight = Color.Lerp(main, Color.white, 0.45f);
        ApplyPartSprite(bodyHighlightRenderer, "highlight", VanillaSpriteShape.Ellipse, new Color(highlight.r, highlight.g, highlight.b, 0.42f));
        ApplyPartSprite(eyeRenderer, "eye", VanillaSpriteShape.Circle, new Color(0.08f, 0.08f, 0.1f, 1f));
        ApplyPartSprite(eyeShineRenderer, "eye-shine", VanillaSpriteShape.Circle, new Color(1f, 1f, 1f, 0.92f));
        ApplyPartSprite(shadowRenderer, "shadow", VanillaSpriteShape.Ellipse, new Color(shadow.r, shadow.g, shadow.b, 0.32f));
        ApplyPartSprite(frontFootRenderer, "front-foot", VanillaSpriteShape.Ellipse, new Color(shadow.r, shadow.g, shadow.b, 0.88f));
        ApplyPartSprite(backFootRenderer, "back-foot", VanillaSpriteShape.Ellipse, new Color(shadow.r, shadow.g, shadow.b, 0.7f));
        ApplyPartSprite(attackFlashRenderer, "attack-flash", VanillaSpriteShape.Diamond, new Color(accent.r, accent.g, accent.b, 0f));

        ApplyPartColors();
    }

    public void Initialize(bool resetState)
    {
        EnsureBuilt();
        if (resetState)
            Play(MonsterVisualState.Idle, true);
    }

    public void SetFacing(bool positiveX)
    {
        EnsureBuilt();
        float magnitude = Mathf.Abs(MonsterScale.WorldVisual);
        Vector3 desiredScale = new Vector3(positiveX ? magnitude : -magnitude, magnitude, magnitude);
        if (facingPositiveX == positiveX && baseLocalScale == desiredScale)
            return;

        facingPositiveX = positiveX;
        baseLocalScale = desiredScale;
        ApplyAnchorFacing();

        if (!CanReusePose(currentState, loop))
            return;

        KillPoseTweens();
        Play(currentState, loop, timeScale);
    }

    public void Play(MonsterVisualState state, bool shouldLoop = true, float speed = 1f)
    {
        EnsureBuilt();
        float normalizedSpeed = Mathf.Max(0.01f, speed);
        if (posePlaying && CanReusePose(state, shouldLoop) && currentState == state && loop == shouldLoop && Mathf.Approximately(timeScale, normalizedSpeed))
            return;

        loop = shouldLoop;
        timeScale = normalizedSpeed;
        currentState = state;

        ResetPose();

        SetVisible(state != MonsterVisualState.Hidden);

        float duration = Mathf.Max(0.05f, GetDuration(state) / timeScale);
        switch (state)
        {
            case MonsterVisualState.Run:
                AnimateWalk(duration);
                break;
            case MonsterVisualState.Hover:
                visualRoot.DOScale(baseLocalScale * 1.08f, duration * 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
            case MonsterVisualState.Grabbed:
                visualRoot.DORotate(new Vector3(0f, 0f, 7f), duration * 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
            case MonsterVisualState.AttackMelee:
                AnimateMeleeAttack(duration, shouldLoop);
                break;
            case MonsterVisualState.AttackRanged:
                AnimateRangedAttack(duration, shouldLoop);
                break;
            case MonsterVisualState.Cast:
                AnimateCastJump(duration, shouldLoop);
                break;
            case MonsterVisualState.Hit:
                visualRoot.DOShakePosition(duration, 0.09f, 8, 35f, false, true);
                break;
            case MonsterVisualState.Victory:
                visualRoot.DOLocalMoveY(baseLocalPosition.y + 0.18f, duration * 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                visualRoot.DORotate(new Vector3(0f, 0f, 5f), duration * 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
            case MonsterVisualState.Shrimp:
                AnimateMeleeAttack(duration, shouldLoop);
                break;
            case MonsterVisualState.Appear:
                visualRoot.localScale = Vector3.zero;
                visualRoot.DOScale(baseLocalScale, duration).SetEase(Ease.OutBack);
                break;
            case MonsterVisualState.Idle:
            default:
                visualRoot.DOLocalMoveY(baseLocalPosition.y + 0.06f, duration * 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
        }

        posePlaying = true;
    }

    public float GetDuration(MonsterVisualState state)
    {
        switch (state)
        {
            case MonsterVisualState.Run:
                return 0.42f;
            case MonsterVisualState.AttackMelee:
            case MonsterVisualState.AttackRanged:
                return 0.85f;
            case MonsterVisualState.Cast:
                return 1.1f;
            case MonsterVisualState.Shrimp:
                return 0.9f;
            case MonsterVisualState.Hit:
                return 0.45f;
            case MonsterVisualState.Appear:
                return 0.55f;
            default:
                return 1f;
        }
    }

    public void SetSortingOrder(int order)
    {
        EnsureBuilt();
        for (int i = 0; i < renderers.Count; i++)
            renderers[i].sortingOrder = order + i;

        attackFlashRenderer.sortingOrder = order + renderers.Count + 1;
    }

    public void SetVisible(bool visible)
    {
        EnsureBuilt();
        foreach (SpriteRenderer spriteRenderer in renderers)
            spriteRenderer.enabled = visible;

        attackFlashRenderer.enabled = false;
    }

    public Transform GetPartAnchor(BodyPart part)
    {
        EnsureBuilt();
        return partAnchors.TryGetValue(part, out Transform anchor) ? anchor : BodyAnchor;
    }

    private void EnsureBuilt()
    {
        if (visualRoot != null)
            return;

        visualRoot = new GameObject("Visual Root").transform;
        visualRoot.SetParent(transform, false);
        baseLocalPosition = Vector3.zero;
        baseLocalScale = MonsterScale.WorldVector;

        shadowRenderer = CreateRenderer("Shadow", VanillaSpriteShape.Ellipse, new Vector3(0f, -0.28f, 0.04f), new Vector3(1.35f, 0.22f, 1f), -3);
        backFootRenderer = CreateRenderer("Back Foot", VanillaSpriteShape.Ellipse, BackFootBasePosition, FootBaseScale, -2);
        frontFootRenderer = CreateRenderer("Front Foot", VanillaSpriteShape.Ellipse, FrontFootBasePosition, FootBaseScale, -1);
        bodyRenderer = CreateRenderer("Body", VanillaSpriteShape.Ellipse, Vector3.zero, new Vector3(1.25f, 0.8f, 1f), 0);
        bellyRenderer = CreateRenderer("Belly", VanillaSpriteShape.Ellipse, new Vector3(0.08f, -0.06f, -0.02f), new Vector3(0.65f, 0.42f, 1f), 1);
        bodyHighlightRenderer = CreateRenderer("Body Highlight", VanillaSpriteShape.Ellipse, new Vector3(-0.18f, 0.2f, -0.035f), new Vector3(0.48f, 0.2f, 1f), 2);
        faceRenderer = CreateRenderer("Face", VanillaSpriteShape.Circle, new Vector3(-0.36f, 0.09f, -0.04f), new Vector3(0.28f, 0.28f, 1f), 3);
        eyeRenderer = CreateRenderer("Eye", VanillaSpriteShape.Circle, new Vector3(-0.46f, 0.18f, -0.06f), new Vector3(0.075f, 0.075f, 1f), 4);
        eyeShineRenderer = CreateRenderer("Eye Shine", VanillaSpriteShape.Circle, new Vector3(-0.485f, 0.2f, -0.07f), new Vector3(0.024f, 0.024f, 1f), 5);
        attackFlashRenderer = CreateRenderer("Attack Flash", VanillaSpriteShape.Diamond, new Vector3(-0.72f, 0.1f, -0.05f), AttackFlashBaseScale, 8, false);
        attackFlashRenderer.enabled = false;

        BodyAnchor = CreateAnchor("Body Anchor", new Vector3(0f, 0.08f, 0f));
        OverheadAnchor = CreateAnchor("Overhead Anchor", new Vector3(0f, 0.78f, 0f));
        CreatePartAnchor(BodyPart.Eyes, new Vector3(-0.44f, 0.16f, 0f), VanillaSpriteShape.Circle, new Vector3(0.08f, 0.08f, 1f));
        CreatePartAnchor(BodyPart.Ears, new Vector3(-0.3f, 0.43f, 0f), VanillaSpriteShape.Triangle, new Vector3(0.18f, 0.18f, 1f));
        CreatePartAnchor(BodyPart.Horn, new Vector3(-0.08f, 0.46f, 0f), VanillaSpriteShape.Triangle, new Vector3(0.18f, 0.2f, 1f));
        CreatePartAnchor(BodyPart.Mouth, new Vector3(-0.5f, -0.02f, 0f), VanillaSpriteShape.RoundedBox, new Vector3(0.14f, 0.07f, 1f));
        CreatePartAnchor(BodyPart.Back, new Vector3(0.2f, 0.44f, 0f), VanillaSpriteShape.Diamond, new Vector3(0.2f, 0.2f, 1f));
        CreatePartAnchor(BodyPart.Tail, new Vector3(0.68f, 0.04f, 0f), VanillaSpriteShape.Triangle, new Vector3(0.22f, 0.18f, 1f));

        SetDescriptor(descriptor);
    }

    private SpriteRenderer CreateRenderer(string name, VanillaSpriteShape shape, Vector3 localPosition, Vector3 localScale, int order, bool tracked = true)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(visualRoot, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;

        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = VanillaSpriteCache.Get(shape);
        spriteRenderer.sortingOrder = order;
        if (tracked)
            renderers.Add(spriteRenderer);
        return spriteRenderer;
    }

    private void ResetPose()
    {
        KillPoseTweens();

        visualRoot.localPosition = baseLocalPosition;
        visualRoot.localScale = baseLocalScale;
        visualRoot.localRotation = Quaternion.identity;

        ResetRendererTransform(frontFootRenderer, FrontFootBasePosition, FootBaseScale);
        ResetRendererTransform(backFootRenderer, BackFootBasePosition, FootBaseScale);
        ResetRendererTransform(attackFlashRenderer, new Vector3(-0.72f, 0.1f, -0.05f), AttackFlashBaseScale);
        attackFlashRenderer.enabled = false;
    }

    private void KillPoseTweens()
    {
        visualRoot?.DOKill();
        frontFootRenderer?.transform.DOKill();
        backFootRenderer?.transform.DOKill();
        attackFlashRenderer?.transform.DOKill();
        attackFlashRenderer?.DOKill();
        posePlaying = false;
    }

    private void ResetRendererTransform(SpriteRenderer spriteRenderer, Vector3 localPosition, Vector3 localScale)
    {
        if (spriteRenderer == null)
            return;

        Transform rendererTransform = spriteRenderer.transform;
        rendererTransform.localPosition = localPosition;
        rendererTransform.localScale = localScale;
        rendererTransform.localRotation = Quaternion.identity;
    }

    private void AnimateWalk(float duration)
    {
        float halfStep = duration * 0.5f;
        visualRoot.DOLocalMoveY(baseLocalPosition.y + 0.1f, halfStep)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        visualRoot.DORotate(new Vector3(0f, 0f, facingPositiveX ? -4f : 4f), halfStep)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        AnimateFoot(frontFootRenderer.transform, FrontFootBasePosition, -0.12f, duration, 0f);
        AnimateFoot(backFootRenderer.transform, BackFootBasePosition, 0.12f, duration, halfStep);
    }

    private void AnimateFoot(Transform foot, Vector3 basePosition, float stride, float duration, float delay)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(foot.DOLocalMove(new Vector3(basePosition.x + stride, basePosition.y + 0.05f, basePosition.z), duration * 0.25f).SetEase(Ease.OutSine));
        sequence.Join(foot.DOScale(new Vector3(FootBaseScale.x * 1.12f, FootBaseScale.y * 0.82f, FootBaseScale.z), duration * 0.25f).SetEase(Ease.OutSine));
        sequence.Append(foot.DOLocalMove(new Vector3(basePosition.x - stride, basePosition.y, basePosition.z), duration * 0.25f).SetEase(Ease.InSine));
        sequence.Join(foot.DOScale(FootBaseScale, duration * 0.25f).SetEase(Ease.InSine));
        sequence.AppendInterval(Mathf.Max(0.01f, duration * 0.5f - delay));
        sequence.SetLoops(-1);
    }

    private void AnimateMeleeAttack(float duration, bool shouldLoop)
    {
        float forward = ForwardSign();
        float windupRotation = facingPositiveX ? -10f : 10f;
        float strikeRotation = facingPositiveX ? 18f : -18f;
        float recoilRotation = facingPositiveX ? -6f : 6f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOLocalMoveX(baseLocalPosition.x - forward * 0.08f, duration * 0.16f).SetEase(Ease.OutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, windupRotation), duration * 0.16f).SetEase(Ease.OutSine));
        sequence.Append(visualRoot.DOLocalMoveX(baseLocalPosition.x + forward * 0.26f, duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, strikeRotation), duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.AppendCallback(delegate { PlayAttackFlash(forward, duration, false); });
        sequence.Append(visualRoot.DOLocalMoveX(baseLocalPosition.x - forward * 0.05f, duration * 0.18f).SetEase(Ease.OutBack));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, recoilRotation), duration * 0.18f).SetEase(Ease.OutBack));
        sequence.Append(visualRoot.DOLocalMoveX(baseLocalPosition.x, duration * 0.2f).SetEase(Ease.InOutSine));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.2f).SetEase(Ease.InOutSine));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.18f);
            sequence.SetLoops(-1);
        }
    }

    private void AnimateRangedAttack(float duration, bool shouldLoop)
    {
        float forward = ForwardSign();
        float windupRotation = facingPositiveX ? -8f : 8f;
        float castRotation = facingPositiveX ? 12f : -12f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOLocalMoveY(baseLocalPosition.y + 0.12f, duration * 0.2f).SetEase(Ease.OutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, windupRotation), duration * 0.2f).SetEase(Ease.OutSine));
        sequence.AppendCallback(delegate { PlayAttackFlash(forward, duration, true); });
        sequence.Append(visualRoot.DOLocalMove(new Vector3(baseLocalPosition.x - forward * 0.06f, baseLocalPosition.y - 0.03f, baseLocalPosition.z), duration * 0.18f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, castRotation), duration * 0.18f).SetEase(Ease.OutQuad));
        sequence.Append(visualRoot.DOLocalMove(baseLocalPosition, duration * 0.28f).SetEase(Ease.OutBack));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.28f).SetEase(Ease.OutBack));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.2f);
            sequence.SetLoops(-1);
        }
    }

    private void AnimateCastJump(float duration, bool shouldLoop)
    {
        float forward = ForwardSign();
        float liftRotation = facingPositiveX ? -8f : 8f;
        float castRotation = facingPositiveX ? 13f : -13f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOLocalMove(
            new Vector3(baseLocalPosition.x - forward * 0.03f, baseLocalPosition.y + 0.34f, baseLocalPosition.z),
            duration * 0.28f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, liftRotation), duration * 0.28f).SetEase(Ease.OutSine));
        sequence.AppendCallback(delegate { PlayAttackFlash(forward, duration, true); });
        sequence.Append(visualRoot.DOLocalMove(
            new Vector3(baseLocalPosition.x + forward * 0.05f, baseLocalPosition.y + 0.16f, baseLocalPosition.z),
            duration * 0.16f).SetEase(Ease.InOutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, castRotation), duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.Append(visualRoot.DOLocalMove(baseLocalPosition, duration * 0.28f).SetEase(Ease.InQuad));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.28f).SetEase(Ease.InOutSine));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.18f);
            sequence.SetLoops(-1);
        }
    }

    private void PlayAttackFlash(float forward, float duration, bool ranged)
    {
        Transform flashTransform = attackFlashRenderer.transform;
        flashTransform.DOKill();
        attackFlashRenderer.DOKill();

        MonsterVisualDescriptor activeDescriptor = descriptor ?? MonsterVisualDescriptor.Default();
        Color color = MonsterClassPalette.Accent(activeDescriptor.Class);
        attackFlashRenderer.color = new Color(color.r, color.g, color.b, 0.85f);
        attackFlashRenderer.enabled = true;
        flashTransform.localPosition = new Vector3(-0.72f, ranged ? 0.18f : 0.04f, -0.05f);
        flashTransform.localScale = Vector3.zero;
        flashTransform.localRotation = Quaternion.Euler(0f, 0f, ranged ? 45f : -20f * forward);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(flashTransform.DOScale(ranged ? AttackFlashBaseScale * 1.35f : AttackFlashBaseScale, duration * 0.12f).SetEase(Ease.OutBack));
        sequence.Join(flashTransform.DOLocalMoveX(-0.18f, duration * 0.18f).SetRelative().SetEase(Ease.OutQuad));
        sequence.Append(attackFlashRenderer.DOFade(0f, duration * 0.18f).SetEase(Ease.InQuad));
        sequence.OnComplete(delegate { attackFlashRenderer.enabled = false; });
    }

    private float ForwardSign()
    {
        return facingPositiveX ? -1f : 1f;
    }

    private static bool CanReusePose(MonsterVisualState state, bool shouldLoop)
    {
        return shouldLoop || state == MonsterVisualState.Hidden;
    }

    private Transform CreateAnchor(string name, Vector3 localPosition)
    {
        Transform anchor = new GameObject(name).transform;
        anchor.SetParent(transform, false);
        anchorBasePositions[anchor] = localPosition * MonsterScale.WorldVisual;
        ApplyAnchorFacing(anchor);
        return anchor;
    }

    private void CreatePartAnchor(BodyPart part, Vector3 localPosition, VanillaSpriteShape shape, Vector3 localScale)
    {
        Transform anchor = CreateAnchor(part + " Anchor", localPosition);
        partAnchors[part] = anchor;

        SpriteRenderer marker = CreateRenderer(part + " Marker", shape, localPosition, localScale * 0.45f, 6);
        partRenderers[part] = marker;
        marker.color = MonsterClassPalette.BodyPartColor(part);
    }

    private void ApplyPartSprite(SpriteRenderer spriteRenderer, string partKey, VanillaSpriteShape fallbackShape, Color fallbackColor)
    {
        if (spriteRenderer == null)
            return;

        if (VanillaMonsterSpriteLibrary.TryGetPartSprite(descriptor.Class, partKey, out Sprite sprite))
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            return;
        }

        spriteRenderer.sprite = VanillaSpriteCache.Get(fallbackShape);
        spriteRenderer.color = fallbackColor;
    }

    private void ApplyAnchorFacing()
    {
        foreach (Transform anchor in anchorBasePositions.Keys)
            ApplyAnchorFacing(anchor);
    }

    private void ApplyAnchorFacing(Transform anchor)
    {
        if (anchor == null || !anchorBasePositions.TryGetValue(anchor, out Vector3 basePosition))
            return;

        anchor.localPosition = new Vector3(facingPositiveX ? basePosition.x : -basePosition.x, basePosition.y, basePosition.z);
    }

    private void ApplyPartColors()
    {
        foreach (KeyValuePair<BodyPart, SpriteRenderer> partRenderer in partRenderers)
        {
            if (VanillaMonsterSpriteLibrary.TryGetBodyPartSprite(descriptor.Class, partRenderer.Key, out Sprite sprite))
            {
                partRenderer.Value.sprite = sprite;
                partRenderer.Value.color = Color.white;
            }
            else
            {
                partRenderer.Value.sprite = VanillaSpriteCache.Get(VanillaMonsterIconUtility.GetBodyPartShape(partRenderer.Key));
                partRenderer.Value.color = MonsterClassPalette.BodyPartColor(partRenderer.Key);
            }
        }
    }
}

public static class MonsterVisualStateMapper
{
    public static MonsterVisualState FromLegacyName(string animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return MonsterVisualState.Idle;

        string normalized = animationName.ToLowerInvariant();
        if (normalized.Contains("run") || normalized.Contains("move"))
            return MonsterVisualState.Run;
        if (normalized.Contains("shrimp"))
            return MonsterVisualState.Shrimp;
        if (normalized.Contains("victory"))
            return MonsterVisualState.Victory;
        if (normalized.Contains("debuff") || normalized.Contains("hit") || normalized.Contains("buff"))
            return MonsterVisualState.Hit;
        if (normalized.Contains("random-01"))
            return MonsterVisualState.Hover;
        if (normalized.Contains("random-03"))
            return MonsterVisualState.Grabbed;
        if (normalized.Contains("cast"))
            return MonsterVisualState.Cast;
        if (normalized.Contains("ranged"))
            return MonsterVisualState.AttackRanged;
        if (normalized.Contains("attack"))
            return MonsterVisualState.AttackMelee;
        if (normalized.Contains("appear") || normalized.Contains("entrance"))
            return MonsterVisualState.Appear;

        return MonsterVisualState.Idle;
    }

    public static MonsterVisualState FromMonsterAnimation(MonsterAnimation animation)
    {
        return FromLegacyName(animation.ToString().Replace("_", "/"));
    }

    public static string ToLegacyName(MonsterVisualState state)
    {
        switch (state)
        {
            case MonsterVisualState.Run:
                return "action/run";
            case MonsterVisualState.Hover:
                return "action/idle/random-01";
            case MonsterVisualState.Grabbed:
                return "action/idle/random-03";
            case MonsterVisualState.AttackMelee:
                return "attack/melee/normal-attack";
            case MonsterVisualState.AttackRanged:
                return "attack/ranged/cast-multi";
            case MonsterVisualState.Cast:
                return "attack/ranged/cast-high";
            case MonsterVisualState.Hit:
                return "battle/get-debuff";
            case MonsterVisualState.Victory:
                return "activity/victory-pose-back-flip";
            case MonsterVisualState.Shrimp:
                return "attack/melee/shrimp";
            case MonsterVisualState.Appear:
                return "activity/appear";
            case MonsterVisualState.Hidden:
                return "hidden";
            default:
                return "action/idle/normal";
        }
    }
}
