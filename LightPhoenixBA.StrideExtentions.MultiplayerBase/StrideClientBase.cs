using Stride.Engine.Processors;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase
{
	 public class StrideClientBase : IService
	 {
			readonly static Logger Log = GlobalLogger.GetLogger(typeof(StrideClientBase).FullName);
			public static IServiceRegistry Services { get; private set; }
			public static StrideClientBase Instance { get; private set; }
			public static Game Game { get; private set; }
			public static Scene GamePlayScene { get; private set; }
			public ScriptSystem ScriptSystem { get; private set; }
			private NetClient netClient = new NetClient(NetConnectionConfig.GetDefaultClientConfig());
			public NetPeerConfiguration serverConfig = NetConnectionConfig.GetDefaultConfig();

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
				 netClient.Start();
				 netClient.Connect(
					 serverConfig.LocalAddress.ToString()
				 , serverConfig.Port
				 , netClient.CreateMessage($"{Game.Window.Name} is requesting connection"));
				 Log.Warning("Starting Client Loop");

				 while (Game.IsRunning)
				 {
						NetIncomingMessage inc;
						while ((inc = netClient.ReadMessage()) != null)
						{
							 switch (inc.MessageType)
							 {
									case NetIncomingMessageType.DebugMessage:
									case NetIncomingMessageType.ErrorMessage:
									case NetIncomingMessageType.WarningMessage:
									case NetIncomingMessageType.VerboseDebugMessage:
										 Log.Info(inc.ReadString());
										 break;
									case NetIncomingMessageType.StatusChanged:
										 NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
										 string reason = inc.ReadString();
										 Log.Info(inc.SenderConnection + ": " + status + " (" + reason + ")");
										 switch (status)
										 {
												case NetConnectionStatus.InitiatedConnect:
													 netClient.Tag = this;
													 //ConnectionPacket.SyncConnectionPacket(inc);
													 MP_PacketBase.InitilizePacketSystem(Game.Content, netClient, inc);
													 break;

												case NetConnectionStatus.Connected:
													 if (MP_PacketBase.registry.Count == 0)
													 {
															throw new InvalidOperationException("MP_PacketBase.registry is empty");
													 }
													 break;
												default:
													 Log.Warning($"{ToString()} unhandled for ({status}) of {inc.SenderConnection} = {inc.Data}");
													 break;
										 }
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
									default:
										 Log.Info("Unhandled type: " + inc.MessageType + " " + inc.LengthBytes + " bytes");
										 Console.WriteLine($"{inc.ReadString()} has no action");
										 break;
							 }
						}
						//await ScriptSystem.NextFrame();
				 }
			}
	 }
}
