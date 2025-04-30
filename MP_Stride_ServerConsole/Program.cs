namespace MP_Stride_ServerConsole;
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
        await new MP_Stride_ServerBase().Execute();
        //  new MP_Stride_ServerBase().Run();
    }
    /// <summary>
    /// A general console based server for the Stride game engine developed by LightPhoenix_BA (this method launches the console)
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
        MainAsync().Wait();
    }
}
