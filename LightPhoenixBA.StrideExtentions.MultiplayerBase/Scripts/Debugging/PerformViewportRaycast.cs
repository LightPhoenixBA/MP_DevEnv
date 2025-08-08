using Stride.Core.Diagnostics;
using Stride.Engine.Processors;
using Stride.Graphics.SDL;
using Stride.Physics;
using Stride.Profiling;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static partial class MP_Stride_MultiplayerBaseExtentions
{
#if DEBUG
		private static readonly Logger Log = GlobalLogger.GetLogger(typeof(MP_Stride_MultiplayerBaseExtentions).FullName);
#endif
		/// <summary>
		/// uses the camera scene physics to determine if/what physics collier was hit.
		/// </summary>
		/// <param name="camera"></param> 
		/// <param name="mousePos" ></param>
		public static HitResult PerformServerRaycast(Vector3 nearPoint, Vector3 farPoint)
		{
				Vector3 direction = Vector3.Normalize(nearPoint - farPoint);
				return StrideServerBase.Instance.sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation
						 .Raycast(nearPoint, nearPoint + direction * 100f, CollisionFilterGroups.AllFilter);
		}
		public static void PerformCameraRaycast(CameraComponent camera)
		{
			DebugTextSystem DebugText = StrideClientService.Services.GetService<DebugTextSystem>();
			 // Get the window client bounds relative to the screen
			 GameWindow window = StrideClientService.Game.Window;
				var clientBounds = window.ClientBounds;
				Int2 windowPosition = window.Position;//new Int2(Application.WindowWithFocus.ClientSize.Width, Application.WindowWithFocus.ClientSize.Height) ;
				Int2 mouseScreenPos = new Int2(Application.MousePosition);

				// Mouse position relative to the window client area
				Int2 mousePos = mouseScreenPos - windowPosition - new Int2(clientBounds.X, clientBounds.Y);
				//Int2 mousePos = new Int2(Application.MousePosition.X, Application.MousePosition.Y);

				Int2 resolution = new Int2(clientBounds.Width, clientBounds.Height);

				// Calculate the near (close) point in world space (z = 0 for near plane)
				Vector3 closePoint = ScreenToWorld(
						camera, new Vector3(mousePos.X, mousePos.Y, 0f),
						new Vector2(resolution.X, resolution.Y)
				);

				Vector3 farPoint = ScreenToWorld(
						camera, new Vector3(mousePos.X, mousePos.Y, 1f),
						new Vector2(resolution.X, resolution.Y)
				);

				HitResult result = PerformServerRaycast(closePoint, farPoint);
				if (result.Succeeded)
				{
						Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
				 DebugText.Print($"Hit: {result.Collider.Entity.Name}", mousePos, null, TimeSpan.FromSeconds(3));
				}
				else
				{
						Log.Info("Miss");
						DebugText.Print("Miss", mousePos, null, TimeSpan.FromSeconds(1));
				}
		}
		public static Vector3 ScreenToWorld(CameraComponent camera, Vector3 screenPosition, Vector2 screenSize)
		{
				// screenPosition: (x,y) is pixel coords, z is depth [0=near, 1=far]
				Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

				// Convert screen space [0, width] -> [-1, 1] (NDC space)
				Vector3 ndc = new Vector3(
						2.0f * screenPosition.X / screenSize.X - 1.0f,
						1.0f - 2.0f * screenPosition.Y / screenSize.Y,
						screenPosition.Z * 2.0f - 1.0f
				);

				// Create vector in homogeneous clip space
				Vector4 clipSpace = new Vector4(ndc, 1.0f);

				// Transform to world space
				Vector4 worldSpace = Vector4.Transform(clipSpace, invViewProj);
				if (worldSpace.W != 0f)
				{
						worldSpace /= worldSpace.W; // Perspective divide
				}

				return new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z);
		}
}
