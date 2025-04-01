using Lidgren.Network;
using System.Runtime.CompilerServices;

namespace MP_GameBase
{
    public static partial class MP_GameBase
    {
        public class ScenePacket : Packet
        {
            Guid id;
            string name;

            //public ScenePacket(Guid id,string name)
            //{
            //    this.id = id;
            //    this.name = name;
            //}
            public ScenePacket(Scene scene)
            {
                id = scene.Id;
                name = scene.Name;
                //   entities = [];
                //   childrenScenes = null;
            }
            public ScenePacket(NetIncomingMessage netIncomingMessage)
            {
                PacketReceive(netIncomingMessage);
            }

            //   Entity[] entities;
            //  SerializedScene[] childrenScenes;

            public void PacketSend(NetOutgoingMessage netOutgoingMessage)
            {
                netOutgoingMessage.Write((int)packetType);
                netOutgoingMessage.Write(id.ToString());
                netOutgoingMessage.Write(name);
            }
            //public override void PacketReceive(NetIncomingMessage netIncomingMessage)
            //{
            //    // netOutgoingMessage.Write((int)packetType);
            //    id = new Guid(netIncomingMessage.ReadString());
            //    name = netIncomingMessage.ReadString();
            //}
            public ScenePacket PacketReceive(NetIncomingMessage netIncomingMessage)
            {
                // netOutgoingMessage.Write((int)packetType);
                id = new Guid(netIncomingMessage.ReadString());
                name = netIncomingMessage.ReadString();
                return this;
            }
            public static Scene UnpackScene(NetIncomingMessage netIncomingMessage)
            {
                ScenePacket packet = new ScenePacket(netIncomingMessage);//ScenePacket.PacketReceive(netIncomingMessage);
               return new Scene() { Id = packet.id, Name = packet.name };       
            }
            //public static ScenePacket WritePacket(this Scene scene)
            //{
            //   return new ScenePacket(scene.Id, scene.Name);
            //}
            //public static void SendPacket(this Scene scene,NetServer server)
            //{
            //    WritePacket(scene).PacketSend(server.CreateMessage());
            //}
            //public static Scene ReadPacket(NetIncomingMessage netIncomingMessage)
            //{
            //    ScenePacket packet =  ScenePacket.PacketReceive(netIncomingMessage);
            //   return new Scene() { Id = packet.id, Name = packet.name };
            //}
            //public override Scene PacketReceive(NetIncomingMessage netIncomingMessage, IPacket packet)
            //{
            //   // ScenePacket dataType = new netIncomingMessage.ReadUInt16();
            //    return new Scene() { Id = dataType.id, Name = dataType.name };
            //}

            //public override T PacketReceive<T>(NetIncomingMessage netIncomingMessage, IPacket dataType)
            //{
            //    throw new NotImplementedException();
            //}
        }
        //public static SerializedScene SerializeScene(this Scene scene)
        //{
        //    SerializedScene serializedScene = new SerializedScene(scene.Id,scene.Name);
        //    serializedScene.entities = scene.Entities.ToArray();
        //    serializedScene.childrenScenes = null;// = scene.Children.ForEach(o =>  o.SerializeScene());
        //    return serializedScene;
        //}
        //public static Scene DeserializeScene(this SerializedScene scene)
        //{
        //    Scene newScene = new Scene()
        //    {
        //        Id = scene.id,
        //        Name = scene.name,
        //        //  Children = scene.childrenScenes;
        //    };
        //    foreach (var sceneEntity in scene.entities)
        //    {
        //        newScene.Entities.Add(sceneEntity);
        //    }
        //    return newScene;
        //}
        //public record SerializedScene(Guid id, string name)
        //{
        //    public Guid id = id;
        //    public string name = name;
        //    public Entity[] entities = [];
        //    public SerializedScene[] childrenScenes;
        //}
    }
}
