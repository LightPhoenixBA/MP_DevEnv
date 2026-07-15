using System.Threading.Tasks;
using Stride.Core.Serialization;
using Stride.Engine;
namespace LightPhoenixBA.StrideExtentions.MultiplayerBase.Sample.Client;

public class MultiplayerClientSample : SyncScript
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
		public override void Update()
		{
				if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Right))
				{
						var hitInfo = MP_Stride_MultiplayerBaseExtentions.PerformCameraRaycast(SceneSystem.GraphicsCompositor.Cameras[0].Camera);
				}
		}
}