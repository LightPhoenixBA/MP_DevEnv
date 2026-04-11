using LegionForge.Chunks;
using Stride.Graphics;
using Stride.Physics;
using Stride.Profiling;
using Stride.Rendering;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;
public static partial class MP_Stride_MultiplayerBaseExtentions
{
#if DEBUG
	 private static readonly Logger Log = GlobalLogger.GetLogger(typeof(MP_Stride_MultiplayerBaseExtentions).FullName);
#endif
	 public static readonly PhysicsProcessor physicsProcessor = StrideClientBase.Services.GetService<SceneSystem>().SceneInstance.GetProcessor<PhysicsProcessor>();
	 public static readonly Simulation simulation = physicsProcessor.Simulation;
	 private static readonly DebugTextSystem DebugText = StrideClientBase.Services.GetService<DebugTextSystem>();

	 /// <summary>
	 /// uses the camera scene physics to determine if/what physics collier was hit.
	 /// </summary>
	 /// <param name="camera"></param> 
	 /// <param name="mousePos" ></param>
	 public static HitResult PerformServerRaycast(Vector3 nearPoint, Vector3 farPoint)
	 {
			Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
			return StrideServerBase.Instance.sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation
					 .Raycast(nearPoint, nearPoint + (direction * 100f), CollisionFilterGroups.AllFilter);
	 }
	 public static HitResult PerformCameraRaycast(CameraComponent camera)
	 {
			if (physicsProcessor == null)
			{
				 Log.Error($"No physicsProcessor in Scene");
				 return default;
			}
			Vector2 mousePos = StrideClientBase.Game.Input.AbsoluteMousePosition;
			Viewport viewport = RenderContext.GetShared(StrideClientBase.Game.Services).ViewportState.Viewport0;
			Vector3 origin = viewport.Unproject(new Vector3(mousePos, 0.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);//camera.Entity.Transform.Position;
			Vector3 target = viewport.Unproject(new Vector3(mousePos, 1.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity); ;
			HitResult result = simulation.Raycast(origin, target, CollisionFilterGroups.AllFilter);//PerformServerRaycast(closePoint, farPoint);
#if DEBUG

			if (result.Succeeded)
			{
				 Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
				 DebugText.Print($"Hit: {result.Collider.Entity.Name}", new Int2(mousePos), null, TimeSpan.FromSeconds(3));
				 var hmm = Debugging_StaticSphereModel.PlaceDebugSphere(StrideClientBase.Game.GraphicsDevice, target, Color.GreenYellow);
			}
			else
			{
				 Log.Info("Miss");
				 DebugText.Print("Miss", new Int2(mousePos), null, TimeSpan.FromSeconds(1));
				 var hmm = Debugging_StaticSphereModel.PlaceDebugSphere(StrideClientBase.Game.GraphicsDevice, target, Color.OrangeRed);
			}
#endif
			return result;
	 }

}