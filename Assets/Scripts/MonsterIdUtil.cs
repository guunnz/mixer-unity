using System;

public static class MonsterIdUtil
{
    /// <summary>
    /// Convert any monsterId string into a stable int.
    /// Uses int.TryParse when possible, otherwise falls back to a stable hash.
    /// </summary>
    public static int ToStableInt(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
            return 1;

        if (int.TryParse(monsterId, out int parsed))
            return parsed;

        // FNV-1a 32-bit, matching LandSeedUtil approach.
        unchecked
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;
            uint hash = offsetBasis;
            for (int i = 0; i < monsterId.Length; i++)
            {
                hash ^= monsterId[i];
                hash *= prime;
            }

            int seed = (int)(hash & 0x7fffffff);
            return seed == 0 ? 1 : seed;
        }
    }
}

