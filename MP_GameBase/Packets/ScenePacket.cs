using Lidgren.Network;

namespace MP_GameBase;
public class ScenePacket : MP_PacketBase<Scene>
{
    internal override void Write(Scene data, NetOutgoingMessage msg)
    {
        msg.Write(data.Id.ToString());
        msg.Write(data.Name);
    }
    internal override object Read(NetIncomingMessage msg)
    {
        return new Scene
        {
            Id = Guid.Parse(msg.ReadString()),
            Name = msg.ReadString()
        };
    }
}