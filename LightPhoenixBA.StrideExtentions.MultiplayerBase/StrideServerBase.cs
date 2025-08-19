using Stride.BepuPhysics;
using Stride.Core.Serialization;
using Stride.Engine.Design;
using Stride.Engine.Processors;
using Stride.Physics;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public class StrideServerBase : IService
{
	 public static StrideServerBase Instance { get; protected set; }
	 readonly static Logger Log = GlobalLogger.GetLogger(typeof(StrideServerBase).FullName);
	 public static UrlReference<Scene> sceneUrl;
	 public ServiceRegistry Services { get; init; }
	 public GameSettings gameSettings { get; init; }
	 public ContentManager Content { get; init; }
	 public GameSystemCollection GameSystems { get; init; }
	 public SceneSystem sceneSystem { get; init; }
	 public BepuConfiguration physicsEngine { get; init; }
	 private Scene serverScene;

	 public readonly NetPeerConfiguration ServerConfig = NetConnectionConfig.GetDefaultConfig();
	 private NetServer netServer = new NetServer(NetConnectionConfig.GetDefaultConfig());
	 public static readonly ContentManagerLoaderSettings loadSettings = new ContentManagerLoaderSettings
	 {
			ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType([
				typeof(Entity),
				typeof(Scene),
				typeof(TransformComponent),
				typeof(Prefab),
				typeof(ColliderShape)
			]),
			AllowContentStreaming = false,
			LoadContentReferences = true,
	 };
	 public string ServerRootPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

	 private StrideServerBase()
	 {
			Console.WriteLine("Starting Stride console server");
			Instance = this;
			//start core systems
			Services = new ServiceRegistry();
			var objectDatabase = ObjectDatabase.CreateDefaultDatabase();
			var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
			var databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
			Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));
			Services.AddService<IContentManager>(Content = new ContentManager(Services));
			Services.AddService(gameSettings = HeadlessServer_SetupGameSettings());
			Services.RemoveService<IPhysicsSystem>();
			Services.AddService(physicsEngine = gameSettings.Configurations?.Get<BepuConfiguration>());
			// Services.AddService<ScriptSystem>();
			//start game systems
			GameSystems = new GameSystemCollection(Services);
			Services.AddService<IGameSystemCollection>(GameSystems);
			sceneSystem = new SceneSystem(Services);
			Services.AddService(sceneSystem);
			Services.AddService(new ScriptSystem(Services));
			GameSystems.Add(sceneSystem);

			GameSystems.Initialize();
			sceneSystem.Initialize();
			serverScene = Content.Load<Scene>(sceneUrl.Url, loadSettings);
			serverScene.Name = sceneUrl.ToString();
			sceneSystem.SceneInstance = new SceneInstance(Services, serverScene);
	 }

	 public StrideServerBase(IServiceRegistry Services)
	 {
			this.Services = (ServiceRegistry)Services;
			Log.Info("Starting Stride singleplayer server");
			Instance = this;
			Game Game = (Game)Services.GetService<IGame>();
			gameSettings = Game.Settings;
			Content = (ContentManager)Services.GetSafeServiceAs<IContentManager>();
			GameSystems = (GameSystemCollection)Services.GetSafeServiceAs<IGameSystemCollection>();
			sceneSystem = Game.SceneSystem;
			if (sceneUrl == null)
			{
				 throw new NullReferenceException($"{this.ToString()} was not provided a scene to connect to from the client");
			}

			sceneSystem.SceneInstance.RootScene.Children.Add(serverScene = Content.Load<Scene>(sceneUrl.Url));
			gameSettings = Content.Load<GameSettings>(GameSettings.AssetUrl); //Services.GetService<GameSettings>();
			Services.AddService(physicsEngine = gameSettings.Configurations?.Get<BepuConfiguration>());
	 }

	 public static IService NewInstance(IServiceRegistry services)
	 {
			if (Instance != null)
			{
				 Log.Error($"{typeof(StrideServerBase).FullName} is already declared as {Instance.ToString()}");
				 return Instance;
			}
			if (sceneUrl == null)
			{
				 Log.Error($"sceneUrl({sceneUrl}) is not set loading default");
				 sceneUrl = new UrlReference<Scene>("ServerScene");
			}
			if (services != null)
			{
				 services.AddService(new StrideServerBase(services));
			}
			else
			{
				 new StrideServerBase();
				 Instance.Services.AddService(Instance);
			}
			return Instance;

	 }

	 private GameSettings HeadlessServer_SetupGameSettings()
	 {
			GameSettings gameSettings = new();
			gameSettings.DefaultSceneUrl = sceneUrl.ToString();
			gameSettings.PackageName = this.ToString();
			if (gameSettings.DefaultGraphicsCompositorUrl != null)
			{
				 Log.Error(gameSettings.PackageName + "for the headless server seems to have a graphics renderer");
			}
			gameSettings.Configurations = new()
			{
				 Configurations = new()
				 {
						new Stride.Data.ConfigurationOverride()
						{ Configuration = new BepuConfiguration { BepuSimulations = [new BepuSimulation()] }}
				 },
			};
			return gameSettings;
	 }
	 private void StartServerSystems()
	 {
			netServer.Start();
			MP_PacketBase.InitilizePacketSystem(Content, netServer);
	 }
	 public async Task Execute()
	 {
			StartServerSystems();

			Console.WriteLine($"Server online {ToString()} at {netServer.Configuration.LocalAddress}");
			while (netServer.Status == NetPeerStatus.Running)
			{
				 NetIncomingMessage inc;
				 while ((inc = netServer.ReadMessage()) != null)
				 {
						switch (inc.MessageType)
						{
							 case NetIncomingMessageType.StatusChanged:
									OnHandleStatusChange(inc, (NetConnectionStatus)inc.ReadByte());
									break;

							 case NetIncomingMessageType.Data:
									object packetData = MP_PacketBase.ReceivePacket(inc);
									break;

							 case NetIncomingMessageType.DebugMessage:
									Console.WriteLine("Info = " + inc.ReadString());
									break;

							 default:
									Console.WriteLine($"({inc.MessageType}) has no action [{inc.PeekString()}]");
									break;
						}
						netServer.Recycle(inc);
						Thread.Sleep(1);
				 }
			}
	 }
	 private void OnHandleStatusChange(NetIncomingMessage inc, NetConnectionStatus status)
	 {
			string message = inc.PeekString();
			Console.WriteLine($"processing status({status}) of [{inc.SenderConnection}] with ({message})");

			switch (status)
			{
				 case NetConnectionStatus.RespondedConnect:
						OnRespondedConnect(inc, status);
						break;

				 case NetConnectionStatus.Connected:
						OnConnected(inc, status);
						break;

				 default:
						Console.Error.WriteLine($"Server unhandled action({status}) from {inc.SenderConnection} = ({inc.PeekString()})");
						break;
			}
	 }

	 public virtual void OnRespondedConnect(NetIncomingMessage inc, NetConnectionStatus status)
	 {
			//netServer.SendDiscoveryResponse(ConnectionPacket.SyncConnectionPacket(netServer), inc.SenderEndPoint);
			netServer.SendUnconnectedMessage(ConnectionPacket.SyncConnectionPacket(netServer), inc.SenderEndPoint);
	 }
	 public virtual void OnConnected(NetIncomingMessage inc, NetConnectionStatus status)
	 {
			SendSceneAndEntities(inc.SenderConnection);
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
