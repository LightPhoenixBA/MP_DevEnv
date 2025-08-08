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
			Console.WriteLine($"Starting Console Server in {Environment.OSVersion}");
			new StrideServerBase().Execute().Wait();
	 }
}
