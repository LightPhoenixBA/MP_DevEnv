using Lidgren.Network;

namespace MP_GameBase;

public abstract class MP_PacketBase
{
    public static readonly List<MP_PacketBase> registry = new();
    public static void RegisterAll()
    {
        _ = new ScenePacket();
        _ = new EntityPacket();
        _ = new TransformPacket();
        //_ = ScenePacket.PacketId;
        //_ = EntityPacket.PacketId;
        //_ = TransformPacket.PacketId;
    }
    public static int Register(MP_PacketBase packet)
    {
        if (registry.Contains(packet))
        {
            return registry.IndexOf(packet);
        }
        registry.Add(packet);
        return registry.Count - 1;
    }

    public static MP_PacketBase GetById(int id) => registry[id];

    public static object ReceivePacket(NetIncomingMessage msg)
    {
        int id = msg.ReadVariableInt32();
        return registry[id].Read(msg);
    }
    public abstract void SendPacket(object data, NetOutgoingMessage msg);

    protected abstract object Read(NetIncomingMessage msg);
}

public abstract class MP_PacketBase<T> : MP_PacketBase
{
    public static int PacketId { get; private set; }
    protected MP_PacketBase()
    {
        PacketId = Register(this);
    }

    public static void SendPacket(T data, NetOutgoingMessage msg)
    {
        msg.WriteVariableInt32(PacketId);
        registry[PacketId].SendPacket(data, msg);
    }

    protected abstract void Write(T data, NetOutgoingMessage msg);

    public override void SendPacket(object data, NetOutgoingMessage msg)
    {
        Write((T)data, msg);
    }
}