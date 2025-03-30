using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Engine;
using Stride.Engine.Network;
using Stride.Graphics;
using Stride.Input;
using System;
using System.Threading.Tasks;
using static MP_GameBase.MP_GameBase;

namespace MP_GameStrideClient
{
    class StrideClient
    {
        public class NetworkClient : AsyncScript
        {
            private bool? lastResult;
            private TimeSpan lastResultTime;

            public override async Task Execute()
            {
                var socket = new SimpleSocket();
                await socket.StartClient("localhost", 2655, true);
                var writer = new BinarySerializationWriter(socket.WriteStream);
                Scene serverScene =  writer.Read<SerializedScene>().DeserializeScene();

                //Scene serverScene =  writer.Read<Scene>();
                //foreach (var newEntity in writer.Read<TrackingCollection<Entity>>())
                //{
                //    serverScene.Entities.Add(newEntity);
                //}
                SceneSystem.SceneInstance.RootScene.Children.Add(serverScene);
                writer.Flush();

                while (Game.IsRunning)
                {
                    // Do stuff every new frame
                    await Script.NextFrame();

                    if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(Keys.Space))
                    {
                        var rotation = Matrix.RotationQuaternion(Entity.Transform.Rotation);

                        // Ask server
                        lastResult = await Task.Run(() =>
                        {
                            writer.Write(Entity.Transform.Position);
                            writer.Write(Entity.Transform.Position + (rotation.Forward * 100.0f));
                            writer.Flush();

                            // Get result
                            return socket.ReadStream.ReadByte() == 1;
                        });
                        lastResultTime = Game.UpdateTime.Total;
                    }

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
