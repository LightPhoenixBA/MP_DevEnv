using JetBrains.Diagnostics;
using Stride.BepuPhysics;
using Stride.Core.Assets.Diagnostics;
using Stride.Core.Diagnostics;
using Stride.Core.Serialization;
using Stride.Engine.Design;
using Stride.Engine.Processors;
using Stride.Graphics.SDL;
using Stride.Physics;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public class StrideServerBase : IService
{
	 public static StrideServerBase Instance { get; protected set; }
	 readonly static Logger Log = GlobalLogger.GetLogger(typeof(StrideServerBase).FullName);
	 public static UrlReference<Scene> sceneUrl; //=> new UrlReference<Scene>("ServerScene");
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
			 //typeof(ProceduralModelDescriptor),
			 //typeof(Model)
			 ]),
			AllowContentStreaming = false,
			LoadContentReferences = true,
	 };
	 public string ServerRootPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
	 private StrideServerBase(IServiceRegistry Services)
	 {
			Log.Info("Starting Stride singleplayer server");
			Instance = this;
			Game Game = (Game)Services.GetService<IGame>();
			gameSettings = Game.Settings;
			Content = (ContentManager)Services.GetSafeServiceAs<IContentManager>();
			GameSystems = (GameSystemCollection)Services.GetSafeServiceAs<IGameSystemCollection>();
			sceneSystem = Game.SceneSystem;
			if (sceneUrl == null) throw new NullReferenceException($"{this.ToString()} was not provided a scene to connect to from the client");
			sceneSystem.SceneInstance.RootScene.Children.Add(Content.Load<Scene>(sceneUrl.Url));
			gameSettings = Content.Load<GameSettings>(GameSettings.AssetUrl); //Services.GetService<GameSettings>();
			Services.AddService(physicsEngine = gameSettings.Configurations?.Get<BepuConfiguration>());
	 }
	 public static IService NewInstance(IServiceRegistry services)
	 {
			if (Instance != null)
			{
				 Log.Error($"{typeof(StrideServerBase).FullName} is already declared");
				 return Instance;
			}
			if (sceneUrl == null)
			{
				 Log.Error($"sceneUrl({sceneUrl}) is not set loading default");
				 sceneUrl = new UrlReference<Scene>("ServerScene");
			}
			if (services != null)
			{
				 return new StrideServerBase(services);
			}
			else
			{
				 return new StrideServerBase();
			}
			//return Instance = Init(services, sceneUrl);
	 }
	 //public static StrideServerBase Init(IServiceRegistry? Services, UrlReference<Scene> ServerSceneHandle)
	 //{
		//	sceneUrl = ServerSceneHandle;
		//	if (Services != null)
		//	{
		//		 return new StrideServerBase(Services);
		//	}
		//	else
		//	{
		//		 return new StrideServerBase();
		//	}
	 //}

	 private StrideServerBase()
	 {
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
	 private GameSettings HeadlessServer_SetupGameSettings()
	 {
			var hmm = Content.Load<GameSettings>("ServerGameSettings");
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
			//gameSettings.Configurations.Configurations.Add(new BepuConfiguration { BepuSimulations = [new BepuSimulation()] });
			return gameSettings;
	 }
	 private void StartServerSystems()
	 {
			netServer.Start();
	 }
	 public async Task Execute()
	 {
			StartServerSystems();

			Log.Info("Server online " + ToString());
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
									Console.WriteLine(ToString() + " " + inc.ReadString());
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
