using Lidgren.Network;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public class EntityPacket : MP_PacketBase<Entity>
{
    protected override object Read(NetIncomingMessage msg)
    {
        return new Entity() { Id = new Guid(msg.ReadString()), Name = msg.ReadString() };
    }

    protected override void Write(Entity entity, NetOutgoingMessage msg)
    {
        msg.Write(entity.Id.ToString());
        msg.Write(entity.Name);
    }
}
