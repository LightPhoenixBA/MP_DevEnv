namespace MP_Stride_MultiplayerBase
{
    public class HelloFromScene : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            var message = $"Hello from {Game} - ({SceneSystem.SceneInstance.RootScene.Name}){SceneSystem.InitialSceneUrl} from entity {Entity.Name} with id {Entity.Id} in scene = {Entity.Scene}";
        }
    }
}
