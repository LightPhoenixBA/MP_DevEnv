namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public struct MP_HitInfo
{
		public readonly Vector3 Normal;
		public readonly Vector3 Point;
		public readonly bool Succeeded;
		public readonly Entity? Entity;

		public MP_HitInfo(HitResult result)
		{
				Normal = result.Normal;
				Point = result.Point;
				Succeeded = result.Succeeded;
				if (result.Collider != null)
				{
						Entity = result.Collider.Entity;
				}
		}
		public MP_HitInfo(bool _Succeeded, HitInfo result)
		{
				Normal = result.Normal;
				Point = result.Point;
				Succeeded = _Succeeded;
				if (result.Collidable != null)
				{
						Entity = result.Collidable.Entity;
				}
		}
}
public static partial class MP_Stride_MultiplayerBaseExtentions
{
#if DEBUG
		private static readonly Logger Log = GlobalLogger.GetLogger(typeof(MP_Stride_MultiplayerBaseExtentions).FullName);
#endif
		public static readonly object Physics = SetPhysicsSimulation();
		private static readonly DebugTextSystem DebugText = StrideClientBase.Services.GetService<DebugTextSystem>();
		public static MP_HitInfo PerformCameraRaycast(CameraComponent camera)
		{
				Vector2 mousePos = StrideClientBase.Game.Input.AbsoluteMousePosition;
				Viewport viewport = RenderContext.GetShared(StrideClientBase.Game.Services).ViewportState.Viewport0;
				Vector3 origin = viewport.Unproject(new Vector3(mousePos, 0.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
				Vector3 target = viewport.Unproject(new Vector3(mousePos, 1.0f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
				MP_HitInfo result;
				switch (Physics)
				{
						case Simulation bullet2:
								result = new(bullet2.Raycast(origin, target, CollisionFilterGroups.AllFilter));
								break;
						case BepuSimulation bepu:
								result = new(bepu.RayCast(origin, target, 1000f, out HitInfo hitInfo), hitInfo);
								break;
						default:
								if (Physics == null)
								{
										Log.Warning("No physics simulation found. Raycast will not work.");
										result = default;
										break;
								}
								throw new NotImplementedException($"{Physics} Unsupported physics processor");
				}
#if DEBUG
				if (result.Succeeded)
				{
						Log.Info($"Hit at {result.Point} on {result.Entity.Scene.Name}.{result.Entity.Name}");
						DebugText.Print($"Hit: {result.Entity.Scene.Name}.{result.Entity.Name}", new Int2(mousePos), null, TimeSpan.FromSeconds(3));
				}
				else
				{
						Log.Info("Miss");
						DebugText.Print("Miss", new Int2(mousePos), null, TimeSpan.FromSeconds(1));
				}
#endif
				return result;
		}
		private static object SetPhysicsSimulation()
		{
				var hmm = StrideClientBase.Game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>();
				if (StrideClientBase.Services.GetService<IPhysicsSystem>() is Bullet2PhysicsSystem bullet2PhysicsSystem)
				{
						return StrideClientBase.Game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation;
				}
				else if (StrideClientBase.Services.GetService<BepuConfiguration>() is BepuConfiguration bepuPhysicsSystem)
				{
						return bepuPhysicsSystem.BepuSimulations[0];
				}
				Log.Error($"No physics simulation found. Ensure that either Bullet2PhysicsSystem or BepuConfiguration is registered in the services.");
				return null;
		}
}