using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct SessionGamemodeSettings : INetworkStruct
    {
        public NetworkModObjectGUIDReference gamemodeReference;
    }
}
