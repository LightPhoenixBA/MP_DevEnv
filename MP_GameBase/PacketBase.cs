using Lidgren.Network;
using System.Net.Sockets;

namespace MP_GameBase;

public enum PacketType
{
    Default,
    Player,
    Scene,
    Entity,
    Transform
}
public interface IMP_Packet
{
    public static PacketType packetType { get; }
}
public static class MP_PacketContainer
{
    public static readonly Dictionary<PacketType, MP_PacketBase> packets = new Dictionary<PacketType, MP_PacketBase>()
       {
        { PacketType.Default,null },
        { PacketType.Scene,new ScenePacket() },
        { PacketType.Entity,new EntityPacket() },
        { PacketType.Transform,new TransformPacket() },
    };
    public static NetOutgoingMessage SendPacket(PacketType packetType, object dataToSend, NetOutgoingMessage msg)
    {
        return packets[packetType].WritePacket(dataToSend, msg);
    }
    public static Tuple<PacketType, object> ReceivePacket(NetIncomingMessage msg)
    {
        PacketType newPacketType = (PacketType)msg.ReadUInt32();
        return new Tuple<PacketType, object>( newPacketType,packets[newPacketType].ReadPacket(msg));
    }
}
public abstract class MP_PacketBase : IMP_Packet
{
    public abstract PacketType packetType { get; }
    abstract internal object ReadPacket(NetIncomingMessage msg);
    abstract internal NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg);
}