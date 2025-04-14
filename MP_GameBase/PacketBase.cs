using Lidgren.Network;

namespace MP_GameBase;
public abstract class MP_PacketBase
{
    public static readonly List<MP_PacketBase> Registry = new();

    public static int Register(MP_PacketBase packet)
    {
        if (Registry.Any(p => p.GetType() == packet.GetType()))
        {
            Console.WriteLine($"{packet.GetType().Name} has already been registered");
            return Registry.FindIndex(p => p.GetType() == packet.GetType());
        }
        else
        {
            int id = Registry.Count;
            Registry.Add(packet);
            return id;
        }
    }

    public static object ReceivePacket(NetIncomingMessage msg)
    {
        int id = msg.ReadVariableInt32();
        return ((dynamic)Registry[id]).Read(msg);
    }

    public static void RegisterAll()
    {
        _ = new ScenePacket();
        _ = new EntityPacket();
        _ = new TransformPacket();
    }
    internal abstract object Read(NetIncomingMessage msg);
}
public abstract class MP_PacketBase<T> : MP_PacketBase
{
    public int PacketId { get; }

    protected MP_PacketBase()
    {
        PacketId = Register(this);
    }
    internal abstract void Write(T data, NetOutgoingMessage msg);
}
//public interface IMP_Packet
//{
//   static public int PacketId { get; }
//    internal abstract void Write(object data, NetOutgoingMessage msg);
//    internal abstract object Read(NetIncomingMessage msg);

//}
//public static class MP_PacketBase
//{
//    public static readonly List<MP_PacketBase<T>> Registry = new();

//    public static int Register<T>(MP_PacketBase<T> packet)
//    {
//        int id = Registry.Count;
//        Registry.Add(packet);
//        return id;
//    }
//    public static object ReceivePacket(NetIncomingMessage msg)
//    {
//        int id = msg.ReadInt32();
//        return ((dynamic)Registry[id]).Read(msg); // use dynamic dispatch
//    }
//    public static void RegisterAll()
//    {
//        _ = new ScenePacket();
//        _ = new EntityPacket();
//        _ = new TransformPacket();
//    }
//}
//public abstract class MP_PacketBase<T>
//{    public int PacketId { get; }
//    internal abstract void Write(T data, NetOutgoingMessage msg);
//    internal abstract object Read(NetIncomingMessage msg);
//    public static object ReceivePacket(NetIncomingMessage msg)
//    {
//        int packetID = msg.ReadVariableInt32();
//        return MP_PacketBase.Registry[packetID].Read(msg);
//    }
//    internal static int Register(MP_PacketBase<T> newPacket)
//    {
//        MP_PacketBase.Registry.Add(newPacket);
//        return MP_PacketBase.Registry.Count - 1;
//    }
//}