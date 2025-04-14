using Lidgren.Network;
using System.Net.Sockets;

namespace MP_GameBase
{
    class TransformPacket : MP_PacketBase<TransformComponent>
    {
        //public static readonly int PacketId = MP_PacketBase.Register(new TransformPacket());
        //public override int PacketId => PacketId;
        //internal override object Read(NetIncomingMessage msg)
        //{
        //    return new TransformComponent()
        //    {
        //        Position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
        //        Rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat())
        //    };
        //}

        //internal override void Write(TransformComponent transform, NetOutgoingMessage msg)
        //{
        //    msg.WriteVariableInt32(PacketId);
        //    msg.Write(transform.Position.X);
        //    msg.Write(transform.Position.Y);
        //    msg.Write(transform.Position.Z);
        //    msg.Write(transform.Rotation.X);
        //    msg.Write(transform.Rotation.Y);
        //    msg.Write(transform.Rotation.Z);
        //    msg.Write(transform.Rotation.W);
        //}
        internal override object Read(NetIncomingMessage msg)
        {
            throw new NotImplementedException();
        }

        internal override void Write(TransformComponent data, NetOutgoingMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}