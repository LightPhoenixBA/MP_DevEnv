using Lidgren.Network;

namespace MP_GameBase;

public enum PacketType
{
    Default,
    Player,
    Scene,
    Entity
}
public interface IPacket
{
    static PacketType packetType;
    //void PacketSend(NetOutgoingMessage netOutgoingMessage);
    //void PacketReceive(NetIncomingMessage netIncomingMessage);
    //public T PacketReceive<T>(NetIncomingMessage netIncomingMessage, IPacket dataType);
}
public abstract class Packet : IPacket
{
    public static PacketType packetType = PacketType.Default;
    //public abstract void PacketSend(NetOutgoingMessage netOutgoingMessage);
    //public abstract void PacketReceive(NetIncomingMessage netIncomingMessage);
    //public abstract T PacketReceive<T>(NetIncomingMessage netIncomingMessage, IPacket dataType);


}
