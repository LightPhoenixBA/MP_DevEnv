using Lidgren.Network;
using Stride.Graphics;
using System.Net;

namespace MP_Stride_MultiplayerBase;
public class StrideClient : AsyncScript
{
    public static StrideClient ClientInstance { get; private set; } //= new StrideClient();
    //static StrideClient()
    //{
    //    // ClientInstance ?? new StrideClient();
    //}
    public StrideClient()
    {
        if (ClientInstance == null)
        {
            ClientInstance = this;
        }
        else
        {
            Cancel();
        }
    }

    public NetClient netClient { get; private set; }
    public bool isSinglePlayer { get; private set; } = true;
    public IPAddress localAdress;
    private NetPeerConfiguration clientConfig = NetConnectionConfig.GetDefaultClientConfig();
    private static NetPeerConfiguration serverConfig = NetConnectionConfig.GetDefaultConfig();
    private bool? lastResult;
    private TimeSpan lastResultTime;
    public Scene serverScene { get; private set; }

    public override async Task Execute()
    {
        netClient = new NetClient(clientConfig);
        netClient.Start();
        netClient.Connect(serverConfig.LocalAddress.ToString(), serverConfig.Port, netClient.CreateMessage("Stride Client is requesting connection"));

        MP_PacketBase.RegisterAll(Content);
        //Scene nestedScene = Content.Load<Scene>("ClientScene");
        //nestedScene.Parent = SceneSystem.SceneInstance.RootScene;
        // _ = Content.Load<ProceduralModelDescriptor>("Sphere");

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
                            //case NetConnectionStatus.Connected:
                            //    Console.WriteLine("Connected!");
                            //Log.Info($"Connection establisted! {inc.ReadString()} Server:{inc.SenderConnection}");
                            //break;
                            default:
                                Log.Info(inc.SenderConnection + ": " + status + " (" + inc.ReadString() + ")");

                                break;
                        }
                        var hmm = Content;
                        break;
                    case NetIncomingMessageType.Data:
                        object incPacket = MP_PacketBase.ReceivePacket(inc);
                        switch (incPacket)
                        {
                            case Scene:
                                if (serverScene != null)
                                {
                                    throw new NotImplementedException("StrideClient can only have one server scene and is already set as " + serverScene.Name);
                                }

                                serverScene = incPacket as Scene;
                                SceneSystem.SceneInstance.RootScene.Children.Add(serverScene);
                                break;
                            case Stride.Engine.Entity:
                                serverScene.Entities.Add(incPacket as Entity);
                                break;
                            case Tuple<string, Prefab>:
                                Prefab prefab = (incPacket as Tuple<string, Prefab>).Item2;
                                foreach (var entity in prefab.Entities)
                                {
                                    serverScene.Entities.Add(entity);
                                }
                                break;

                        }
                        break;
                    default:
                        Log.Info("Unhandled type: " + inc.MessageType + " " + inc.LengthBytes + " bytes");
                        break;
                }

                if (lastResult.HasValue)
                {
                    DebugText.Print(lastResult.Value ? "Hit!" : "Miss...", new Int2(GraphicsDevice.Presenter.BackBuffer.Width / 2, (int)(GraphicsDevice.Presenter.BackBuffer.Height * 0.6f)));
                    if ((Game.UpdateTime.Total - lastResultTime) > TimeSpan.FromSeconds(2.0f))
                    {
                        lastResult = null;
                    }
                }
            }
            await Script.NextFrame();

        }
    }
}