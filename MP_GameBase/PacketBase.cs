using Lidgren.Network;
using Stride.Core.Serialization.Contents;

namespace MP_GameBase;


public enum PacketType
{
    EndPacket,
    Player,
    Scene,
    Entity,
    Transform
}
public interface IMP_Packet
{
    public static PacketType packetType { get; }
}
public abstract class MP_PacketBase : IMP_Packet
{
    public abstract PacketType packetType { get; }
    abstract internal object ReadPacket(NetIncomingMessage msg);
    abstract internal NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg);
}