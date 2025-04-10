using Lidgren.Network;
namespace MP_GameBase;

public class ScenePacket : MP_PacketBase
{
    public override PacketType packetType => PacketType.Scene;
    public static NetOutgoingMessage SendPacket(Scene scene, NetOutgoingMessage msg)
    {
        MP_PacketContainer.packets[PacketType.Scene].WritePacket(scene, msg);
        return msg;
    }
    internal override Scene ReadPacket(NetIncomingMessage msg)
    {
        Scene newScene = new Scene
        {
            Id = Guid.Parse(msg.ReadString()),
            Name = msg.ReadString()
        };
        var nestedObjs = MP_PacketContainer.ReceiveNestedPackets(msg);
        foreach (var packetType in nestedObjs)
        {
            foreach (var nestedPacket in packetType.Value)
            {
                switch (packetType.Key)
                {
                    case PacketType.Scene:
                        newScene.Children.Add(nestedPacket as Scene);
                        break;
                    case PacketType.Entity:
                        newScene.Entities.Add(nestedPacket as Entity);
                        break;
                    default:
                        Console.WriteLine("scene tried to unpack a packet its not designed to");
                        break;
                }
            }
        }
        return newScene;
    }
    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
    {
        if (dataToSend is not Scene scene)
        {
            throw new InvalidDataException();
        }

        msg.Write((uint)packetType);
        msg.Write(scene.Id.ToString());
        msg.Write(scene.Name);
        foreach (var nestedScene in scene.Children)
        {
            ScenePacket.SendPacket(nestedScene, msg);
        }
        foreach (var entity in scene.Entities)
        {
            EntityPacket.SendPacket(entity, msg);
        }
        msg.Write((uint)PacketType.EndPacket);
        return msg;
    }
}