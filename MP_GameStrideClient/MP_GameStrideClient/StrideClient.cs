using Lidgren.Network;
using MP_GameBase;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MP_GameStrideClient;

class StrideClient
{
    public class NetworkClient : AsyncScript
    {
        NetClient netClient;
        public IPAddress localAdress;
        private NetPeerConfiguration clientConfig = NetConnectionConfig.GetDefaultClientConfig();
        private NetPeerConfiguration serverConfig = NetConnectionConfig.GetDefaultConfig();
        private bool? lastResult;
        private TimeSpan lastResultTime;

        public override async Task Execute()
        {
            netClient = new NetClient(clientConfig);
            //string address = netConfig.LocalAddress.ToString();
            // string address = "lightphoenix.my.to";
            // string address = "localhost";
            netClient.Start();
            netClient.Connect(serverConfig.LocalAddress.ToString(), serverConfig.Port, netClient.CreateMessage("Stride Client is requesting connection"));
            MP_PacketBase.RegisterAll();

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
                                    SceneSystem.SceneInstance.RootScene.Children.Add(incPacket as Scene);
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
}
