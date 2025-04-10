using Lidgren.Network;

namespace MP_GameBase
{
    class TransformPacket : MP_PacketBase
    {
        public override PacketType packetType => PacketType.Transform;

        public static NetOutgoingMessage SendPacket(TransformComponent scene, NetOutgoingMessage msg)
        {
            MP_PacketContainer.packets[PacketType.Transform].WritePacket(scene, msg);
            return msg;
        }
        public static NetOutgoingMessage WritePacket(TransformComponent dataToSend, NetOutgoingMessage msg)
        {
            return MP_PacketContainer.packets[PacketType.Transform].WritePacket(dataToSend, msg);
        }
        internal override TransformComponent ReadPacket(NetIncomingMessage msg)
        {
            return new TransformComponent()
            {
                Position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()),
                Rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat())
            };
        }

        internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
        {
            if (dataToSend is TransformComponent transform)
            {
                msg.Write((uint)PacketType.Transform);
                msg.Write(transform.Position.X);
                msg.Write(transform.Position.Y);
                msg.Write(transform.Position.Z);
                msg.Write(transform.Rotation.X);
                msg.Write(transform.Rotation.Y);
                msg.Write(transform.Rotation.Z);
                msg.Write(transform.Rotation.W);
            }
            return msg;
        }
    }
}
