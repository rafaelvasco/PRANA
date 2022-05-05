
namespace PRANA;

public enum BorderMode
{
    Fixed,
    Resizable,
    Invisible
}

public struct GameSettings
{
    public Size WindowSize { get; set; }

    public string WindowTitle { get; set; }

    public bool StartFullscreen { get; set; }

    public BorderMode WindowBorderMode { get; set; }

    public bool TransparentFrameBuffer { get; set; }

    public bool VSync { get; set; }

    public int Multisamples { get; set; }

    public int TargetFps { get; set; }

    public static GameSettings CreateDefault()
    {
        return new GameSettings()
        {
            WindowSize = new Size(1280, 720),
            WindowTitle = "PRANA Game",
            WindowBorderMode = BorderMode.Fixed,
            StartFullscreen = false,
            TransparentFrameBuffer = false,
            VSync = false,
            Multisamples = -1,
            TargetFps = 60
        };
    }
}
