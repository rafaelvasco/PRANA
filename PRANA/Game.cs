using System.Runtime;
using System.Text.Json;
using PRANA.Common;

namespace PRANA;

public partial class Game : IDisposable
{
    public GameSettings Settings => _settings;

    public static (int Width, int Height) WindowSize
    {
        get => Platform.GetWindowSize();
        set
        {
            var (width, height) = Platform.GetWindowSize();
            if (value.Width == width && value.Height == height)
            {
                return;
            }

            Platform.SetWindowSize(value.Width, value.Height);
        }
    }

    public static string Title
    {
        get => Platform.GetWindowTitle();
        set => Platform.SetWindowTitle(value);
    }

    public static bool Resizable
    {
        get => (Platform.GetWindowFlags() & Platform.WindowFlags.Resizable) != 0;
        set => Platform.SetWindowResizable(value);
    }

    public static bool Borderless
    {
        get => (Platform.GetWindowFlags() & Platform.WindowFlags.Borderless) != 0;
        set => Platform.SetWindowBorderless(value);
    }

    public static bool Fullscreen
    {
        get => Platform.IsFullscreen();
        set
        {
            if (Platform.IsFullscreen() == value)
            {
                return;
            }

            Platform.SetWindowFullscreen(value);
        }
    }

    public static bool ShowCursor
    {
        get => Platform.CursorVisible();
        set => Platform.ShowCursor(value);
    }


    private static Game _instance;

    private readonly Scene _emptyScene;

    private const int DefaultGameWidth = 800;

    private const int DefaultGameHeight = 600;

    private Scene _current_scene;

    private readonly GameSettings _settings;

    private bool _isDisposed;


    public Game()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        _instance = this;

        //InitGameLoopVariables();

        _settings = ProcessGameSettings();

        Platform.Init(_settings);

        Platform.OnQuit = Platform_OnQuit;
        Platform.WindowResized = Platform_WindowResized;
        Platform.Minimized = Platform_LostFocus;
        Platform.LostFocus = Platform_LostFocus;
        Platform.GainedFocus = Platform_RestoredFocus;
        Platform.Restored = Platform_RestoredFocus;
        Platform.OnFileDrop = args => { OnFileDrop?.Invoke(args); };

        Graphics.Init(_settings.WindowSize.Width, _settings.WindowSize.Height);

        Content.Init();

        Graphics.LoadDefaultResources();

        Input.Init();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        _emptyScene = new EmptyScene();
    }

    ~Game()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Content.Free();
                
                _current_scene?.Unload();

                Graphics.Shutdown();

                Platform.Shutdown();
            }

            _isDisposed = true;
            _instance = null;
        }
    }

    public void Run(Scene scene = null)
    {
        if (_running)
        {
            return;
        }

        _running = true;

        _current_scene = scene ?? _emptyScene;

        _current_scene.Load();

        Tick(_current_scene);

        Platform.ShowWindow(true);

        while (_running)
        {
            Platform.ProcessEvents();
            Input.Update();
            Tick(_current_scene);
        }
    }

    public static void ToggleFullscreen()
    {
        Fullscreen = !Fullscreen;
    }

    public static void SuppressDraw()
    {
        _instance._suppressDraw = true;
    }

    public static void Exit()
    {
        _instance._running = false;
        _instance._suppressDraw = true;
    }

    private static GameSettings ProcessGameSettings()
    {
        (bool modified, GameSettings) CheckData(GameSettings settings)
        {
            bool modified = false;

            var title = settings.WindowTitle;
            var wWidth = settings.WindowSize.Width;
            var wHeight = settings.WindowSize.Height;

            if (title == null)
            {
                title = "PRANA Game";
                modified = true;
            }

            if (wWidth <= 0)
            {
                wWidth = DefaultGameWidth;
                modified = true;
            }

            if (wHeight <= 0)
            {
                wHeight = DefaultGameHeight;
                modified = true;
            }

            if (!modified)
            {
                return (false, settings);
            }

            var newSettings = new GameSettings()
            {
                WindowTitle = title,
                WindowSize = new Size(wWidth, wHeight),
                TargetFps = settings.TargetFps,
                Multisamples = settings.Multisamples,
                TransparentFrameBuffer = settings.StartFullscreen,
                VSync = settings.VSync,
                StartFullscreen = settings.StartFullscreen,
                WindowBorderMode = settings.WindowBorderMode
            };

            return (true, newSettings);
        }

        void Write(GameSettings settings)
        {
            var serializationOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var new_json = JsonSerializer.Serialize(settings, serializationOptions);

            File.WriteAllText(ContentProperties.GameSettingsFile, new_json);
        }

        GameSettings settings;

        if (File.Exists(ContentProperties.GameSettingsFile))
        {
            settings = Content.GetGameSettings();

            var (modified, newSettings) = CheckData(settings);

            if (modified)
            {
                Write(newSettings);
            }
        }
        else
        {
            settings = GameSettings.CreateDefault();

            Write(settings);
        }

        return settings;
    }

    private static void ShowExceptionMessage(Exception ex)
    {
        Platform.ShowRuntimeError("PRANA", $"An Error Ocurred: {ex?.Message}");
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ShowExceptionMessage(e.ExceptionObject as Exception);
    }
}