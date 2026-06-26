using UnityEngine;

public static class MonsterScale
{
    public const float WorldVisual = 0.55f;
    public const float WorldHud = 0.48f;
    public const float WorldStatus = 0.30f;
    public const float World = WorldVisual;

    private const float MinMagnitude = 0.001f;
    private static readonly Vector3 GrabColliderSize = new Vector3(1.05f, 0.78f, 0.8f);
    private static readonly Vector3 GrabColliderCenter = new Vector3(0f, 0.25f, 0f);

    public static Vector3 WorldVector => Vector3.one * WorldVisual;

    public static BoxCollider ApplyGrabCollider(GameObject target)
    {
        if (target == null)
            return null;

        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null)
            collider = target.AddComponent<BoxCollider>();

        collider.isTrigger = true;
        collider.size = GrabColliderSize;
        collider.center = GrabColliderCenter;
        return collider;
    }

    public static Vector3 WorldFacing(bool positiveX)
    {
        return new Vector3(positiveX ? WorldVisual : -WorldVisual, WorldVisual, WorldVisual);
    }

    public static void SetFacing(Transform target, bool positiveX)
    {
        if (target == null)
            return;

        VanillaMonsterVisual visual = target.GetComponent<VanillaMonsterVisual>();
        if (visual == null)
            visual = target.GetComponentInChildren<VanillaMonsterVisual>();

        if (visual != null)
        {
            target.localScale = Vector3.one;
            visual.SetFacing(positiveX);
            return;
        }

        Vector3 scale = target.localScale;
        float magnitude = Mathf.Abs(scale.x);
        if (magnitude < MinMagnitude)
            magnitude = Mathf.Abs(scale.y);
        if (magnitude < MinMagnitude)
            magnitude = World;

        target.localScale = new Vector3(positiveX ? magnitude : -magnitude, scale.y, scale.z);
    }

    public static bool IsFacingPositive(Transform target)
    {
        if (target == null)
            return true;

        VanillaMonsterVisual visual = target.GetComponent<VanillaMonsterVisual>();
        if (visual == null)
            visual = target.GetComponentInChildren<VanillaMonsterVisual>();

        if (visual != null)
            return visual.FacingPositiveX;

        return target.localScale.x >= 0f;
    }

    public static void ApplyReadableWorldOverlay(Transform target, float scale)
    {
        ApplyReadableWorldOverlay(target, scale, target != null ? target.rotation : Quaternion.identity);
    }

    public static void ApplyReadableWorldOverlay(Transform target, float scale, Quaternion worldRotation)
    {
        if (target == null)
            return;

        target.rotation = worldRotation;
        if (target.parent == null)
        {
            target.localScale = Vector3.one * scale;
            return;
        }

        Vector3 parentScale = target.parent.lossyScale;
        target.localScale = new Vector3(
            DivideScale(scale, parentScale.x),
            DivideScale(scale, parentScale.y),
            DivideScale(scale, parentScale.z));
    }

    private static float DivideScale(float scale, float parentScale)
    {
        return Mathf.Abs(parentScale) < MinMagnitude ? scale : scale / parentScale;
    }
}

public static class MonsterVfxLimiter
{
    private const float BattleScale = 0.18f;
    private const float PreviewScale = 0.12f;
    private const int MaxParticlesPerSystem = 80;
    private const float MinDestroyDelay = 0.45f;
    private const float MaxDestroyDelay = 2.2f;

    public static void NormalizeSpawned(GameObject vfx, bool preview = false, float suggestedLifetime = 0f)
    {
        if (vfx == null)
            return;

        float scale = preview ? PreviewScale : BattleScale;
        ApplyRootScale(vfx.transform, scale);
        LimitParticles(vfx);

        if (suggestedLifetime >= 0f)
        {
            float lifetime = Mathf.Clamp(suggestedLifetime > 0f ? suggestedLifetime + 0.45f : MaxDestroyDelay,
                MinDestroyDelay, MaxDestroyDelay);
            Object.Destroy(vfx, lifetime);
        }
    }

    private static void ApplyRootScale(Transform transform, float multiplier)
    {
        Vector3 localScale = transform.localScale;
        float sign = localScale.x < 0f ? -1f : 1f;
        transform.localScale = new Vector3(
            sign * Mathf.Abs(localScale.x) * multiplier,
            localScale.y * multiplier,
            localScale.z * multiplier);
    }

    private static void LimitParticles(GameObject vfx)
    {
        ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = Mathf.Min(main.maxParticles, MaxParticlesPerSystem);

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            if (emission.enabled)
                ClampBursts(emission);
        }
    }

    private static void ClampBursts(ParticleSystem.EmissionModule emission)
    {
        int burstCount = emission.burstCount;
        if (burstCount == 0)
            return;

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[burstCount];
        emission.GetBursts(bursts);
        for (int i = 0; i < bursts.Length; i++)
            bursts[i].count = ClampCurve(bursts[i].count, MaxParticlesPerSystem);

        emission.SetBursts(bursts);
    }

    private static ParticleSystem.MinMaxCurve ClampCurve(ParticleSystem.MinMaxCurve curve, float max)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                curve.constant = Mathf.Min(curve.constant, max);
                break;
            case ParticleSystemCurveMode.TwoConstants:
                curve.constantMin = Mathf.Min(curve.constantMin, max);
                curve.constantMax = Mathf.Min(curve.constantMax, max);
                break;
        }

        return curve;
    }
}
