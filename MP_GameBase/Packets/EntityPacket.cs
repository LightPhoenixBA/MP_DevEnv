using Lidgren.Network;
using Stride.Engine;
using Stride.Rendering;
using System.Net.Sockets;

namespace MP_GameBase;

class EntityPacket : MP_PacketBase<Entity>
{
    //public static readonly int PacketId = MP_PacketBase.Register(new EntityPacket ());
    //public override int PacketId => PacketId;
    //internal override object Read(NetIncomingMessage msg)
    //{
    //  return new Entity() { Id = new Guid(msg.ReadString()), Name = msg.ReadString() };
    //}

    //internal override void Write(Entity entity, NetOutgoingMessage msg)
    //{
    //    msg.WriteVariableInt32(PacketId);
    //    msg.Write(entity.Id.ToString());
    //    msg.Write(entity.Name);
    //  //  msg.Write("CubePrefab");
    //  //  TransformPacket.WritePacket(entity.Transform, msg);
    //  //  msg.Write((uint)PacketType.EndPacket);
    //}
    internal override object Read(NetIncomingMessage msg)
    {
        throw new NotImplementedException();
    }

    internal override void Write(Entity data, NetOutgoingMessage msg)
    {
        throw new NotImplementedException();
    }
}