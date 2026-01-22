using JetBrains.Rd.Base;
using LegionForge.Chunks;
using Stride.Core;
using Stride.Graphics;
using Stride.Graphics.SDL;
using Stride.Physics;
using Stride.Profiling;
using Stride.Rendering;
using System.Security;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;
public static partial class MP_Stride_MultiplayerBaseExtentions
{
#if DEBUG
	 private static readonly Logger Log = GlobalLogger.GetLogger(typeof(MP_Stride_MultiplayerBaseExtentions).FullName);
#endif
	 private static readonly Simulation simulation = StrideClientBase.Services.GetService<SceneSystem>().SceneInstance.GetProcessor<PhysicsProcessor>().Simulation;
	 private static readonly DebugTextSystem DebugText = StrideClientBase.Services.GetService<DebugTextSystem>();

	 /// <summary>
	 /// uses the camera scene physics to determine if/what physics collier was hit.
	 /// </summary>
	 /// <param name="camera"></param> 
	 /// <param name="mousePos" ></param>
	 public static HitResult PerformServerRaycast(Vector3 nearPoint, Vector3 farPoint)
	 {
			Vector3 direction = Vector3.Normalize( farPoint - nearPoint);
			return StrideServerBase.Instance.sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation
					 .Raycast(nearPoint, nearPoint + (direction * 100f), CollisionFilterGroups.AllFilter);
	 }
	 public static HitResult PerformCameraRaycast(CameraComponent camera)
	 {
			//GameWindow window = StrideClientBase.Game.Window;
			Vector2 mousePos = StrideClientBase.Game.Input.AbsoluteMousePosition;
			Viewport viewport = RenderContext.GetShared(StrideClientBase.Game.Services).ViewportState.Viewport0;
			Vector3 origin = viewport.Unproject(new Vector3(mousePos, 0.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);//camera.Entity.Transform.Position;
			Vector3 target = viewport.Unproject(new Vector3(mousePos, 1.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity); ;
			HitResult result = simulation.Raycast(origin, target, CollisionFilterGroups.AllFilter);//PerformServerRaycast(closePoint, farPoint);
#if DEBUG

			if (result.Succeeded)
			{
				 Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
				 DebugText.Print($"Hit: {result.Collider.Entity.Name}",new Int2( mousePos), null, TimeSpan.FromSeconds(3));
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
	 //	 public static HitResult PerformCameraRaycast(CameraComponent camera)
	 //	 {
	 //			GameWindow window = StrideClientBase.Game.Window;
	 //			Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);
	 //			Int2 screenPos = (new Int2(Application.MousePosition) - StrideClientBase.Game.Window.Position);

	 //			// Reconstruct the projection-space position in the (-1, +1) range.
	 //			//    Don't forget that Y is down in screen coordinates, but up in projection space
	 //			Vector3 sPos;
	 //			sPos.X = (screenPos.X * 2f) - 1f;
	 //			sPos.Y = 1f - (screenPos.Y * 2f);

	 //			// Compute the near (start) point for the raycast
	 //			// It's assumed to have the same projection space (x,y) coordinates and z = 0 (lying on the near plane)
	 //			// We need to unproject it to world space
	 //			sPos.Z = 0f;
	 //			var vectorNear = Vector3.Transform(sPos, invViewProj);
	 //			vectorNear /= vectorNear.W;

	 //			// Compute the far (end) point for the raycast
	 //			// It's assumed to have the same projection space (x,y) coordinates and z = 1 (lying on the far plane)
	 //			// We need to unproject it to world space
	 //			sPos.Z = 1f;
	 //			var vectorFar = Vector3.Transform(sPos, invViewProj);
	 //			vectorFar /= vectorFar.W;

	 //			// Raycast from the point on the near plane to the point on the far plane and get the collision result
	 //			HitResult result = simulation.Raycast(vectorNear.XYZ(), vectorFar.XYZ());
	 //			//HitResult result = simulation.Raycast(camera.Entity.Transform.Position, camera.Entity.Transform.Position + farPoint, CollisionFilterGroups.AllFilter);//PerformServerRaycast(closePoint, farPoint);
	 //#if DEBUG

	 //			if (result.Succeeded)
	 //			{
	 //				 Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
	 //				 DebugText.Print($"Hit: {result.Collider.Entity.Name}", screenPos, null, TimeSpan.FromSeconds(3));
	 //			}
	 //			else
	 //			{
	 //				 Log.Info("Miss");
	 //				 DebugText.Print("Miss", screenPos, null, TimeSpan.FromSeconds(1));
	 //			}
	 //#endif
	 //			return result;
	 //	 }
	 //	 public static HitResult PerformCameraRaycast(CameraComponent camera)
	 //	 {
	 //			DebugTextSystem DebugText = StrideClientBase.Services.GetService<DebugTextSystem>();
	 //			//// Get the window client bounds relative to the screen
	 //			//GameWindow window = StrideClientBase.Game.Window;
	 //			////var clientBounds = window.ClientBounds;
	 //			//Int2 resolution = new Int2(window.ClientBounds.X, window.ClientBounds.Y);
	 //			//Int2 windowPosition = window.Position;//new Int2(Application.WindowWithFocus.ClientSize.Width, Application.WindowWithFocus.ClientSize.Height) ;
	 //			////Int2 mouseScreenPos = new Int2(Application.MousePosition);

	 //			//// Mouse position relative to the window client area
	 //			////Int2 mousePos = new Int2(Application.MousePosition.X, Application.MousePosition.Y);


	 //			//// Calculate the near (close) point in world space (z = 0 for near plane)
	 //			//Vector3 closePoint = ScreenToWorld(
	 //			//		camera, new Vector3(mousePos.X, mousePos.Y, 0f),
	 //			//		new Vector2(resolution.X, resolution.Y)
	 //			//);

	 //			//Vector3 farPoint = ScreenToWorld(
	 //			//		camera, new Vector3(mousePos.X, mousePos.Y, 1f),
	 //			//		new Vector2(resolution.X, resolution.Y)
	 //			//);
	 //			//Viewport viewport = new Viewport(StrideClientBase.Game.Window.Position.X, StrideClientBase.Game.Window.Position.Y, StrideClientBase.Game.Window.ClientBounds.X, StrideClientBase.Game.Window.ClientBounds.Y);
	 //			Viewport viewport = new Viewport(StrideClientBase.Game.Window.ClientBounds);
	 //			Int2 mousePos = new Int2(StrideClientBase.Game.Input.AbsoluteMousePosition);

	 //			//Vector3 closePoint = viewport.Unproject(new Vector3(mousePos.X, mousePos.Y, camera.NearClipPlane), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
	 //			Vector3 farPoint = viewport.Unproject(new Vector3(mousePos.X, mousePos.Y, camera.FarClipPlane), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

	 //			//	camera.Entity.Scene.Simulation.RayCast(closePoint, Vector3.Normalize(closePoint) * 100f, CollisionFilterGroups.AllFilter)
	 //			//HitResult result = StrideClientBase.Services.GetService<SceneSystem>().SceneInstance.GetProcessor<PhysicsProcessor>().Simulation.Raycast(camera.Entity.Transform.Position, farPoint, CollisionFilterGroups.AllFilter);//PerformServerRaycast(closePoint, farPoint);
	 //			HitResult result = simulation.Raycast(camera.Entity.Transform.Position, camera.Entity.Transform.Position + farPoint, CollisionFilterGroups.AllFilter);//PerformServerRaycast(closePoint, farPoint);
	 //#if DEBUG

	 //			if (result.Succeeded)
	 //			{
	 //				 Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
	 //				 DebugText.Print($"Hit: {result.Collider.Entity.Name}", mousePos, null, TimeSpan.FromSeconds(3));
	 //			}
	 //			else
	 //			{
	 //				 Log.Info("Miss");
	 //				 DebugText.Print("Miss", mousePos, null, TimeSpan.FromSeconds(1));
	 //			}
	 //#endif
	 //			return result;
	 //	 }
