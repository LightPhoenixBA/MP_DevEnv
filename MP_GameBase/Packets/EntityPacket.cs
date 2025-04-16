using Lidgren.Network;

namespace MP_GameBase;

class EntityPacket : MP_PacketBase<Entity>
{
    protected override object Read(NetIncomingMessage msg)
    {
        return new Entity() { Id = new Guid(msg.ReadString()), Name = msg.ReadString() };
    }

    protected override void Write(Entity entity, NetOutgoingMessage msg)
    {
        msg.WriteVariableInt32(PacketId);
        msg.Write(entity.Id.ToString());
        msg.Write(entity.Name);
    }
}
