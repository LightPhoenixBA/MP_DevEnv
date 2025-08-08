using LightPhoenixBA.StrideExtentions.MultiplayerBase;
using Stride.Core.Serialization;
using Stride.Engine;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase.Sample.Client;

public class MultiplayerClientSample : StartupScript
{
	 public string PlayerName;
	 /// <summary>
	 /// only for singleplayer testing
	 /// </summary>
	 public UrlReference<Scene> ServerSceneHandle;
	 public static StrideClientService ClientInstance;
	 public static StrideServerBase ServerInstance;
	 public MultiplayerClientSample()
	 {
		 
	 }

	 public override void Start()
	 {
			ClientInstance = StrideClientService.NewInstance(Services) as StrideClientService;
			ServerInstance = StrideServerBase.Init(Services,ServerSceneHandle);
			ClientInstance.Execute();
			//this.Script.Dispose();
	 }
}
