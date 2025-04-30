using Lidgren.Network;
using MP_Stride_MultiplayerBase;
using Stride.Engine;

namespace MP_Stride_MultiplayerBase
{
    public class PrefabPacket : MP_PacketBase<Tuple<string, Prefab?>>
    {
        protected override object Read(NetIncomingMessage msg)
        {
            string name = msg.ReadString();
            return Tuple.Create(name, Content.Load<Prefab>(name));
        }

        public static void SendUntypedPacket(string PrefabName, NetOutgoingMessage msg)
        {
            registry[PacketId].SendPacket(Tuple.Create<string, Prefab?>(PrefabName, null), msg);
        }

        protected override void Write(Tuple<string, Prefab?> data, NetOutgoingMessage msg)
        {
            msg.Write(data.Item1);
        }
    }
}
