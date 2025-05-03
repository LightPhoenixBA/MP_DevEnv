using MP_Stride_MultiplayerBase;
using MP_Stride_ServerConsole;
using Stride.Engine;
using Stride.Physics;
using Stride.Rendering.Compositing;

public class SingleplayerServerStart : SyncScript
{
    public static MP_Stride_ServerBase server { get; private set; }
    internal SceneCameraSlot sceneCamera;

   // public CameraComponent camera => sceneCamera.Camera;
    override public void Start()
    {
        if (StrideClient.ClientInstance.isSinglePlayer == true)
        {
            server = new MP_Stride_ServerBase(Services, this.GetSimulation());
        }
        else
        {
            Log.Info("multiplayer instance detected");
        }
            sceneCamera = SceneSystem.GraphicsCompositor.Cameras[0];
        //  camera = SceneSystem.GraphicsCompositor.Cameras.First(o => o.Name == "Main").Camera;
    }

    public override void Update()
    {
        if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Right))
        {
            // var physics = Services.GetService<IPhysicsSystem>();
            HitResult result = server.simulation.Raycast(sceneCamera.Camera.Entity.Transform.Position, sceneCamera.Camera.Entity.Transform.Rotation.Axis,CollisionFilterGroups.AllFilter);
            if (result.Succeeded)
            {
                Log.Info($"hit at {result.Point} on {result.Collider.Entity.Name}");
            }
            // physics.
            //Stride.Physics.HitResult hit =  PhysicsSystem.Raycast(camera.Position, camera.Forward, 1000);
        }
    }
}