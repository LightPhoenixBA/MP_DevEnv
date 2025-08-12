using Stride.Core.Serialization;
using Stride.Engine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase.Sample.Client;

public class MultiplayerClientSample : StartupScript
{
	 public string PlayerName;
	 /// <summary>
	 /// only for singleplayer testing
	 /// </summary>
	 public UrlReference<Scene> ServerSceneHandle;
	 public static StrideClientBase ClientInstance;
	 public static StrideServerBase ServerInstance;
	 public MultiplayerClientSample()
	 {

	 }

	 public override void Start()
	 {
			StrideServerBase.sceneUrl = ServerSceneHandle;
			//if (Process.GetProcessesByName("LightPhoenixBA.StrideExtentions.MultiplayerServer").Length == 0)
			//{
			//	 ServerInstance = StrideServerBase.NewInstance(Services) as StrideServerBase;
			//	 Task.Run(() => ServerInstance.Execute());
			//}

			ClientInstance = StrideClientBase.NewInstance(Services) as StrideClientBase;
			Task.Run(() => ClientInstance.Execute());
			//this.Script.Dispose();
	 }
}