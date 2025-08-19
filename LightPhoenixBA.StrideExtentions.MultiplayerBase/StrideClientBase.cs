using Lidgren.Network;
using Stride.Engine.Processors;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase
{
	 public class StrideClientBase : NetClient ,  IService
	 {
			readonly static Logger Log = GlobalLogger.GetLogger(typeof(StrideClientBase).FullName);
			public StrideClientBase() : base(NetConnectionConfig.GetDefaultClientConfig())
			{

			}

			public static IServiceRegistry Services { get; private set; }
			public static StrideClientBase Instance { get; private set; }
			public static Game Game { get; private set; }
			public static Scene GamePlayScene { get; private set; }
			public ScriptSystem ScriptSystem { get; private set; }
			public NetPeerConfiguration serverConfig => NetConnectionConfig.GetDefaultConfig();

			public static IService NewInstance(IServiceRegistry services)
			{
				 if (Instance == null)
				 {
						Services = services;
						Game = Services.GetService<IGame>() as Game;
						services.AddService(Instance = new StrideClientBase());
				 }
				 else
				 {
						throw new InvalidOperationException("StrideClientService is already running as " + Services.GetService<StrideClientBase>());
				 }

				 return Instance;
			}
			public async Task Execute()
			{
				 ScriptSystem = Services.GetService<ScriptSystem>();
				 // netClient.Configuration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
				 Instance.Start();
				 MP_PacketBase.InitilizePacketSystem(Game.Content, Instance);
				 Instance.Connect(
					 serverConfig.LocalAddress.ToString()
				 , serverConfig.Port
				 , Instance.CreateMessage($"{Game.Window.Name} is requesting connection")
				 );

				 Log.Info("Starting Client Loop");

				 while (Game.IsRunning)
				 {
						NetIncomingMessage inc;
						while ((inc = Instance.ReadMessage()) != null)
						{
							// Log.Info("hmm = "+ inc.PeekString());
							 switch (inc.MessageType)
							 {
									case NetIncomingMessageType.StatusChanged:
										 NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
										 //Log.Warning($"handling {status} {inc.PeekString()}");
										 switch (status)
										 {
												case NetConnectionStatus.InitiatedConnect:
													 Instance.Tag = this;
													 //netClient.DiscoverKnownPeer(serverConfig.LocalAddress.ToString(), serverConfig.Port);
													 break;

												case NetConnectionStatus.Connected:
													 Log.Info($" {inc.ReadString()} at {inc.SenderEndPoint}");
													 if (MP_PacketBase.registry.Count == 0)
													 {
															Log.Error("MP_PacketBase.registry has no elements");
															//throw new NullReferenceException("MP_PacketBase.registry is empty");
													 }
													 break;
												default:
													 Log.Warning($"unhandled for ({status}) of {inc.SenderConnection} = {inc.Data}");
													 break;
										 }
										 break;
									case NetIncomingMessageType.UnconnectedData:
										 Log.Warning($"Syncing Packets using {inc.MessageType} from " + inc.SenderEndPoint);
										 ConnectionPacket.SyncConnectionPacket(inc);
										 break;
									case NetIncomingMessageType.Data:
										 object incPacket = MP_PacketBase.ReceivePacket(inc);
										 switch (incPacket)
										 {
												case Scene:
													 if (GamePlayScene == null)
													 {
															Log.Warning("Main gameplay scene has been loaded from " + inc.SenderConnection);
															GamePlayScene = incPacket as Scene;
															Game.SceneSystem.SceneInstance.RootScene.Children.Add(GamePlayScene);
													 }
													 else
													 {
															GamePlayScene.Children.Add(incPacket as Scene);
													 }
													 break;

												case Entity:
													 GamePlayScene.Entities.Add(incPacket as Entity);
													 break;

												case Tuple<string, Prefab>:
													 Prefab prefab = (incPacket as Tuple<string, Prefab>).Item2;
													 foreach (var entity in prefab.Entities)
													 {
															GamePlayScene.Entities.Add(entity);
													 }
													 break;

										 }
										 break;

									case NetIncomingMessageType.VerboseDebugMessage:
									case NetIncomingMessageType.DebugMessage:
										 Log.Info(inc.ReadString());
										 break;
									case NetIncomingMessageType.WarningMessage:
										 Log.Warning(inc.ReadString());
										 break;
									case NetIncomingMessageType.ErrorMessage:
										 Log.Error(inc.ReadString());
										 break;
									default:
										 Log.Error($"Unhandled {inc.MessageType} : {inc.ReadString()} length{inc.LengthBytes}");
										 break;

							 }
						}
						//await ScriptSystem.NextFrame();
				 }
			}
			public static bool IsSingleplayer => StrideClientBase.Services.GetService<StrideServerBase>() != null;
	 }
}
