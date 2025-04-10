using Lidgren.Network;
using Stride.Rendering;

namespace MP_GameBase;

class EntityPacket : MP_PacketBase
{
    public override PacketType packetType => PacketType.Entity;

    public static NetOutgoingMessage SendPacket(Entity entity, NetOutgoingMessage msg)
    {
        MP_PacketContainer.packets[PacketType.Entity].WritePacket(entity, msg);
        return msg;
    }

    internal override Entity ReadPacket(NetIncomingMessage msg)
    {
        Entity entity = new Entity() { Id = new Guid(msg.ReadString()), Name = msg.ReadString() };
        entity.Add(new ModelComponent(MP_PacketContainer.Content.Load<Model>(msg.ReadString())));
        TransformComponent transformComponent = (TransformComponent)MP_PacketContainer.packets[PacketType.Transform].ReadPacket(msg);
        entity.Transform.Position = transformComponent.Position;
        entity.Transform.Rotation = transformComponent.Rotation;
        return entity;
    }

    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
    {
        if (dataToSend is not Entity entity)
        {
            throw new InvalidDataException();
        }
        msg.Write((uint)PacketType.Entity);
        msg.Write(entity.Id.ToString());
        msg.Write(entity.Name);
        msg.Write("Cube");
        TransformPacket.WritePacket(entity.Transform, msg);
        return msg;
    }
}