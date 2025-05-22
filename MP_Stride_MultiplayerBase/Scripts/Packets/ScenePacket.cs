using Lidgren.Network;

namespace MP_Stride_MultiplayerBase;
public class ScenePacket : MP_PacketBase<Scene>
{
    protected override void Write(Scene data, NetOutgoingMessage msg)
    {
        msg.Write(data.Id.ToString());
        msg.Write(data.Name);
    }
    protected override object Read(NetIncomingMessage msg)
    {
        return new Scene
        {
            Id = Guid.Parse(msg.ReadString()),
            Name = msg.ReadString()
        };
    }
}