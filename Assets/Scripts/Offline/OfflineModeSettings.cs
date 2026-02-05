using UnityEngine;

[CreateAssetMenu(menuName = "Offline/Offline Mode Settings", fileName = "OfflineModeSettings")]
public class OfflineModeSettings : ScriptableObject
{
    public bool useOfflineMode = true;

    public string offlineUsername = "Offline Player";
    public string offlineWalletAddress = "offline";

    public OfflineAxieDatabase axieDatabase;
}

