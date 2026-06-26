using DG.Tweening;
using UnityEngine;

public static class VanillaSkillVfx
{
    public static void Play(Transform parent, Vector3 origin, Vector3 target, MonsterClass monsterClass,
        MonsterVisualState state, float duration, bool small = false)
    {
        duration = Mathf.Max(0.15f, duration);
        Color main = MonsterClassPalette.Main(monsterClass);
        Color accent = MonsterClassPalette.Accent(monsterClass);
        float scale = small ? 0.35f : 1f;

        GameObject root = new GameObject("Vanilla Skill VFX");
        root.transform.position = origin;
        if (parent != null)
            root.transform.SetParent(parent, true);

        SpriteRenderer castPulse = CreateRenderer("Cast Pulse", root.transform, VanillaSpriteShape.Circle,
            origin, new Vector3(0.25f, 0.25f, 1f) * scale, main, 60);
        FadeScale(castPulse, Vector3.one * 0.75f * scale, duration * 0.6f);

        bool projectile = state == MonsterVisualState.AttackRanged || state == MonsterVisualState.Cast;
        if (projectile)
        {
            SpriteRenderer bolt = CreateRenderer("Projectile", root.transform, VanillaSpriteShape.Diamond,
                origin, new Vector3(0.22f, 0.22f, 1f) * scale, accent, 62);
            bolt.transform.DOMove(target, duration * 0.7f).SetEase(Ease.OutQuad);
            FadeScale(bolt, new Vector3(0.28f, 0.28f, 1f) * scale, duration * 0.75f, 0.15f);
        }
        else
        {
            Vector3 midPoint = Vector3.Lerp(origin, target, 0.58f);
            SpriteRenderer slash = CreateRenderer("Melee Flash", root.transform, VanillaSpriteShape.RoundedBox,
                midPoint, new Vector3(0.12f, 0.62f, 1f) * scale, accent, 62);
            slash.transform.Rotate(0f, 0f, -35f);
            FadeScale(slash, new Vector3(0.2f, 0.95f, 1f) * scale, duration * 0.45f);
        }

        SpriteRenderer impact = CreateRenderer("Impact", root.transform, VanillaSpriteShape.Circle,
            target, new Vector3(0.18f, 0.18f, 1f) * scale, main, 64);
        impact.transform.DOScale(new Vector3(0.9f, 0.9f, 1f) * scale, duration * 0.35f)
            .SetDelay(projectile ? duration * 0.45f : duration * 0.1f)
            .SetEase(Ease.OutQuad);
        impact.DOFade(0f, duration * 0.35f)
            .SetDelay(projectile ? duration * 0.5f : duration * 0.15f);

        Object.Destroy(root, duration + 0.35f);
    }

    private static SpriteRenderer CreateRenderer(string name, Transform parent, VanillaSpriteShape shape,
        Vector3 position, Vector3 scale, Color color, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = VanillaSpriteCache.Get(shape);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static void FadeScale(SpriteRenderer renderer, Vector3 scale, float duration, float fadeDelay = 0f)
    {
        renderer.transform.DOScale(scale, duration).SetEase(Ease.OutQuad);
        renderer.DOFade(0f, duration * 0.7f).SetDelay(fadeDelay);
    }
}
