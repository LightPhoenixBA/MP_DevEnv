using Lidgren.Network;

namespace MP_GameBase
{
    public static class NetConnectionConfig
    {
       public static NetPeerConfiguration GetDefaultPeerConfig()
        {
            return new NetPeerConfiguration("MP_GameStride")
            {
                LocalAddress = new([127,0,0,1]),
                Port = 4420,
            };
        }
    }
}
