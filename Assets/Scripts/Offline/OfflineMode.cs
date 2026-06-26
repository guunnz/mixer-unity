using UnityEngine;

public static class OfflineMode
{
    private const string SettingsResourceName = "OfflineModeSettings";
    private static OfflineModeSettings cached;

    public static OfflineModeSettings Settings
    {
        get
        {
            if (cached == null)
            {
                cached = Resources.Load<OfflineModeSettings>(SettingsResourceName);
            }

            return cached;
        }
    }

    public static bool Enabled => Settings != null && Settings.useOfflineMode;
}

