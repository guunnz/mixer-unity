using System;

public static class AxieIdUtil
{
    /// <summary>
    /// Convert any axieId string into a stable int.
    /// Uses int.TryParse when possible, otherwise falls back to a stable hash.
    /// </summary>
    public static int ToStableInt(string axieId)
    {
        if (string.IsNullOrEmpty(axieId))
            return 1;

        if (int.TryParse(axieId, out int parsed))
            return parsed;

        // FNV-1a 32-bit, matching LandSeedUtil approach.
        unchecked
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;
            uint hash = offsetBasis;
            for (int i = 0; i < axieId.Length; i++)
            {
                hash ^= axieId[i];
                hash *= prime;
            }

            int seed = (int)(hash & 0x7fffffff);
            return seed == 0 ? 1 : seed;
        }
    }
}

