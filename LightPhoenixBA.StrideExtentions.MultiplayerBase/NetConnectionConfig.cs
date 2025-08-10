using System.Security.Cryptography.X509Certificates;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static class NetConnectionConfig
{
	 public static NetPeerConfiguration GetDefaultConfig()
	 {
			return new NetPeerConfiguration(System.AppContext.TargetFrameworkName)
			{
				 LocalAddress = System.Net.IPAddress.Loopback,
				 Port = 4420
			};
	 }
	 public static NetPeerConfiguration GetDefaultClientConfig()
	 {
			//NetPeerConfiguration latestConfig = GetDefaultConfig();
			//latestConfig.Port++;
			return new NetPeerConfiguration(System.AppContext.TargetFrameworkName)
			{
				 LocalAddress = System.Net.IPAddress.Loopback,
			//	 Port = 4420
			};
		//	return latestConfig;
	 }
}
