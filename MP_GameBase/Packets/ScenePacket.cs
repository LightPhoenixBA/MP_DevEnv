using Lidgren.Network;

namespace MP_GameBase;

public class ScenePacket : MP_PacketBase
{
    public override PacketType packetType => PacketType.Scene;
    public static NetOutgoingMessage SendPacket(Scene scene, NetOutgoingMessage msg)
    {
        MP_PacketContainer.packets[PacketType.Scene].WritePacket(scene, msg);
        return msg;
    }
    internal override object ReadPacket(NetIncomingMessage msg)
    {
        return new Scene
        {
            Id = Guid.Parse(msg.ReadString()),
            Name = msg.ReadString()
        };
    }
    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
    {
        if (dataToSend is Scene scene)
        {
            msg.Write((uint)packetType);
            msg.Write(scene.Id.ToString());
            msg.Write(scene.Name);
            foreach (var entity in scene.Entities)
            {
                EntityPacket.SendPacket(entity,msg);
            }
        }
        return msg;
    }
}