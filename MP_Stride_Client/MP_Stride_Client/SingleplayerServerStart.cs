using MP_Stride_MultiplayerBase;
using MP_Stride_ServerConsole;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Compositing;
using System;

public class SingleplayerServerStart : SyncScript
{
    public static MP_Stride_ServerBase server { get; private set; }
    public SceneCameraSlot sceneCamera { get; private set; }
    Viewport viewport;

    // public CameraComponent camera => sceneCamera.Camera;
    override public void Start()
    {
        if (StrideClient.isSinglePlayer == true)
        {
            server = new MP_Stride_ServerBase(Services);
            SceneSystem.SceneInstance.RootScene.Children.Add(Content.Load<Scene>("ServerScene"));
        }
        else
        {
            Log.Info("multiplayer instance detected");
        }
        sceneCamera = SceneSystem.GraphicsCompositor.Cameras[0];
        viewport = new Viewport(Game.Window.ClientBounds.Left, Game.Window.ClientBounds.Bottom, Game.Window.ClientBounds.X, Game.Window.ClientBounds.Y);
        //  camera = SceneSystem.GraphicsCompositor.Cameras.First(o => o.Name == "Main").Camera;
    }

    public override void Update()
    {
        if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Right))
        {
            if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Right))
            {
                Log.Warning("Clean me");
                var cameraEntity = sceneCamera.Camera.Entity;
                CameraComponent camera = sceneCamera.Camera;

                Int2 mousePos = new Int2((int)Input.AbsoluteMousePosition.X, (int)Input.AbsoluteMousePosition.Y);

                Vector3 nearPoint = cameraEntity.Transform.Position;
                Vector3 farPoint = ScreenToWorld(camera, new Vector3(mousePos.X, mousePos.Y, 1f),new Vector2 (Game.Window.ClientBounds.Size.Width, Game.Window.ClientBounds.Size.Height));

                Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

                Simulation simulation = this.GetSimulation();
                HitResult result = simulation.Raycast(nearPoint, nearPoint + (direction * 1000f), CollisionFilterGroups.AllFilter);

                if (result.Succeeded)
                {
                    Log.Info($"Hit at {result.Point} on {result.Collider.Entity.Name}");
                    DebugText.Print($"Hit: {result.Collider.Entity.Name}", mousePos, null, TimeSpan.FromSeconds(3));
                }
                else
                {
                    Log.Info("Miss");
                    DebugText.Print("Miss", mousePos, null, TimeSpan.FromSeconds(0.3));
                }
            }
        }
    }
    public Vector3 ScreenToWorld(CameraComponent camera, Vector3 screenPosition, Vector2 screenSize)
    {
        // screenPosition: (x,y) is pixel coords, z is depth [0=near, 1=far]
        Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

        // Convert screen space [0, width] -> [-1, 1] (NDC space)
        Vector3 ndc = new Vector3(
            (2.0f * screenPosition.X) / screenSize.X - 1.0f,
            1.0f - (2.0f * screenPosition.Y) / screenSize.Y,
            screenPosition.Z * 2.0f - 1.0f
        );

        // Create vector in homogeneous clip space
        Vector4 clipSpace = new Vector4(ndc, 1.0f);

        // Transform to world space
        Vector4 worldSpace = Vector4.Transform(clipSpace, invViewProj);
        if (worldSpace.W != 0f)
            worldSpace /= worldSpace.W; // Perspective divide

        return new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z);
    }
}