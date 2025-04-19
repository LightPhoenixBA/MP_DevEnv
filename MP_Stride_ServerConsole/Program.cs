namespace MP_GameStrideServer;
/// <summary>
/// root client side class for MP_StrideGameBase
/// </summary>
public class MultiplayerConsoleGame : StartupScript
{
    public ServiceRegistry Services { get; private set; }
    private Scene scene;//{  get; private set; }
    private PhysicsProcessor physics;
    private SceneSystem sceneSystem;
    private GameSystemCollection gameSystems;
    public NetServer netServer;
    public static readonly ContentManagerLoaderSettings loadSettings = new ContentManagerLoaderSettings
    {
        ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType([typeof(Entity), typeof(Scene), typeof(TransformComponent), typeof(Prefab)]),
        AllowContentStreaming = false,
        LoadContentReferences = false,
    };

    public ContentManager Content { get; private set; }
    public readonly string ServerRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
    /// <summary>
    /// A general console server for the Stride game engine developed by LightPhoenix_BA (this method in intended to be called using Stride)
    /// </summary>
    public static async Task MainAsync()
    {
        await new MultiplayerConsoleGame().Run();
    }
    /// <summary>
    /// A general console based server for the Stride game engine developed by LightPhoenix_BA (this method launches the console)
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
        MainAsync().Wait();
    }
    public void Initialize()
    {
        //lidgren networking
        netServer = new(NetConnectionConfig.GetDefaultConfig());
        netServer.Start();

        //stride Database file provider
        Services = new ServiceRegistry();

        ObjectDatabase objectDatabase = ObjectDatabase.CreateDefaultDatabase();
        //server path
        var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
        DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
        Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));

        //stride Content manager
        Content = new ContentManager(Services);
        Services.AddService<IContentManager>(Content);

        // Game systems
        gameSystems = new GameSystemCollection(Services);
        Services.AddService<IGameSystemCollection>(gameSystems);
        gameSystems.Initialize();
        scene = Content.Load<Scene>("ServerScene", loadSettings);
        scene.Name = "ServerScene";
        var sceneInstance = new SceneInstance(Services, scene, ExecutionMode.Runtime);
        var sceneSystem = new SceneSystem(Services)
        {
            SceneInstance = sceneInstance,
        };
        Services.AddService(sceneSystem);

        var physics = new PhysicsProcessor();
        sceneInstance.Processors.Add(physics);
        MP_PacketBase.RegisterAll(Content);
    }

    public async Task Run()
    {
        Initialize();

        while (gameSystems != null)
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
                        Console.Write(inc.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:

                        NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                        switch (status)
                        {
                            case NetConnectionStatus.InitiatedConnect:
                                Console.WriteLine("Starting streaming to " + inc.SenderConnection.ToString());
                                break;
                            case NetConnectionStatus.Connected:
                                NetOutgoingMessage connectedMessage = netServer.CreateMessage();
                                ScenePacket.SendPacket(scene, connectedMessage);
                                netServer.SendMessage(connectedMessage, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                foreach (var entity in scene.Entities)
                                {
                                    NetOutgoingMessage entityMessage = netServer.CreateMessage();
                                    Prefab prefabCheck = Content.Load<Prefab>(entity.Name, loadSettings);
                                    if (prefabCheck != null)
                                    {
                                        PrefabPacket.SendUntypedPacket(entity.Name, entityMessage);
                                    }
                                    else
                                    {
                                        EntityPacket.SendPacket(entity, entityMessage);
                                    }
                                    netServer.SendMessage(entityMessage, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                }
                                break;
                            case NetConnectionStatus.Disconnected:
                                Console.WriteLine(inc.SenderConnection + " has disconnected");
                                break;
                            default:
                                Console.WriteLine("/n" + inc.SenderConnection + ": " + status + " (" + inc.ReadString() + ")");
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:

                        break;
                }
                netServer.Recycle(inc);
            }
        }
    }
}
