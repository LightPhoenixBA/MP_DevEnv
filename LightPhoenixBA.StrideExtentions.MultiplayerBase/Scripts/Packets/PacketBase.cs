using Stride.Core.Extensions;
using System.Reflection;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public abstract class MP_PacketBase
{
	 public static readonly List<MP_PacketBase> registry = new();
	 protected static ContentManager Content { get; private set; }
	 public static void InitilizePacketSystem(ContentManager content, NetPeer NetPeer, NetIncomingMessage? inc = null)
	 {
			Content = content;
			switch (NetPeer)
			{
				 case NetServer:
						RegisterAllPackets();
						break;
				 //case NetClient:
						//RegisterAllPackets();
						//throw new InvalidOperationException("client receives its packetRegistry from server");
					//	break;
				 default:
						break;
			}
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

	 public static object ReceivePacket(NetIncomingMessage msg)
	 {
			int id = msg.ReadVariableInt32();
			return registry[id].Read(msg);
	 }

	 public abstract void SendPacket(object data, NetOutgoingMessage msg);
	 protected abstract object Read(NetIncomingMessage msg);

	 private static void RegisterAllPackets()
	 {
			AppDomain.CurrentDomain.GetAssemblies()
			.Where(a =>
			{
				 // Keep the MultiplayerBase assembly itself
				 if (a == typeof(MP_PacketBase).Assembly)
				 {
						return true;
				 }

				 // Keep any assembly that directly references MultiplayerBase
				 return a.GetReferencedAssemblies()
					 .Any(r => r.FullName == typeof(MP_PacketBase).Assembly.FullName);
			})
			.SelectMany(a =>
			{
				 try
				 {
						return a.GetTypes();
				 }
				 catch (ReflectionTypeLoadException e)
				 {
						return e.Types.Where(t => t != null)!;
				 }
			})
			.Where(type =>
					type.IsClass &&
					!type.IsAbstract &&
					type.BaseType?.IsGenericType == true &&
					type.BaseType.GetGenericTypeDefinition() == typeof(MP_PacketBase<>))
			.ForEach(type => MP_PacketBase.Register((MP_PacketBase)Activator.CreateInstance(type)!))
			;
	 }
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