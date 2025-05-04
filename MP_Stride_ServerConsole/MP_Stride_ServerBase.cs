using Stride.Core.Diagnostics;
using Stride.Engine.Design;
using Stride.Graphics;
using Stride.Physics;
using System.Drawing.Printing;
namespace MP_Stride_ServerConsole;

public class MP_Stride_ServerBase
{
    public ServiceRegistry Services { get; init; } = new ServiceRegistry();
    internal GameSettings gameSettings { get; init; } = new GameSettings()
    {
        DefaultSceneUrl = "ServerScene",
        DefaultGraphicsCompositorUrl = null
    };
    //public Bullet2PhysicsSystem physicsSystem { get; init; }
    public ContentManager Content { get; init; }
    public GameSystemCollection GameSystems { get; init; }
    private Stride.Core.Diagnostics.Logger Log { get; } = GlobalLogger.GetLogger("MP_Stride_ServerBase");

    private Scene serverScene;
    private NetServer netServer = new NetServer(NetConnectionConfig.GetDefaultConfig());
    public static readonly ContentManagerLoaderSettings loadSettings = new ContentManagerLoaderSettings
    {
        ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType([
            typeof(Entity),
            typeof(Scene),
            typeof(TransformComponent),
            typeof(Prefab),
            //typeof(ProceduralModelDescriptor),
            //typeof(Model)
        ]),
        AllowContentStreaming = false,
        LoadContentReferences = true,
    };
    public string ServerRootPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
    public MP_Stride_ServerBase(IServiceRegistry Services)
    {
        Log.Info("Starting Stride singleplayer server");
        this.Services = (ServiceRegistry)Services;
        Content = (ContentManager)Services.GetSafeServiceAs<IContentManager>();
        GameSystems = (GameSystemCollection)Services.GetSafeServiceAs<IGameSystemCollection>();
        StartServerSystems();
    }
    public MP_Stride_ServerBase() : base()
    {
        Console.WriteLine("Starting Console Server");
        //start core systems
        var objectDatabase = ObjectDatabase.CreateDefaultDatabase();
        var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
        var databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
        Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));
        Services.AddService<IContentManager>(Content = new ContentManager(Services));
        StartServerSystems();

        //start game systems
        serverScene = Content.Load<Scene>("ServerScene", loadSettings);
        Services.AddService(gameSettings);
        Services.AddService<IPhysicsSystem>(new Bullet2PhysicsSystem(Services));
        GameSystems = new GameSystemCollection(Services);
        Services.AddService<IGameSystemCollection>(GameSystems);
        var sceneSystem = new SceneSystem(Services)
        {
            SceneInstance = new SceneInstance(Services, serverScene)
        };
        GameSystems.Add(sceneSystem);

        Services.AddService(sceneSystem);

        GameSystems.Initialize();
        // Services.AddService(sceneSystem);
        sceneSystem.Initialize();
        //physicsSystem = new Bullet2PhysicsSystem(Services);
        //Services.AddService(physicsSystem);
        //simulation = physicsSystem.Create(new PhysicsProcessor(), PhysicsEngineFlags.UseHardwareWhenPossible);
    }
    private void StartServerSystems()
    {
        MP_PacketBase.RegisterAll(Content);
        netServer.Start();
    }
    public async Task Execute()
    {
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
                        Console.WriteLine(this.ToString() + " " + inc.ReadString());
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
                //Log.Info($"{ToString()} Initiating connection with {inc.SenderConnection}");
                Console.WriteLine($"{ToString()} Initiating connection with {inc.SenderConnection}");
                break;

            case NetConnectionStatus.Connected:
                //Log.Info($"{ToString()} Client connected: {inc.SenderConnection}");
                Console.WriteLine($"{ToString()} Client connected: {inc.SenderConnection}");
                SendSceneAndEntities(inc.SenderConnection);
                break;

            case NetConnectionStatus.Disconnected:
                // Log.Info($"{ToString()} Client disconnected: {inc.SenderConnection}");
                Console.WriteLine($"{ToString()} Client disconnected: {inc.SenderConnection}");
                break;

            default:
                // Log.Info($"{ToString()} Status changed: {status} - {inc.ReadString()}");
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
            Console.WriteLine($"sending {entity.Name} as {referencedObj}");
            switch (referencedObj)
            {
                case ProceduralModelDescriptor:
                    throw new InvalidOperationException("depreciated");
                case Prefab:
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
}
