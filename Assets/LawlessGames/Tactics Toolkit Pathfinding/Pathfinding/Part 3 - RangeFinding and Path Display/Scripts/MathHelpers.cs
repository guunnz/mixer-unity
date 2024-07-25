public static class MathHelpers
{
    static public float InvLerp(float a, float b, float v)
    {
        return (v - a) / (b - a);
    }
}