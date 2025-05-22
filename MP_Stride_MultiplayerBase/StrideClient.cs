using Lidgren.Network;
using Stride.Engine.Design;
using System.Diagnostics;
using System.Net;

namespace MP_Stride_MultiplayerBase;
public class StrideClient : AsyncScript
{
    public static StrideClient ClientInstance { get; internal set; } = new();
    public StrideClient()
    {
        Priority = 1;
        if (ClientInstance == null)
        {
            ClientInstance = this;
        }
        else
        {
            Cancel();
            Log.Error("aborting player duping");
           throw new InvalidOperationException("aborting player duping");
        }
       // ClientInstance.SceneSystem.SceneInstance.RootScene.Entities.Add(new Entity{ this});
    }
    public NetClient netClient { get; private set; }
    public static bool isSinglePlayer  => Process.GetProcessesByName("MP_Stride_ServerConsole").Length == 0;//{ get; private set; }
    public IPAddress localAddress;
    private NetPeerConfiguration clientConfig = NetConnectionConfig.GetDefaultClientConfig();
    private static NetPeerConfiguration serverConfig = NetConnectionConfig.GetDefaultConfig();
    public Scene serverScene { get; private set; }

    public override async Task Execute()
    {
        netClient = new NetClient(clientConfig);
        netClient.Start();
        netClient.Connect(serverConfig.LocalAddress.ToString(), serverConfig.Port
        , netClient.CreateMessage("Stride Client is requesting connection"));

        //MP_PacketBase.RegisterAll(Content);
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
            }
            await Script.NextFrame();

        }
    }
}