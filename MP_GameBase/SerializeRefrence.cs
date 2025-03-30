using Stride.Core.Extensions;
using Stride.Core.Serialization;

namespace MP_GameBase
{
    public static partial class MP_GameBase
    {
        public static SerializedScene SerializeScene(this Scene scene)
        {
            SerializedScene serializedScene = new SerializedScene(scene.Id,scene.Name);
            serializedScene.entities = scene.Entities.ToArray();
            serializedScene.childrenScenes = null;// = scene.Children.ForEach(o =>  o.SerializeScene());
            return serializedScene;
        }
        public static Scene DeserializeScene(this SerializedScene scene)
        {
            Scene newScene = new Scene()
            {
                Id = scene.id,
                Name = scene.name,
                //  Children = scene.childrenScenes;
            };
            foreach (var sceneEntity in scene.entities)
            {
                newScene.Entities.Add(sceneEntity);
            }
            return newScene;
        }
        public struct SerializedScene(Guid id, string name)
        {
            public Guid id = id;
            public string name = name;
            public Entity[] entities = [];
            public SerializedScene[] childrenScenes;
        }
    }
}
