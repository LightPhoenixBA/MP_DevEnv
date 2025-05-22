using Lidgren.Network;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Graphics.SDL;
namespace MP_Stride_MultiplayerBase;

public abstract class MP_PacketBase
{
    public static readonly List<MP_PacketBase> registry = RegisterAll();
    protected static ContentManager Content { get; private set; } 
    private static List<MP_PacketBase> RegisterAll()
    {
        // Content = content;
        return new List<MP_PacketBase>() {
             new ScenePacket()
            , new EntityPacket()
            , new TransformPacket()
            , new PrefabPacket()
        };
    }
    public static void SetContentManager(ContentManager content)
    {
        Content = content;
    }
    //private static void RegisterAll(ContentManager content)
    //{
    //    Content = content;
    //    _ = new ScenePacket();
    //    _ = new EntityPacket();
    //    _ = new TransformPacket();
    //    _ = new PrefabPacket();
    //}
    public static int Register(MP_PacketBase packet)
    {
        if (registry.Contains(packet))
        {
            return registry.IndexOf(packet);
        }
        registry.Add(packet);
        return registry.Count - 1;
    }

    public static MP_PacketBase GetById(int id)
    {
        return registry[id];
    }

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