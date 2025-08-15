//namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

//public static class MP_PacketRegistry
//{
//		public static List<MP_PacketBase<T>> PacketRegistry = new List<MP_PacketBase<T>>();

//		public static int Register<T>(MP_PacketBase<T> packet)
//		{
//			 if (PacketRegistry.Contains(packet))
//			 {
//					throw new InvalidOperationException("registry already contains " + packet.ToString());
//			 }
//			 else
//			 {
//					PacketRegistry.Add(packet);
//					return PacketRegistry.Count - 1;
//			 }
//		}

//		public static object ReceivePacket(NetIncomingMessage msg)
//		{
//			 int id = msg.ReadVariableInt32();
//			 return PacketRegistry[id].Read(msg);

//		}

//		//public static int GetIdFor<T>()
//		//{
//		//	 return ids[typeof(T)];
//		//}
//}
