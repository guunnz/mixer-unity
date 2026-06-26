using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class VanillaMonsterGraphic : MonoBehaviour
{
    private static readonly Vector2 DefaultVisualSize = new Vector2(160f, 120f);
    private static readonly Vector2 AttackFlashPosition = new Vector2(-72f, 8f);
    private static readonly Vector2 AttackFlashSize = new Vector2(28f, 28f);
    private const string DefaultCenteredChildName = "Vanilla Monster Graphic";

    private readonly List<Image> images = new List<Image>();
    private readonly Dictionary<BodyPart, RectTransform> partAnchors = new Dictionary<BodyPart, RectTransform>();
    private readonly Dictionary<BodyPart, Image> partImages = new Dictionary<BodyPart, Image>();

    private RectTransform visualRoot;
    private Image bodyImage;
    private Image bellyImage;
    private Image faceImage;
    private Image attackFlashImage;
    private MonsterVisualDescriptor descriptor = MonsterVisualDescriptor.Default();
    private MonsterVisualState currentState = MonsterVisualState.Idle;
    private Vector3 baseVisualScale = Vector3.one;
    private bool posePlaying;
    private bool currentLoop = true;

    public string startingAnimation = "action/idle/normal";

    public MonsterVisualDescriptor Descriptor => descriptor;
    public MonsterVisualState CurrentState => currentState;

    private void Awake()
    {
        EnsureBuilt();
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public static VanillaMonsterGraphic Ensure(GameObject go)
    {
        VanillaMonsterGraphic graphic = go.GetComponent<VanillaMonsterGraphic>();
        if (graphic == null)
            graphic = go.AddComponent<VanillaMonsterGraphic>();

        graphic.EnsureBuilt();
        return graphic;
    }

    public static VanillaMonsterGraphic EnsureCenteredChild(Transform parent, VanillaMonsterGraphic current = null, string childName = DefaultCenteredChildName)
    {
        if (parent == null)
            return current;

        VanillaMonsterGraphic graphic = null;
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            graphic = Ensure(existing.gameObject);
        }
        else if (current != null && current.transform != parent && current.transform.parent == parent && current.name == childName)
        {
            graphic = Ensure(current.gameObject);
        }
        else
        {
            GameObject graphicGo = new GameObject(childName, typeof(RectTransform));
            graphicGo.transform.SetParent(parent, false);
            graphic = Ensure(graphicGo);
        }

        if (current != null && current != graphic)
        {
            current.Clear();
            current.enabled = false;
        }

        graphic.CenterInParent();
        return graphic;
    }

    public void CenterInParent()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = GetAvailableSize(rect);
        UpdateLayoutScale();
    }

    public void SetDescriptor(MonsterVisualDescriptor newDescriptor)
    {
        EnsureBuilt();
        visualRoot.gameObject.SetActive(true);
        descriptor = newDescriptor ?? MonsterVisualDescriptor.Default();
        Color main = MonsterClassPalette.Main(descriptor.Class);
        Color accent = MonsterClassPalette.Accent(descriptor.Class);

        ApplyPartSprite(bodyImage, "body", VanillaSpriteShape.Ellipse, main);
        ApplyPartSprite(bellyImage, "belly", VanillaSpriteShape.Ellipse, accent);
        ApplyPartSprite(faceImage, "face", VanillaSpriteShape.Circle, Color.Lerp(accent, Color.white, 0.2f));
        ApplyPartSprite(attackFlashImage, "attack-flash", VanillaSpriteShape.Diamond, new Color(accent.r, accent.g, accent.b, 0f));

        foreach (Image image in images)
            image.enabled = true;

        ApplyPartImages();

        attackFlashImage.enabled = false;
    }

    public void SetMonster(GetMonstersExample.Monster monster)
    {
        SetDescriptor(MonsterVisualDescriptor.FromMonster(monster));
    }

    public void Clear()
    {
        EnsureBuilt();
        KillTweens();
        descriptor = null;
        visualRoot.gameObject.SetActive(false);
        foreach (Image image in images)
            image.enabled = false;

        if (attackFlashImage != null)
            attackFlashImage.enabled = false;
    }

    public void Initialize(bool resetState)
    {
        EnsureBuilt();
        visualRoot.gameObject.SetActive(true);
        foreach (Image image in images)
            image.enabled = true;

        attackFlashImage.enabled = false;

        if (resetState)
            Play(MonsterVisualStateMapper.FromLegacyName(startingAnimation), true);
    }

    public void Play(MonsterVisualState state, bool loop = true)
    {
        EnsureBuilt();
        if (posePlaying && CanReusePose(state, loop) && currentState == state && currentLoop == loop)
            return;

        currentState = state;
        currentLoop = loop;
        ResetPose();

        float duration = GetDuration(state);
        switch (state)
        {
            case MonsterVisualState.Run:
                AnimateWalk(duration, loop);
                break;
            case MonsterVisualState.AttackMelee:
            case MonsterVisualState.Shrimp:
                AnimateMeleeAttack(duration, loop);
                break;
            case MonsterVisualState.AttackRanged:
                AnimateRangedAttack(duration, loop);
                break;
            case MonsterVisualState.Cast:
                AnimateCastJump(duration, loop);
                break;
            case MonsterVisualState.Hit:
                visualRoot.DOShakeAnchorPos(duration, 8f, 8, 35f, false, true);
                break;
            case MonsterVisualState.Hover:
            case MonsterVisualState.Grabbed:
                visualRoot.DOScale(baseVisualScale * 1.08f, duration * 0.5f).SetLoops(loop ? -1 : 2, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
            case MonsterVisualState.Appear:
                visualRoot.localScale = Vector3.zero;
                visualRoot.DOScale(baseVisualScale, duration).SetEase(Ease.OutBack);
                break;
            case MonsterVisualState.Victory:
                visualRoot.DOAnchorPosY(8f, duration * 0.5f).SetLoops(loop ? -1 : 2, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
            case MonsterVisualState.Hidden:
                foreach (Image image in images)
                    image.enabled = false;
                attackFlashImage.enabled = false;
                break;
            default:
                visualRoot.DOAnchorPosY(4f, duration * 0.5f).SetLoops(loop ? -1 : 2, LoopType.Yoyo).SetEase(Ease.InOutSine);
                break;
        }

        posePlaying = true;
    }

    public RectTransform GetPartAnchor(BodyPart part)
    {
        EnsureBuilt();
        return partAnchors.TryGetValue(part, out RectTransform anchor) ? anchor : visualRoot;
    }

    private float GetDuration(MonsterVisualState state)
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
            case MonsterVisualState.Victory:
                return 0.9f;
            default:
                return 1f;
        }
    }

    private void EnsureBuilt()
    {
        if (visualRoot != null)
            return;

        RectTransform rect = GetComponent<RectTransform>();
        if (rect.sizeDelta == Vector2.zero && rect.rect.size == Vector2.zero)
            rect.sizeDelta = DefaultVisualSize;

        visualRoot = CreateRect("Visual Root", rect, Vector2.zero, DefaultVisualSize);
        bodyImage = CreateImage("Body", VanillaSpriteShape.Ellipse, visualRoot, Vector2.zero, new Vector2(128f, 82f), 0);
        bellyImage = CreateImage("Belly", VanillaSpriteShape.Ellipse, visualRoot, new Vector2(8f, -6f), new Vector2(66f, 42f), 1);
        faceImage = CreateImage("Face", VanillaSpriteShape.Circle, visualRoot, new Vector2(-36f, 10f), new Vector2(30f, 30f), 2);
        attackFlashImage = CreateImage("Attack Flash", VanillaSpriteShape.Diamond, visualRoot, AttackFlashPosition, AttackFlashSize, 8, false);
        attackFlashImage.enabled = false;

        CreatePartAnchor(BodyPart.Eyes, new Vector2(-45f, 16f), VanillaSpriteShape.Circle, new Vector2(9f, 9f));
        CreatePartAnchor(BodyPart.Ears, new Vector2(-28f, 43f), VanillaSpriteShape.Triangle, new Vector2(20f, 20f));
        CreatePartAnchor(BodyPart.Horn, new Vector2(-7f, 46f), VanillaSpriteShape.Triangle, new Vector2(20f, 22f));
        CreatePartAnchor(BodyPart.Mouth, new Vector2(-52f, -2f), VanillaSpriteShape.RoundedBox, new Vector2(16f, 8f));
        CreatePartAnchor(BodyPart.Back, new Vector2(22f, 43f), VanillaSpriteShape.Diamond, new Vector2(22f, 22f));
        CreatePartAnchor(BodyPart.Tail, new Vector2(68f, 4f), VanillaSpriteShape.Triangle, new Vector2(24f, 20f));

        UpdateLayoutScale();
        SetDescriptor(descriptor);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (visualRoot != null)
            UpdateLayoutScale();
    }

    private void UpdateLayoutScale()
    {
        if (visualRoot == null)
            return;

        RectTransform rect = GetComponent<RectTransform>();
        Vector2 availableSize = GetAvailableSize(rect);
        float scale = Mathf.Min(availableSize.x / DefaultVisualSize.x, availableSize.y / DefaultVisualSize.y);
        scale = Mathf.Clamp(scale * 1.15f, 0.55f, 1.8f);
        baseVisualScale = Vector3.one * scale;

        visualRoot.sizeDelta = DefaultVisualSize;
        if (!DOTween.IsTweening(visualRoot))
            visualRoot.localScale = baseVisualScale;
    }

    private static Vector2 GetAvailableSize(RectTransform rect)
    {
        Vector2 size = rect.rect.size;
        if (size.x <= 1f || size.y <= 1f)
            size = rect.sizeDelta;

        if ((size.x <= 1f || size.y <= 1f) && rect.parent is RectTransform parentRect)
            size = parentRect.rect.size;

        if (size.x <= 1f || size.y <= 1f)
            size = DefaultVisualSize;

        return new Vector2(Mathf.Max(48f, size.x), Mathf.Max(48f, size.y));
    }

    private RectTransform CreateRect(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private Image CreateImage(string name, VanillaSpriteShape shape, RectTransform parent, Vector2 anchoredPosition, Vector2 size, int siblingIndex, bool tracked = true)
    {
        RectTransform rect = CreateRect(name, parent, anchoredPosition, size);
        rect.SetSiblingIndex(siblingIndex);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = VanillaSpriteCache.Get(shape);
        image.raycastTarget = false;
        if (tracked)
            images.Add(image);
        return image;
    }

    private void CreatePartAnchor(BodyPart part, Vector2 anchoredPosition, VanillaSpriteShape shape, Vector2 size)
    {
        RectTransform anchor = CreateRect(part + " Anchor", visualRoot, anchoredPosition, Vector2.one);
        partAnchors[part] = anchor;
        Image marker = CreateImage(part + " Marker", shape, visualRoot, anchoredPosition, size, 3);
        partImages[part] = marker;
        marker.color = MonsterClassPalette.BodyPartColor(part);
    }

    private void ApplyPartSprite(Image image, string partKey, VanillaSpriteShape fallbackShape, Color fallbackColor)
    {
        if (image == null)
            return;

        if (VanillaMonsterSpriteLibrary.TryGetPartSprite(descriptor.Class, partKey, out Sprite sprite))
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            return;
        }

        image.sprite = VanillaSpriteCache.Get(fallbackShape);
        image.color = fallbackColor;
        image.preserveAspect = true;
    }

    private void ApplyPartImages()
    {
        foreach (KeyValuePair<BodyPart, Image> partImage in partImages)
        {
            if (VanillaMonsterSpriteLibrary.TryGetBodyPartSprite(descriptor.Class, partImage.Key, out Sprite sprite))
            {
                partImage.Value.sprite = sprite;
                partImage.Value.color = Color.white;
            }
            else
            {
                partImage.Value.sprite = VanillaSpriteCache.Get(VanillaMonsterIconUtility.GetBodyPartShape(partImage.Key));
                partImage.Value.color = MonsterClassPalette.BodyPartColor(partImage.Key);
            }

            partImage.Value.preserveAspect = true;
        }
    }

    private void ResetPose()
    {
        KillTweens();
        UpdateLayoutScale();

        foreach (Image image in images)
            image.enabled = true;

        visualRoot.localScale = baseVisualScale;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.anchoredPosition = Vector2.zero;

        RectTransform flashRect = (RectTransform)attackFlashImage.transform;
        flashRect.anchoredPosition = AttackFlashPosition;
        flashRect.localScale = Vector3.one;
        flashRect.localRotation = Quaternion.identity;
        attackFlashImage.enabled = false;
    }

    private void KillTweens()
    {
        visualRoot?.DOKill();
        attackFlashImage?.DOKill();
        if (attackFlashImage != null)
            attackFlashImage.transform.DOKill();

        posePlaying = false;
    }

    private void AnimateWalk(float duration, bool shouldLoop)
    {
        float halfStep = duration * 0.5f;
        visualRoot.DOAnchorPosY(7f, halfStep)
            .SetLoops(shouldLoop ? -1 : 2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        visualRoot.DORotate(new Vector3(0f, 0f, 4f), halfStep)
            .SetLoops(shouldLoop ? -1 : 2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void AnimateMeleeAttack(float duration, bool shouldLoop)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOAnchorPosX(8f, duration * 0.16f).SetEase(Ease.OutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, -10f), duration * 0.16f).SetEase(Ease.OutSine));
        sequence.Append(visualRoot.DOAnchorPosX(-22f, duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, 18f), duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.AppendCallback(delegate { PlayAttackFlash(duration, false); });
        sequence.Append(visualRoot.DOAnchorPosX(4f, duration * 0.18f).SetEase(Ease.OutBack));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, -6f), duration * 0.18f).SetEase(Ease.OutBack));
        sequence.Append(visualRoot.DOAnchorPos(Vector2.zero, duration * 0.2f).SetEase(Ease.InOutSine));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.2f).SetEase(Ease.InOutSine));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.18f);
            sequence.SetLoops(-1);
        }
    }

    private void AnimateRangedAttack(float duration, bool shouldLoop)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOAnchorPosY(11f, duration * 0.2f).SetEase(Ease.OutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, -8f), duration * 0.2f).SetEase(Ease.OutSine));
        sequence.AppendCallback(delegate { PlayAttackFlash(duration, true); });
        sequence.Append(visualRoot.DOAnchorPos(new Vector2(5f, -3f), duration * 0.18f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, 12f), duration * 0.18f).SetEase(Ease.OutQuad));
        sequence.Append(visualRoot.DOAnchorPos(Vector2.zero, duration * 0.28f).SetEase(Ease.OutBack));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.28f).SetEase(Ease.OutBack));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.2f);
            sequence.SetLoops(-1);
        }
    }

    private void AnimateCastJump(float duration, bool shouldLoop)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(visualRoot.DOAnchorPos(new Vector2(3f, 30f), duration * 0.28f).SetEase(Ease.OutQuad));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, -8f), duration * 0.28f).SetEase(Ease.OutSine));
        sequence.AppendCallback(delegate { PlayAttackFlash(duration, true); });
        sequence.Append(visualRoot.DOAnchorPos(new Vector2(-5f, 14f), duration * 0.16f).SetEase(Ease.InOutSine));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, 13f), duration * 0.16f).SetEase(Ease.OutQuad));
        sequence.Append(visualRoot.DOAnchorPos(Vector2.zero, duration * 0.28f).SetEase(Ease.InQuad));
        sequence.Join(visualRoot.DORotate(Vector3.zero, duration * 0.28f).SetEase(Ease.InOutSine));
        if (shouldLoop)
        {
            sequence.AppendInterval(duration * 0.18f);
            sequence.SetLoops(-1);
        }
    }

    private void PlayAttackFlash(float duration, bool ranged)
    {
        RectTransform flashRect = (RectTransform)attackFlashImage.transform;
        flashRect.DOKill();
        attackFlashImage.DOKill();

        MonsterVisualDescriptor activeDescriptor = descriptor ?? MonsterVisualDescriptor.Default();
        Color color = MonsterClassPalette.Accent(activeDescriptor.Class);
        attackFlashImage.color = new Color(color.r, color.g, color.b, 0.85f);
        attackFlashImage.enabled = true;
        flashRect.anchoredPosition = ranged ? new Vector2(AttackFlashPosition.x, AttackFlashPosition.y + 10f) : AttackFlashPosition;
        flashRect.localScale = Vector3.zero;
        flashRect.localRotation = Quaternion.Euler(0f, 0f, ranged ? 45f : -20f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(flashRect.DOScale(ranged ? Vector3.one * 1.35f : Vector3.one, duration * 0.12f).SetEase(Ease.OutBack));
        sequence.Join(flashRect.DOAnchorPosX(-16f, duration * 0.18f).SetRelative().SetEase(Ease.OutQuad));
        sequence.Append(attackFlashImage.DOFade(0f, duration * 0.18f).SetEase(Ease.InQuad));
        sequence.OnComplete(delegate { attackFlashImage.enabled = false; });
    }

    private static bool CanReusePose(MonsterVisualState state, bool loop)
    {
        return loop || state == MonsterVisualState.Hidden;
    }
}
