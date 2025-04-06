using Lidgren.Network;

namespace MP_GameBase;

class EntityPacket : MP_PacketBase
{
    public override PacketType packetType => PacketType.Entity;

    public static NetOutgoingMessage SendPacket(Entity scene, NetOutgoingMessage msg)
    {
        MP_PacketContainer.packets[PacketType.Entity].WritePacket(scene, msg);
        return msg;
    }

    internal override Entity ReadPacket(NetIncomingMessage msg)
    {
      Entity entity = new Entity() {Id = new Guid( msg.ReadString()),Name = msg.ReadString() };
        TransformComponent transformComponent = (TransformComponent)MP_PacketContainer.packets[PacketType.Transform].ReadPacket( msg);
        entity.Transform.Position = transformComponent.Position;
        entity.Transform.Rotation = transformComponent.Rotation;
        return entity;
    }

    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
    {
        if (dataToSend is Entity entity)
        {
            msg.Write((int)packetType);
            msg.Write(entity.Id.ToString());
            msg.Write(entity.Name);
            MP_PacketContainer.packets[PacketType.Transform].WritePacket(entity.Transform,msg);
        }
        return msg;
    }
}