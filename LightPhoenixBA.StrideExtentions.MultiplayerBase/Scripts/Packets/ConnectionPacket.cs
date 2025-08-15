using System.Reflection;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static class ConnectionPacket
{
	 public static List<MP_PacketBase> registry => MP_PacketBase.registry;

	 //public static void SyncConnectionPacket(NetIncomingMessage incFromServer)
	 //{
	 //	while (incFromServer.Data.Length != 0)
	 //	{
	 //		 string packetName = incFromServer.ReadString();
	 //		 // MP_PacketBase packet = AppDomain.CurrentDomain.GetAssemblies()
	 //		 //.First(o => o.GetTypes()
	 //		 // .First(o => o.Name == packetName))
	 //		 //;
	 //		 // registry.Add(packet);
	 //		 var packetType = AppDomain.CurrentDomain.GetAssemblies()
	 //					 .SelectMany(a =>
	 //					 {
	 //							try
	 //							{
	 //								 return a.GetTypes();
	 //							}
	 //							catch (ReflectionTypeLoadException e)
	 //							{
	 //								 return e.Types.Where(t => t != null)!;
	 //							}
	 //					 })
	 //					 .FirstOrDefault(t => t.FullName == packetName);

	 //		 if (packetType == null)
	 //		 {
	 //				Console.WriteLine($"[WARN] Packet type '{packetName}' not found.");
	 //				continue;
	 //		 }

	 //		 // Create instance
	 //		 if (Activator.CreateInstance(packetType) is MP_PacketBase packetInstance)
	 //		 {
	 //				registry.Add(packetInstance);
	 //		 }
	 //		 else
	 //		 {
	 //				Console.WriteLine($"[WARN] Type '{packetName}' is not an MP_PacketBase.");
	 //		 }
	 //	}
	 //}
	 public static void SyncConnectionPacket(NetIncomingMessage incFromServer)
	 {
			string packetName = incFromServer.ReadString();
			while (packetName.Length > 0)
			{
				 // MP_PacketBase packet = AppDomain.CurrentDomain.GetAssemblies()
				 //.First(o => o.GetTypes()
				 // .First(o => o.Name == packetName))
				 //;
				 // registry.Add(packet);
				 var packetType = AppDomain.CurrentDomain.GetAssemblies()
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
							 .FirstOrDefault(t => t.FullName == packetName);

				 if (packetType == null)
				 {
						Console.WriteLine($"[WARN] Packet type '{packetName}' not found.");
						continue;
				 }

				 //// Create instance
				 if (packetType == typeof(MP_PacketBase))
				 //if (Activator.CreateInstance(packetType) is MP_PacketBase packetInstance)
				 {

						MP_PacketBase.Register(Activator.CreateInstance(packetType) as MP_PacketBase);
				 }
				 //else
				 //{
				 //	Console.WriteLine($"[WARN] Type '{packetName}' is not an MP_PacketBase.");
				 //}
			}
	 }
	 public static NetOutgoingMessage SyncConnectionPacket(NetServer netServer)
	 {
			NetOutgoingMessage outFromServer = netServer.CreateMessage();
			foreach (MP_PacketBase packet in registry)
			{
				 outFromServer.Write(packet.GetType().FullName);
			}
			return outFromServer;
	 }
}
