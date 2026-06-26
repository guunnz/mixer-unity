using System.Collections.Generic;
using UnityEngine;

public enum VanillaSpriteShape
{
    Circle,
    Ellipse,
    Triangle,
    RoundedBox,
    Diamond
}

public static class VanillaSpriteCache
{
    private static readonly Dictionary<VanillaSpriteShape, Sprite> Sprites = new Dictionary<VanillaSpriteShape, Sprite>();

    public static Sprite Get(VanillaSpriteShape shape)
    {
        if (Sprites.TryGetValue(shape, out Sprite sprite) && sprite != null)
            return sprite;

        sprite = Build(shape);
        Sprites[shape] = sprite;
        return sprite;
    }

    private static Sprite Build(VanillaSpriteShape shape)
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / size, (y + 0.5f) / size);
                texture.SetPixel(x, y, IsInside(shape, uv) ? white : clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static bool IsInside(VanillaSpriteShape shape, Vector2 uv)
    {
        Vector2 p = uv - new Vector2(0.5f, 0.5f);
        switch (shape)
        {
            case VanillaSpriteShape.Circle:
                return p.sqrMagnitude <= 0.24f;
            case VanillaSpriteShape.Ellipse:
                return (p.x * p.x) / 0.24f + (p.y * p.y) / 0.13f <= 1f;
            case VanillaSpriteShape.Triangle:
                return uv.y >= 0.16f && uv.y <= 0.86f && Mathf.Abs(p.x) <= (uv.y - 0.16f) * 0.48f;
            case VanillaSpriteShape.RoundedBox:
                return Mathf.Abs(p.x) <= 0.42f && Mathf.Abs(p.y) <= 0.34f;
            case VanillaSpriteShape.Diamond:
                return Mathf.Abs(p.x) + Mathf.Abs(p.y) <= 0.48f;
            default:
                return true;
        }
    }
}
