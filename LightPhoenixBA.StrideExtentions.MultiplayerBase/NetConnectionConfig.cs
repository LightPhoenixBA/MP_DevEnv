namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static class NetConnectionConfig
{
	 public static int DefaultPort = 4420;
	 public static NetPeerConfiguration GetDefaultConfig()
	 {
			return new NetPeerConfiguration(System.AppContext.TargetFrameworkName)
			{
				 LocalAddress = System.Net.IPAddress.Loopback,
				 Port = DefaultPort
			};
	 }
	 public static NetPeerConfiguration GetDefaultClientConfig()
	 {
			NetPeerConfiguration Config = GetDefaultConfig();
			Config.Port = Random.Shared.Next(DefaultPort + 1, 9999);
			//Config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			return Config;
			//latestConfig.Port++;
			//return new NetPeerConfiguration(System.AppContext.TargetFrameworkName)
			//{
			//	 LocalAddress = System.Net.IPAddress.Loopback,
			//	 Port = Random.Shared.Next(DefaultPort + 1, 9999),
				 
				 
			//}.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			//	return latestConfig;
	 }
}
