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
        string modelName = msg.ReadString();
        var Cube = MP_PacketContainer.Content.Load<Prefab>(modelName);

        Model model = MP_PacketContainer.Content.Load<Model>("CubePrefab");
        entity.Add(new ModelComponent());
        var nestedPackets = MP_PacketContainer.ReceiveNestedPackets(msg);
        foreach (var packet in nestedPackets)
        {

            foreach (var packetData in packet.Value)
            {
                switch (packet.Key)
                {
                    case PacketType.Player:
                        break;
                    case PacketType.Scene:
                        break;
                    case PacketType.Transform:
                        TransformComponent newTransform = (TransformComponent)packetData;
                        entity.Transform.Position = newTransform.Position;
                        entity.Transform.Rotation = newTransform.Rotation;

                        break;
                    default:
                        break;
                }
            }
        }
        //TransformComponent transformComponent = (TransformComponent)MP_PacketContainer.packets[PacketType.Transform].ReadPacket(msg);
        //entity.Transform.Position = transformComponent.Position;
        //entity.Transform.Rotation = transformComponent.Rotation;
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
        msg.Write("CubePrefab");
        TransformPacket.WritePacket(entity.Transform, msg);
        msg.Write((uint)PacketType.EndPacket);

        return msg;
    }
}