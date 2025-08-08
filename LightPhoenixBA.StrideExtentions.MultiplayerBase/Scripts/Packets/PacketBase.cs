namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public abstract class MP_PacketBase
{
	 public static readonly List<MP_PacketBase> registry = new();
	 protected static ContentManager Content { get; private set; }
	 public static void SetContentManager(ContentManager content)
	 {
			Content = content;
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

	 public static MP_PacketBase GetById(int id)
	 {
			return registry[id];
	 }

	 public static object ReceivePacket(NetIncomingMessage msg)
	 {
			int id = msg.ReadVariableInt32();
			return registry[id].Read(msg);
	 }
	 //public static object ReceivePacket(NetIncomingMessage msg)
	 //{
	 //	int id = msg.ReadVariableInt32();
	 //	return registry[id].Read(msg);
	 //}
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