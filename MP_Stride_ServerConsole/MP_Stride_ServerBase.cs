using Stride.Core.Diagnostics;
using Stride.Graphics;
using Stride.Rendering;

namespace MP_Stride_ServerConsole;

public class MP_Stride_ServerBase : Game
{
    // public readonly MP_GraphicsService serverGraphicsService { get; init; } = new MP_GraphicsService(this);
    new public ServiceRegistry Services { get; init; } = new ServiceRegistry();
    public GameSystemCollection GameSystems { get; init; }
    public ContentManager Content { get; init; }

    public GraphicsDevice GraphicsDevice { get; init; } = GraphicsDevice.New(DeviceCreationFlags.None,GraphicsProfile.Level_9_1);
    private Scene serverScene;
    private NetServer netServer = new NetServer(NetConnectionConfig.GetDefaultConfig());
    public static readonly ContentManagerLoaderSettings loadSettings = new ContentManagerLoaderSettings
    {
        ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType([
            typeof(Entity),
            typeof(Scene),
            typeof(TransformComponent),
            typeof(Prefab),
            typeof(ProceduralModelDescriptor),
            typeof(Model)
        ]),
        AllowContentStreaming = false,
        LoadContentReferences = false,
    };
    public string ServerRootPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
    public MP_Stride_ServerBase(IServiceRegistry Services)
    {
        this.Services = Services as ServiceRegistry;
        Content = Services.GetSafeServiceAs<IContentManager>() as ContentManager;
        GameSystems = Services.GetSafeServiceAs<IGameSystemCollection>() as GameSystemCollection;
        SceneSystem.InitialSceneUrl = "ServerScene";
        StartServerSystems();
    }
    public MP_Stride_ServerBase() : base()
    {
        var objectDatabase = ObjectDatabase.CreateDefaultDatabase();
        var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
        var databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
        Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));
        Services.AddService<IContentManager>(Content = new ContentManager(Services));
        Services.AddService(GameSystems = new GameSystemCollection(Services));
        Services.AddService(GraphicsDevice);

        //Add physics
        //  var wat = new GraphicsDeviceManager(this);
        // graphicsDevice = new GraphicsDevice(Services);
        // Services.AddService<IGraphicsDeviceService>(wat);
       // var physics = new PhysicsProcessor();
        //{
        //    ParentScene = serverScene,
        //    Enabled = true,
        //};
        GameSystems.Initialize();
        StartServerSystems();
        var sceneSystem = new SceneSystem(Services)
        {
            SceneInstance = new SceneInstance(Services, serverScene),
        };
        Services.AddService(sceneSystem);

    }
    private void StartServerSystems()
    {
        // Register all packets
        MP_PacketBase.RegisterAll(Content);

        // Scene setup
        serverScene = Content.Load<Scene>("ServerScene", loadSettings);
        serverScene.Name = "ServerScene";

        netServer.Start();
    }
    public async Task Execute()
    {
     //   Stride.Graphics.GraphicsAdapter.;
        while (netServer.Status == NetPeerStatus.Running)
        {
            NetIncomingMessage inc;
            while ((inc = netServer.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine(this.ToString() + " "+inc.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        HandleStatusChange(inc);
                        break;

                    case NetIncomingMessageType.Data:
                        // Handle game-specific data
                        break;
                }
                netServer.Recycle(inc);
            }
        }
    }
    private void HandleStatusChange(NetIncomingMessage inc)
    {
        var status = (NetConnectionStatus)inc.ReadByte();

      switch (status)
{
    case NetConnectionStatus.InitiatedConnect:
        Log.Info($"{ToString()} Initiating connection with {inc.SenderConnection}");
        Console.WriteLine($"{ToString()} Initiating connection with {inc.SenderConnection}");
        break;

    case NetConnectionStatus.Connected:
        Log.Info($"{ToString()} Client connected: {inc.SenderConnection}");
        Console.WriteLine($"{ToString()} Client connected: {inc.SenderConnection}");
        SendSceneAndEntities(inc.SenderConnection);
        break;

    case NetConnectionStatus.Disconnected:
        Log.Info($"{ToString()} Client disconnected: {inc.SenderConnection}");
        Console.WriteLine($"{ToString()} Client disconnected: {inc.SenderConnection}");
        break;

    default:
        Log.Info($"{ToString()} Status changed: {status} - {inc.ReadString()}");
        Console.WriteLine($"{ToString()} Status changed: {status} - {inc.ReadString()}");
        break;
}
    }

    private void SendSceneAndEntities(NetConnection connection)
    {
        var sceneMessage = netServer.CreateMessage();
        ScenePacket.SendPacket(serverScene, sceneMessage);
        netServer.SendMessage(sceneMessage, connection, NetDeliveryMethod.ReliableOrdered);

        foreach (var entity in serverScene.Entities)
        {
            var entityMessage = netServer.CreateMessage();
            var referencedObj = Content.Get(typeof(object), entity.Name) ?? Content.Load(typeof(object), entity.Name, loadSettings);

            switch (referencedObj)
            {
                case Prefab:
                case ProceduralModelDescriptor:
                    PrefabPacket.SendUntypedPacket(entity.Name, entityMessage);
                    break;
                case Stride.Engine.Entity:
                    EntityPacket.SendPacket(entity, entityMessage);
                    break;
                default:
                    Console.WriteLine($"Unhandled entity type: {referencedObj?.GetType().Name ?? "null"} for {entity.Name}");
                    break;
            }

            netServer.SendMessage(entityMessage, connection, NetDeliveryMethod.ReliableOrdered);
        }
    }

    public override void ConfirmRenderingSettings(bool gameCreation)
    {
        throw new NotImplementedException();
    }
}
