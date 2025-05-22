using Lidgren.Network;

namespace MP_Stride_MultiplayerBase
{
    class TransformPacket : MP_PacketBase<TransformComponent>
    {
        protected override object Read(NetIncomingMessage msg)
        {
            return new TransformComponent()
            {
                Position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
                Rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat())
            };
        }
        protected override void Write(TransformComponent transform, NetOutgoingMessage msg)
        {
            msg.WriteVariableInt32(PacketId);
            msg.Write(transform.Position.X);
            msg.Write(transform.Position.Y);
            msg.Write(transform.Position.Z);
            msg.Write(transform.Rotation.X);
            msg.Write(transform.Rotation.Y);
            msg.Write(transform.Rotation.Z);
            msg.Write(transform.Rotation.W);
        }
    }
}