using Lidgren.Network;

namespace MP_GameBase
{
    public static class NetConnectionConfig
    {
        public static NetPeerConfiguration GetDefaultConfig()
        {
            return new NetPeerConfiguration("MP_GameStride")
            {
                LocalAddress = System.Net.IPAddress.Loopback,//new([127,0,0,1]),
                Port = 4420
            };
        }
        public static NetPeerConfiguration GetDefaultClientConfig()
        {
            NetPeerConfiguration config = GetDefaultConfig();
            config.Port++;
            return config;
        }
    }
}
