using Stride.BepuPhysics;
using Stride.Core.Diagnostics;
using Stride.Engine.Design;
using Stride.Physics;
using System.Diagnostics;
using System.Numerics;
namespace MP_Stride_ServerConsole;

public class MP_Stride_ServerBase
{
    public ServiceRegistry Services { get; init; }
    public GameSettings gameSettings { get; init; }
    public ContentManager Content { get; init; }
    public GameSystemCollection GameSystems { get; init; }
    public SceneSystem sceneSystem { get; init; }
    public BepuConfiguration physicsEngine { get; init; }
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
            typeof(ColliderShape)
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
        Game Game = (Game)Services.GetService<IGame>();
        gameSettings = Game.Settings;
        Content = (ContentManager)Services.GetSafeServiceAs<IContentManager>();
        GameSystems = (GameSystemCollection)Services.GetSafeServiceAs<IGameSystemCollection>();
        sceneSystem = Game.SceneSystem;
        //Bullet2PhysicsSystem discardPhysics = (Bullet2PhysicsSystem)Services.GetSafeServiceAs<IPhysicsSystem>();
        //Services.RemoveService<IPhysicsSystem>(discardPhysics);
        //GameSystems.Remove(discardPhysics);
        //Game.SceneSystem.SceneInstance.Processors.Clear();
        gameSettings = Content.Load<GameSettings>(GameSettings.AssetUrl); //Services.GetService<GameSettings>();
        Services.AddService(physicsEngine = gameSettings.Configurations?.Get<BepuConfiguration>());
        StartServerSystems();
    }
    public MP_Stride_ServerBase() : base()
    {
        Console.WriteLine("Starting Console Server");
        //start core systems
        Services = new ServiceRegistry();
        var objectDatabase = ObjectDatabase.CreateDefaultDatabase();
        var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
        var databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
        Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));
        Services.AddService<IContentManager>(Content = new ContentManager(Services));
        Services.AddService(gameSettings = Content.Load<GameSettings>("ServerGameSettings"));
        Services.RemoveService<IPhysicsSystem>();
        Services.AddService(physicsEngine = gameSettings.Configurations?.Get<BepuConfiguration>());
        // PhysicsGameSystem physics = new PhysicsGameSystem(Services, physicsEngine);
        //PhysicsGameSystem
        //start game systems
        serverScene = Content.Load<Scene>(gameSettings.DefaultSceneUrl, loadSettings);
        GameSystems = new GameSystemCollection(Services);
        Services.AddService<IGameSystemCollection>(GameSystems);
        sceneSystem = new SceneSystem(Services);
        Services.AddService(sceneSystem);
        GameSystems.Add(sceneSystem);
        // _simulation = sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;

        // BepuConfiguration _bepuConfig = Services.GetService<BepuConfiguration>();
        // var physicsSystem = new Bullet2PhysicsSystem(Services);
        // Services.AddService<IPhysicsSystem>(physicsSystem);
        // GameSystems.Add(physicsSystem);
        // Services.AddService(sceneSystem);
        GameSystems.Initialize();
        sceneSystem.Initialize();
        sceneSystem.SceneInstance = new SceneInstance(Services, serverScene);
        Simulation sim = sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;
        var hrp = sim.Raycast(-Vector3.UnitZ, Vector3.UnitZ);
        //  physicsSystem.Create(sceneSystem.SceneInstance.Processors.Get<PhysicsProcessor>(), PhysicsEngineFlags.CollisionsOnly);


        StartServerSystems();
        //physicsSystem = new Bullet2PhysicsSystem(Services);
        //Services.AddService(physicsSystem);
        //simulation = physicsSystem.Create(new PhysicsProcessor(), PhysicsEngineFlags.UseHardwareWhenPossible);
    }
    private void SafetyCheck()
    {
        var hrr = Services.GetService<IPhysicsSystem>().Name;
        if (hrr == "Bullet2PhysicsSystem")
        {
            throw new InvalidDataException("a bullet2Physics system is still active");
        }
        if (sceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>().Simulation.GetType() != typeof(BepuSimulation))
        {
            throw new InvalidDataException("a BepuSimulation was not found");
        }
    }
    private void StartServerSystems()
    {
        MP_PacketBase.RegisterAll(Content);
        netServer.Start();
    }
    public async Task Execute()
    {
        Log.Info("Server online "+ ToString());
        //SafetyCheck();
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
