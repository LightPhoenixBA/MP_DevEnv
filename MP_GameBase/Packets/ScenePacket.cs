using Lidgren.Network;

namespace MP_GameBase;
public class ScenePacket : MP_PacketBase<Scene>
{
    public static readonly int PacketId = Register(new ScenePacket());
    internal override void Write(Scene data, NetOutgoingMessage msg)
    {
        msg.WriteVariableInt32(PacketId);
        msg.Write(data.Id.ToString());
        msg.Write(data.Name);
    }

    internal override object Read(NetIncomingMessage msg)
    {
        return new Scene
        {
            Id = Guid.Parse(msg.ReadString()),//public class ScenePacket : MP_PacketBase<Scene>
            Name = msg.ReadString()
            //{
        };
    }
    public static void SendPacket(Scene data, NetOutgoingMessage msg)
    {
        ((ScenePacket)Registry[PacketId]).Write(data, msg);
    }
}
//    internal override void Write(Scene data, NetOutgoingMessage msg)
//    {
//        msg.WriteVariableInt32(PacketId);
//        msg.Write(data.Id.ToString());
//        msg.Write(data.Name);
//    }

//    internal override Scene Read(NetIncomingMessage msg)
//    {
//        return new Scene
//        {
//            Id = Guid.Parse(msg.ReadString()),
//            Name = msg.ReadString()
//        };
//    }
//    public static void SendPacket(Scene data, NetOutgoingMessage msg)
//    {
//    Registry[PacketId].Write(data,msg);
//    }

//}


//public class ScenePacket : MP_PacketBase<Scene>
//{
//    public override MP_PacketBase<Scene> PacketType => this;
//    static ScenePacket()
//    {
//        Register(new());
//    }

//    internal override void Write(NetOutgoingMessage msg, object data)
//    {
//        if (data is not Scene scene)
//            throw new InvalidDataException("Expected Scene object");

//        msg.Write(scene.Id.ToString());
//        msg.Write(scene.Name);
//    }

//    internal override object Read(NetIncomingMessage msg)
//    {
//        return new Scene
//        {
//            Id = Guid.Parse(msg.ReadString()),
//            Name = msg.ReadString()
//        };
//    }

//    public override void SendPacket(Scene dataToSend, NetOutgoingMessage msg)
//    {
//        throw new NotImplementedException();
//    }

//    public override NetOutgoingMessage SendPacket(object dataToSend, NetOutgoingMessage msg)
//    {
//        throw new NotImplementedException();
//    }
//}
//public class ScenePacket : MP_PacketBase
//{
//    static ScenePacket() => Register<ScenePacket>();
//    internal override object ReadPacket(NetIncomingMessage msg)
//    {
//        throw new NotImplementedException();
//    }

//    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
//    {
//        throw new NotImplementedException();
//    }
//    public static NetOutgoingMessage SendPacket(Scene scene, NetOutgoingMessage msg)
//    {
//        PacketRegistry["ScenePacket"].WritePacket(scene, msg);
//        // MP_PacketContainer.packets[PacketType.Scene].WritePacket(scene, msg);
//        return msg;
//    }

//    public override MP_PacketBase SendPacket()
//    {
//        throw new NotImplementedException();
//    }
//}

//    public override MP_PacketBase SendPacket()
//    {
//        throw new NotImplementedException();
//    }

//    internal override Scene ReadPacket(NetIncomingMessage msg)
//    {
//        Scene newScene = new Scene
//        {
//            Id = Guid.Parse(msg.ReadString()),
//            Name = msg.ReadString()
//        };
//        var nestedObjs = MP_PacketContainer.ReceiveNestedPackets(msg);
//        foreach (var packetType in nestedObjs)
//        {
//            foreach (var nestedPacket in packetType.Value)
//            {
//                switch (packetType.Key)
//                {
//                    case PacketType.Scene:
//                        newScene.Children.Add(nestedPacket as Scene);
//                        break;
//                    case PacketType.Entity:
//                        newScene.Entities.Add(nestedPacket as Entity);
//                        break;
//                    default:
//                        Console.WriteLine("scene tried to unpack a packet its not designed to");
//                        break;
//                }
//            }
//        }
//        return newScene;
//    }
//    internal override NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg)
//    {
//        if (dataToSend is not Scene scene)
//        {
//            throw new InvalidDataException();
//        }

//        msg.Write(this.ToString());
//        msg.Write(scene.Id.ToString());
//        msg.Write(scene.Name);
//        foreach (var nestedScene in scene.Children)
//        {
//            ScenePacket.SendPacket(nestedScene, msg);
//        }
//        foreach (var entity in scene.Entities)
//        {
//            EntityPacket.SendPacket(entity, msg);
//        }
//       // msg.Write((uint)PacketType.EndPacket);
//        return msg;
//    }
//}