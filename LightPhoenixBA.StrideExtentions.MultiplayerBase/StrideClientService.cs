using Stride.Core.Diagnostics;
using Stride.Engine.Processors;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase
{
	 public class StrideClientService : IService
	 {
			readonly static Stride.Core.Diagnostics.Logger Log = GlobalLogger.GetLogger(typeof(StrideClientService).FullName);
			public static IServiceRegistry Services { get; private set; }
			public static StrideClientService Instance { get; private set; }
			public static Game Game { get; private set; }
			public static Scene GamePlayScene { get; private set; }

			public ScriptSystem ScriptSystem { get; private set; }
			public NetClient netClient { get; private set; }
			//public NetPeerConfiguration netPeerConfiguration { get; private set; }
			private NetPeerConfiguration clientConfig = NetConnectionConfig.GetDefaultClientConfig();
			private static NetPeerConfiguration serverConfig = NetConnectionConfig.GetDefaultConfig();

			public static IService NewInstance(IServiceRegistry services)
			{
				 if (Instance == null)
				 {
						Services = services;
						Game = Services.GetService<IGame>() as Game;
						services.AddService(Instance = new StrideClientService());
				 }
				 else throw new InvalidOperationException("StrideClientService is already running as " + Services.GetService<StrideClientService>());
						return Instance;
			}
			public async Task Execute()
			{
				 ScriptSystem = Services.GetService<ScriptSystem>();
				 netClient = new NetClient(clientConfig);
				 netClient.Start();
				 netClient.Connect(
					 serverConfig.LocalAddress.ToString()
				 , serverConfig.Port
				 , netClient.CreateMessage($"{Game.Window.Name} is requesting connection"));
				 MP_PacketBase.SetContentManager(Game.Content);

				 while (Game.IsRunning)
				 {
						NetIncomingMessage inc;
						while ((inc = netClient.ReadMessage()) != null)
						{
							 switch (inc.MessageType)
							 {
									case NetIncomingMessageType.ErrorMessage:
									case NetIncomingMessageType.WarningMessage:
									case NetIncomingMessageType.DebugMessage:
										 Log.Info(inc.ReadString());
										 break;
									case NetIncomingMessageType.StatusChanged:
										 NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
										 switch (status)
										 {
												case NetConnectionStatus.InitiatedConnect:
													 netClient.Tag = this;
													 break;
												case NetConnectionStatus.Connected:
													 Log.Info($"Connection establisted! {inc.ReadString()} Server:{inc.SenderConnection}");
													 break;
												default:
													 Log.Info(inc.SenderConnection + ": " + status + " (" + inc.ReadString() + ")");
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
															Log.Warning("Main gameplay scene has been set using " + ToString());
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
										 break;
							 }
						}
						await ScriptSystem.NextFrame();
				 }
			}
	 }
}
