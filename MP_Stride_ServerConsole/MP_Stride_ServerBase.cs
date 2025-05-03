using Stride.Physics;

namespace MP_Stride_ServerConsole;

public class MP_Stride_ServerBase //: Game
{
    public ServiceRegistry Services { get; init; } = new ServiceRegistry();
    // public PhysicsProcessor physicsProcessor { get; init; }
    public Bullet2PhysicsSystem physicsSystem { get; init; }
    public Simulation simulation { get; init; }
    public ContentManager Content { get; init; }
    public GameSystemCollection GameSystems { get; init; }

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
    public MP_Stride_ServerBase(IServiceRegistry Services, Simulation Simulation)
    {
        this.Services = Services as ServiceRegistry;
        Content = Services.GetSafeServiceAs<IContentManager>() as ContentManager;
        GameSystems = Services.GetSafeServiceAs<IGameSystemCollection>() as GameSystemCollection;
        // physicsProcessor = (PhysicsProcessor)GameSystems.First(o => o.GetType() == typeof(PhysicsProcessor));//.First<PhysicsProcessor>();//Services.GetSafeServiceAs<IPhysicsSystem>() as PhysicsProcessor;
        //physicsProcessor = (Services.GetService<IPhysicsSystem>() as Bullet2PhysicsSystem);
        // var dafuq =     (Services.GetService<IPhysicsSystem>() as Bullet2PhysicsSystem).SafeArgument("PhysicsScene");
        //SceneSystem.InitialSceneUrl = "ServerScene";
        simulation = Simulation;
        StartServerSystems();
    }
    public MP_Stride_ServerBase() : base()
    {
        var objectDatabase = ObjectDatabase.CreateDefaultDatabase();
        var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
        var databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
        Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));
        Services.AddService<IContentManager>(Content = new ContentManager(Services));
        physicsSystem = new Bullet2PhysicsSystem(Services);
        Services.AddService(physicsSystem);
       // simulation = physicsSystem.Create(new PhysicsProcessor(),PhysicsEngineFlags.MultiThreaded);
        // physicsSystem = Services.GetService<IPhysicsSystem>() as Bullet2PhysicsSystem;
        serverScene = Content.Load<Scene>("ServerScene", loadSettings);
        // serverScene.Name = "ServerScene";
        // physicsProcessor.ParentScene = serverScene;
        //   simulation = physicsSystem.GetSimulation(this.GameSystems);//physicsSystem.Create(physicsProcessor, PhysicsEngineFlags.MultiThreaded);
        // physicsSystem.Initialize();
        //GameSystems.Initialize();
        StartServerSystems();
        var sceneSystem = new SceneSystem(Services);
        Services.AddService(sceneSystem);
        GameSystems = new GameSystemCollection(Services) { sceneSystem };
        Services.AddService<IGameSystemCollection>(GameSystems);
        GameSystems.Initialize();
        sceneSystem.SceneInstance = new SceneInstance(Services, serverScene);
       // Services.AddService(sceneSystem);
        sceneSystem.Initialize();

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
