using Stride.Graphics;

namespace MP_Stride_ServerConsole;

public class MP_GraphicsService : IGraphicsDeviceService, IGraphicsDeviceManager

{
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    private GraphicsDevice _graphicsDevice;

    public MP_GraphicsService(GameBase gameBase)
    {
        gameBase.ConfirmRenderingSettings(false);

        //new GraphicsDevice();
        // _graphicsDevice = new GraphicsDevice(new GraphicsContext());
    }

    public event EventHandler<EventArgs> DeviceCreated;
    public event EventHandler<EventArgs> DeviceDisposing;
    public event EventHandler<EventArgs> DeviceReset;
    public event EventHandler<EventArgs> DeviceResetting;

    public bool BeginDraw()
    {
        throw new NotImplementedException();
    }

    public void CreateDevice()
    {
        throw new NotImplementedException();
    }

    public void EndDraw(bool present)
    {
        throw new NotImplementedException();
    }
}
