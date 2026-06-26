using System;

public static class LandSeedUtil
{
    /// <summary>
    /// Produce a stable (deterministic) positive int seed from any tokenId string.
    /// Avoids exceptions from int.Parse and works for non-numeric IDs (offline).
    /// </summary>
    public static int SeedFromTokenId(string tokenId)
    {
        if (string.IsNullOrEmpty(tokenId))
            return 1;

        // FNV-1a 32-bit
        unchecked
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;
            uint hash = offsetBasis;
            for (int i = 0; i < tokenId.Length; i++)
            {
                hash ^= tokenId[i];
                hash *= prime;
            }

            // Ensure positive non-zero seed.
            int seed = (int)(hash & 0x7fffffff);
            return seed == 0 ? 1 : seed;
        }
    }
}

