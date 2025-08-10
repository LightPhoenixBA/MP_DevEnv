namespace LightPhoenixBA.StrideExtentions.MultiplayerServer;
/// <summary>
/// root Server class for running Multiplayer in Stride with Lidgren containing MainAsync() method for running server
/// </summary>
public class ConsoleProgram
{
	 /// <summary>
	 /// A general console based server for the Stride game engine developed by LightPhoenix_BA (this method is called by the console)
	 /// </summary>
	 /// <param name="args"></param>
	 [STAThread]
	 public static void Main(string[] args)
	 {
			Console.WriteLine($"Starting a Stride console server in {Environment.OSVersion}");
			string argName = "Scene = ";
			if (args.Contains(argName))
			{
				 StrideServerBase.sceneUrl = new Stride.Core.Serialization.UrlReference<Scene>(args[0].Remove(0, argName.Length));
			}
			(StrideServerBase.NewInstance(null) as StrideServerBase).Execute().Wait();
	 }
}
