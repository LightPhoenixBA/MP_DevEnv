namespace LightPhoenixBA.StrideExtentions.MultiplayerServer;
/// <summary>
/// root Server class for running Multiplayer in Stride with Lidgren containing Main() method for running server
/// </summary>
public class ConsoleProgram
{
	 /// <summary>
	 /// A general console server for the Stride game engine developed by LightPhoenix_BA (this method in intended to be called using Stride)
	 /// </summary>
	 public static async Task MainAsync()
	 {
			await new StrideServerBase().Execute();
	 }
	 /// <summary>
	 /// A general console based server for the Stride game engine developed by LightPhoenix_BA (this method is called by the console)
	 /// </summary>
	 /// <param name="args"></param>
	 [STAThread]
	 public static void Main(string[] args)
	 {
			Console.WriteLine($"Starting Console Server in {Environment.OSVersion}");
			MainAsync().Wait();
	 }
}
