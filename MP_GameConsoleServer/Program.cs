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

            MP_PacketContainer.Initialize( Services);
            //  MP_PacketContainer.Services = Services;
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

        }
    }
}
