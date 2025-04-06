// Copyright (c) Xenko contributors (https://xenko.com)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Lidgren.Network;
using MP_GameBase;
using Stride.Core;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Core.Storage;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Games;
using Stride.Physics;

namespace MP_GameStrideServer
{
    class MultiplayerConsoleGame
    {
        public ServiceRegistry Services { get; private set; }
        private Scene scene;//{  get; private set; }
        private PhysicsProcessor physics;
        private SceneSystem sceneSystem;
        private GameSystemCollection gameSystems;
        //   public NetPeerConfiguration NetPeerConfiguration = NetConnectionConfig.GetDefaultPeerConfig();
        //{
        //    AcceptIncomingConnections = true,
        //    Port = 4420
        //};
        public NetServer netServer;
        public NetConnection[] netConnections;

        public ContentManager Content { get; private set; }
        public readonly string ServerRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));



        [STAThread]
        static void Main(string[] args)
        {
            new MultiplayerConsoleGame().Run().Wait();
        }
        public void Initialize()
        {
            //lidgren networking
            netServer = new(NetConnectionConfig.GetDefaultConfig());
            //netServer.Configuration.LocalAddress = new System.Net.IPAddress([0, 0, 0, 0]);
            netServer.Start();

            //stride Database file provider
            Services = new ServiceRegistry();
            var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
            ObjectDatabase objectDatabase = ObjectDatabase.CreateDefaultDatabase();
            DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
            Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));

            //stride Content manager
            Content = new ContentManager(Services);
            Services.AddService<IContentManager>(Content);

            // Game systems
            gameSystems = new GameSystemCollection(Services);
            Services.AddService<IGameSystemCollection>(gameSystems);
            gameSystems.Initialize();

            // Load scene (physics only)
            var loadSettings = new ContentManagerLoaderSettings
            {
                // Ignore all references (Model, etc...)
                ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType(),
                AllowContentStreaming = true,
                //LoadContentReferences = true,
            };
            scene = Content.Load<Scene>("ServerScene", loadSettings);
            // Prefab playerPrefab = Content.Load<Prefab>("SpherePrefab");
            // Model model = Content.Load<Model>("SphereModel");
            var sceneInstance = new SceneInstance(Services, scene, ExecutionMode.Runtime);
            //foreach (var newEntity in playerPrefab.Entities)
            //{
            //scene.Entities.Add(newEntity);
            //}
            var sceneSystem = new SceneSystem(Services)
            {
                SceneInstance = sceneInstance,
            };
            Services.AddService(sceneSystem);

            var physics = new PhysicsProcessor();
            sceneInstance.Processors.Add(physics);

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
                                    netServer.SendMessage(connectedMessage,inc.SenderConnection,NetDeliveryMethod.ReliableOrdered);
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


            //   var socket = new SimpleSocket();
            //   socket.Connected += clientSocket =>
            //   {
            //       Console.WriteLine("Client connected");
            //       var reader = new BinarySerializationReader(clientSocket.ReadStream);
            //       ScenePacket packet = new(scene);
            //       packet.PacketSend(netServer.CreateMessage());
            //// var wat =      netServer.CreateMessage();
            //  //     wat.
            //       // reader.Serialize<Scene>(ref scene, ArchiveMode.Serialize);
            //      // reader.Write(scene.SerializeScene());
            //       // reader.Write<Scene>( scene);
            //       // reader.Write(  scene.Entities);
            //       while (true)
            //       {
            //           // Receive ray start/end
            //           var start = reader.Read<Vector3>();
            //           var end = reader.Read<Vector3>();
            //           // Raycast
            //           var result = physics.Simulation.Raycast(start, end);
            //           Console.WriteLine($"Performing raycast: {(result.Succeeded ? "hit" : "miss")}");
            //           // Send result
            //           clientSocket.WriteStream.WriteByte((byte)(result.Succeeded ? 1 : 0));
            //           clientSocket.WriteStream.Flush();
            //       }
            //   };
            //   await socket.StartServer(2655, false);
            //   Console.WriteLine("Server listening, press a key to exit");
            //   Console.ReadKey();
        }
        //static void AppLoop(object sender, EventArgs e)
        //{
        //    while (NativeMethods.AppStillIdle)
        //    {
        //        NetIncomingMessage inc;
        //        while ((inc = s_server.ReadMessage()) != null)
        //        {
        //            switch (inc.MessageType)
        //            {
        //                case NetIncomingMessageType.DebugMessage:
        //                case NetIncomingMessageType.WarningMessage:
        //                case NetIncomingMessageType.ErrorMessage:
        //                case NetIncomingMessageType.VerboseDebugMessage:
        //                    Output(inc.ReadString());
        //                    break;
        //                case NetIncomingMessageType.StatusChanged:
        //                    NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
        //                    switch (status)
        //                    {
        //                        case NetConnectionStatus.Connected:
        //                            // start streaming to this client
        //                            inc.SenderConnection.Tag = new StreamingClient(inc.SenderConnection, s_fileName);
        //                            Output("Starting streaming to " + inc.SenderConnection);
        //                            break;
        //                        default:
        //                            Output(inc.SenderConnection + ": " + status + " (" + inc.ReadString() + ")");
        //                            break;
        //                    }
        //                    break;
        //            }
        //            s_server.Recycle(inc);
        //        }

        //        // stream to all connections
        //        foreach (NetConnection conn in s_server.Connections)
        //        {
        //            StreamingClient client = conn.Tag as StreamingClient;
        //            if (client != null)
        //                client.Heartbeat();
        //        }

        //        System.Threading.Thread.Sleep(0);
        //    }
        //}
    }

}
