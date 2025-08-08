using System.Security.Cryptography.X509Certificates;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static class NetConnectionConfig
{
	 public static NetPeerConfiguration latestConfig = GetDefaultConfig();
	 public static NetPeerConfiguration GetDefaultConfig()
	 {
			return latestConfig = new NetPeerConfiguration(System.AppContext.TargetFrameworkName)
			{
				 LocalAddress = System.Net.IPAddress.Loopback,//new([127,0,0,1]),
				 Port = 4420
			};
	 }
	 public static NetPeerConfiguration GetDefaultClientConfig()
	 {
			latestConfig.Port++;
			return latestConfig;
	 }
}
