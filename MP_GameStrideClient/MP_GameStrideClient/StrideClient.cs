using Lidgren.Network;
using MP_GameBase;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Net;
using System.Threading.Tasks;
using static MP_GameBase.MP_GameBase;

namespace MP_GameStrideClient
{
    class StrideClient
    {
        public class NetworkClient : AsyncScript
        {
            NetClient netClient;
            public IPAddress localAdress;
            private NetPeerConfiguration netConfig = NetConnectionConfig.GetDefaultPeerConfig();
            private bool? lastResult;
            private TimeSpan lastResultTime;

            public override async Task Execute()
            {
                // var socket = new SimpleSocket();
                //netConfig.LocalAddress = localAdress;
                netClient = new NetClient(netConfig);
                // netClient.DiscoverLocalPeers(netConfig.Port);
                // netClient.RegisterReceivedCallback(new SendOrPostCallback(GotMessage));
                string address = netConfig.LocalAddress.ToString();
                // string address = "lightphoenix.my.to";
                   //string address = "localhost";
               // string address = IPAddress.Any.ToString();//netConfig.LocalAddress.ToString();
                //  netConfig.AcceptIncomingConnections = true;
                netClient.Start();
                netClient.Connect(address, netConfig.Port, netClient.CreateMessage("Stride Client is requesting connection"));

                while (Game.IsRunning)
                {
                    // Do stuff every new frame
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
                                    //    case NetConnectionStatus.None:
                                    //        break;
                                    case NetConnectionStatus.InitiatedConnect:
                                        netClient.Tag = this;
                                        break;
                                    //    case NetConnectionStatus.ReceivedInitiation:
                                    //        break;
                                    //    case NetConnectionStatus.RespondedAwaitingApproval:
                                    //        break;
                                    //    case NetConnectionStatus.RespondedConnect:
                                    //        break;
                                    //case NetConnectionStatus.Connected:
                                    //    break;
                                    ////case NetConnectionStatus.Disconnecting:
                                    ////    break;
                                    //case NetConnectionStatus.Disconnected:
                                    //    break;
                                    default:
                                        Log.Info(inc.SenderConnection + ": " + status + " (" + inc.ReadString() + ")");

                                        break;
                                }
                                // if (status == NetConnectionStatus.Connected)
                                //    s_form.EnableInput();
                                //else
                                //    s_form.DisableInput();

                                //if (status == NetConnectionStatus.Disconnected)
                                //    s_form.button2.Text = "Connect";

                                //string reason = inc.ReadString();
                                //Log.Info(status.ToString() + ": " + reason);

                                break;
                            case NetIncomingMessageType.Data:
                                PacketType incPacket = (PacketType)inc.ReadInt32();
                                switch (incPacket)
                                {
                                    case PacketType.Scene:

                                        Scene serverScene = ScenePacket.UnpackScene(inc);
                                        break;
                                    default:
                                        throw new NotImplementedException("unhandled packet for " + incPacket.ToString());
                                }
                                break;
                            default:
                                Log.Info("Unhandled type: " + inc.MessageType + " " + inc.LengthBytes + " bytes");
                                break;
                        }
                        await Script.NextFrame();

                        //if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(Keys.Space))
                        //{
                        //    var rotation = Matrix.RotationQuaternion(Entity.Transform.Rotation);

                        //    // Ask server
                        //    lastResult = await Task.Run(() =>
                        //    {
                        //        writer.Write(Entity.Transform.Position);
                        //        writer.Write(Entity.Transform.Position + (rotation.Forward * 100.0f));
                        //        writer.Flush();

                        //        // Get result
                        //        return socket.ReadStream.ReadByte() == 1;
                        //    });
                        //    lastResultTime = Game.UpdateTime.Total;
                        //}

                        // Display last result (max 2 seconds)
                        if (lastResult.HasValue)
                        {
                            DebugText.Print(lastResult.Value ? "Hit!" : "Miss...", new Int2(GraphicsDevice.Presenter.BackBuffer.Width / 2, (int)(GraphicsDevice.Presenter.BackBuffer.Height * 0.6f)));
                            if ((Game.UpdateTime.Total - lastResultTime) > TimeSpan.FromSeconds(2.0f))
                            {
                                lastResult = null;
                            }
                        }
                    }
                }
            }
        }
    }
}
