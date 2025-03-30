// Copyright (c) Xenko contributors (https://xenko.com)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using MP_GameBase;
using Stride.Core;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Core.Storage;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Engine.Network;
using Stride.Games;
using Stride.Physics;
using System.Diagnostics;
using Lidgren.Network;

namespace MP_GameStrideServer
{
        [ReferenceSerializer]
    class MultiplayerConsoleGame
    {
        public ServiceRegistry Services { get; private set; }
        public Scene scene;//{  get; private set; }
        public NetPeerConfiguration NetPeerConfiguration = new((typeof(MultiplayerConsoleGame)).ToString())
        {
            AcceptIncomingConnections = true,
        };
        public NetServer netServer;

        public ContentManager Content { get; private set; }
        public readonly string ServerRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

        static void Main(string[] args)
        {
            new MultiplayerConsoleGame().Run().Wait();
        }

        public async Task Run()
        {
            Services = new ServiceRegistry();
            netServer = new(NetPeerConfiguration);

            // Database file provider
            var mountPath = VirtualFileSystem.ResolveProviderUnsafe("\\Assets", true).Provider == null ? "/asset" : null;
            //  ObjectDatabase objectDatabase = new(ServerRootPath + mountPath, "Content");
            ObjectDatabase objectDatabase = ObjectDatabase.CreateDefaultDatabase();
            DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath);
            // DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, AppContext.BaseDirectory);


            //  var mountPath = VirtualFileSystem.ResolveProvider("Assets",true).Provider;
            // ObjectDatabase objectDatabase = ObjectDatabase.CreateDefaultDatabase();
            // string assetsPath = ServerRootPath + "\\Assets";
            //  ObjectDatabase objectDatabase = new ObjectDatabase(assetsPath, "/asset");
            // var mountPath = VirtualFileSystem.ResolveProvider(System.Reflection.Assembly.GetAssembly(GetType()).FullName, true).Provider;
            ///  var mountPath = VirtualFileSystem.ResolveProviderUnsafe("Assets", true).Provider == null ? "Assets" : null;
            // DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath.RootPath);
            Console.WriteLine(databaseFileProvider.RootPath);
            //Console.WriteLine(mountPath);
            Console.WriteLine(databaseFileProvider.ContentIndexMap);
            // Console.WriteLine(databaseFileProvider.DirectoryExists("ServerRootPath"));
            // DatabaseFileProvider databaseFileProvider = new DatabaseFileProvider(objectDatabase, mountPath.RootPath + "\\Assets");
            Services.AddService<IDatabaseFileProviderService>(new DatabaseFileProviderService(databaseFileProvider));

            // Content manager
            Content = new ContentManager(Services);
            Services.AddService<IContentManager>(Content);
            foreach (var file in Content.FileProvider.ListFiles(databaseFileProvider.RootPath, "*", VirtualSearchOption.AllDirectories))
            {
                Debug.WriteLine($"Available Content: {file}");
            }
            // Game systems
            var gameSystems = new GameSystemCollection(Services);
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
            scene = await Content.LoadAsync<Scene>("ServerScene", loadSettings);
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

            var socket = new SimpleSocket();
            socket.Connected += clientSocket =>
            {
                Console.WriteLine("Client connected");

                var reader = new BinarySerializationReader(clientSocket.ReadStream);
                // reader.Serialize<Scene>(ref scene, ArchiveMode.Serialize);
                reader.Write(scene.SerializeScene());
                // reader.Write<Scene>( scene);
                // reader.Write(  scene.Entities);
                while (true)
                {
                    // Receive ray start/end
                    var start = reader.Read<Vector3>();
                    var end = reader.Read<Vector3>();
                    // Raycast
                    var result = physics.Simulation.Raycast(start, end);
                    Console.WriteLine($"Performing raycast: {(result.Succeeded ? "hit" : "miss")}");
                    // Send result
                    clientSocket.WriteStream.WriteByte((byte)(result.Succeeded ? 1 : 0));
                    clientSocket.WriteStream.Flush();
                }
            };
            await socket.StartServer(2655, false);
            Console.WriteLine("Server listening, press a key to exit");
            Console.ReadKey();
        }
    }
}
