﻿using Lidgren.Network;
using MP_GameBase;
using Stride.Core;
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
            netClient.Connect(serverConfig.LocalAddress.ToString(), serverConfig.Port , netClient.CreateMessage("Stride Client is requesting connection"));
            MP_PacketBase.RegisterAll();

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
                            object incPacket = MP_PacketBase.ReceivePacket(inc);
                            switch (incPacket)
                            {
                                case Scene:
                                    SceneSystem.SceneInstance.RootScene.Children.Add(incPacket as Scene);
                                    break;
                                //case typeof(ScenePacket):
                                //    Scene serverScene = (incPacket as ScenePacket).Read;
                                //    SceneSystem.SceneInstance.RootScene.Children.Add(serverScene);
                                //    //foreach (var item in serverScene.Entities)
                                //    //{
                                //    //    item.Scene = SceneSystem.SceneInstance.RootScene;
                                //    //   // SceneSystem.SceneInstance.RootScene.Entities.Add(item);
                                //    //}
                                //   // SceneSystem.SceneInstance.RootScene.Children.Add(serverScene);
                                //    break;
                                //case PacketType.Entity:
                                //    throw new NotImplementedException();
                                //    break;

                                //default:
                                //    throw new NotImplementedException("unhandled packet for " + incPacket.Item1.ToString());
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
