using Lidgren.Network;
using Stride.Core.Serialization.Contents;

namespace MP_GameBase;

public enum PacketType
{
    EndPacket,
    Player,
    Scene,
    Entity,
    Transform
}
public interface IMP_Packet
{
    public static PacketType packetType { get; }
}
public class MP_PacketContainer
{
    private static ServiceRegistry _services;
    public static ServiceRegistry Services => _services;

    public static ContentManager Content => _contentManager;
    private static ContentManager _contentManager;

    private static MP_PacketContainer _instance;
    public static MP_PacketContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("MP_PacketContainer has not been initialized. Call Initialize() first.");
            }
            return _instance;
        }
    }

    public static void Initialize( ServiceRegistry services)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("MP_PacketContainer is already initialized.");
        }
        _instance = new MP_PacketContainer(services);
    }


    private readonly Dictionary<PacketType, MP_PacketBase> _packets;
    public static Dictionary<PacketType, MP_PacketBase> packets => Instance._packets;

    public static ContentManager ContentManager { get => _contentManager; set => _contentManager = value; }

    private MP_PacketContainer(ServiceRegistry services)
    {
        _services = services;
        _contentManager = (ContentManager)services.GetSafeServiceAs<IContentManager>();

        // You can initialize your packets here if needed
        _packets = new Dictionary<PacketType, MP_PacketBase>
        {
            { PacketType.EndPacket, null },
            { PacketType.Scene, new ScenePacket() },
            { PacketType.Entity, new EntityPacket() },
            { PacketType.Transform, new TransformPacket() },
        };
    }
    public static NetOutgoingMessage SendPacket(PacketType packetType, object dataToSend, NetOutgoingMessage msg)
    {
        return packets[packetType].WritePacket(dataToSend, msg);
    }
    public static Tuple<PacketType, object> ReceivePacket(NetIncomingMessage msg)
    {
        PacketType newPacketType = (PacketType)msg.ReadUInt32();
        return new Tuple<PacketType, object>(newPacketType, packets[newPacketType].ReadPacket(msg));
    }
    public static Dictionary<PacketType, object[]> ReceiveNestedPackets(NetIncomingMessage msg)
    {
        Dictionary<PacketType, List<object>> nestedObjects = new();
        PacketType incPacket = (PacketType)msg.ReadUInt32();
        while (incPacket != PacketType.EndPacket)
        {
            if (!nestedObjects.ContainsKey(incPacket))
            {
                nestedObjects.Add(incPacket, new());
            }
            nestedObjects[incPacket].Add(packets[incPacket].ReadPacket(msg));
            incPacket = (PacketType)msg.ReadUInt32();
        }
        return nestedObjects.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray()
        );
    }
}
public abstract class MP_PacketBase : IMP_Packet
{
    public abstract PacketType packetType { get; }
    abstract internal object ReadPacket(NetIncomingMessage msg);
    abstract internal NetOutgoingMessage WritePacket(object dataToSend, NetOutgoingMessage msg);
}