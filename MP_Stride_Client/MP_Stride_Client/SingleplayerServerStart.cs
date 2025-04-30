using MP_Stride_MultiplayerBase;
using MP_Stride_ServerConsole;
using Stride.Engine;

public class SingleplayerServerStart : StartupScript
{
    public static MP_Stride_ServerBase server { get; private set; }
    override public void Start()
    {
        if (StrideClient.ClientInstance.isSinglePlayer == true)
        {
            server = new MP_Stride_ServerBase(Services);
        }
    }
}