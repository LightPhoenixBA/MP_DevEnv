using Stride.Core.Serialization;
using Stride.Engine;
using System.Threading.Tasks;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase.Sample.Client;

public class MultiplayerClientSample : StartupScript
{
	 public string PlayerName;
	 /// <summary>
	 /// scene to load.
	 /// </summary>
	 public UrlReference<Scene> ServerSceneHandle;
	 public static StrideClientBase ClientInstance;
	 public static StrideServerBase ServerInstance;
	 public override void Start()
	 {
			StrideServerBase.sceneUrl = ServerSceneHandle;
			ClientInstance = StrideClientBase.NewInstance(Services) as StrideClientBase;
			Task.Run(() => ClientInstance.Execute());
	 }
}