using System.Runtime.CompilerServices;
using PRANA.Foundation;

namespace PRANA;



public enum GraphicsBackend
{
    Direct3D,
    OpenGL,
    Metal,
    Vulkan
}

public enum MSAALevel
{
    None,
    Two,
    Four,
    Eight,
    Sixteen
}

public static unsafe partial class Graphics
{
    public static GraphicsBackend GraphicsBackend { get; private set; }

    public static MSAALevel MSAALevel
    {
        get => _msaaLevel;
        set
        {
            if (_msaaLevel != value)
            {
                _msaaLevel = value;

                UpdateGraphicsFlags();
            }
        }
    }

    public static bool VSync
    {
        get => _vSync;
        set
        {
            if (_vSync != value)
            {
                _vSync = value;

                UpdateGraphicsFlags();
            }
        }
    }

    public static RenderView CreateDefaultView()
    {
        var defaultView = new RenderView();

        defaultView.SetBackColor(Color.Black);
        defaultView.SetViewport(0, 0, _backBufferWidth, _backBufferHeight);
        defaultView.SetProjection(Transform.CreateOrthographicOffCenter(0f, _backBufferWidth, _backBufferHeight, 0f, -1000.0f, 1000.0f));
        defaultView.SetTransform(Transform.Identity);

        return defaultView;
    }


    private static int _backBufferWidth;
    private static int _backBufferHeight;
    private static MSAALevel _msaaLevel = MSAALevel.None;
    private static bool _vSync = true;

    private static Bgfx.StateFlags _renderState = Bgfx.StateFlags.WriteRgb | Bgfx.StateFlags.WriteA;
    private static Bgfx.ResetFlags _graphicsFlags = Bgfx.ResetFlags.None;

    private static bool _graphicsFlagsChanged;

    private static ushort _currentViewId;

    private static Texture2D _drawTexture;

    private static Texture2D _primitiveTexture;

    private static Shader _defaultShader;


    private static readonly List<RenderResource> _graphicsResources = new(16);


    internal static void Init(int backbufferWidth, int backbufferHeight)
    {
        _backBufferWidth = backbufferWidth;
        _backBufferHeight = backbufferHeight;

        var platformData = new Bgfx.PlatformData()
        {
            nwh = Platform.GetRenderSurfaceHandle().ToPointer()
        };

        Bgfx.SetPlatformData(&platformData);

        Bgfx.InitData init = new();

        Bgfx.init_ctor(&init);

        init.type = GetRendererType();

        switch (init.type)
        {
            case Bgfx.RendererType.Direct3D11:
            case Bgfx.RendererType.Direct3D12:
                GraphicsBackend = GraphicsBackend.Direct3D;
                break;
            case Bgfx.RendererType.OpenGL:
                GraphicsBackend = GraphicsBackend.OpenGL;
                break;
            case Bgfx.RendererType.Vulkan:
                GraphicsBackend = GraphicsBackend.Vulkan;
                break;
            case Bgfx.RendererType.Metal:
                GraphicsBackend = GraphicsBackend.Metal;
                break;
        }

        init.vendorId = (ushort)Bgfx.PciIdFlags.None;
        init.resolution.width = (uint)_backBufferWidth;
        init.resolution.height = (uint)_backBufferHeight;
        init.resolution.reset = (uint)Bgfx.ResetFlags.Vsync;
        init.resolution.format = Bgfx.TextureFormat.BGRA8;

        Bgfx.Init(&init);

        Bgfx.SetViewRect(0, 0, 0, (ushort)_backBufferWidth, (ushort)_backBufferHeight);
        Bgfx.SetViewClear(0, (ushort)Bgfx.ClearFlags.Color, Color.DodgerBlue.Rgba, 0f, 1);

        #if DEBUG

        Bgfx.SetDebug((uint)Bgfx.DebugFlags.Text);
        Console.WriteLine($"Graphics Initialized:");
        Console.WriteLine($"Backend: {GraphicsBackend}");

        #endif
    }

    internal static void LoadDefaultResources()
    {
        _defaultShader = Content.Get<Shader>("BaseShader");

        if (_defaultShader == null)
        {
            throw new ApplicationException("Could not get BaseShader from Base Pak");
        }

        var pixmap = new Pixmap(1, 1);

        Blitter.Begin(pixmap);

        Blitter.SetColor(Color.White);
        Blitter.Fill();

        Blitter.End();

        _primitiveTexture = Texture2D.Create("primitiveTexture", 1, 1);
        _drawTexture = _primitiveTexture;
    }

    public static void SetBackbufferSize(int width, int height)
    {
        _backBufferWidth = width;
        _backBufferHeight = height;
        _graphicsFlagsChanged = true;
    }

    public static void ApplyRenderView(RenderView view)
    {
        _currentViewId = view.Id;
        Bgfx.SetViewClear(view.Id, (ushort)(Bgfx.ClearFlags.Color | Bgfx.ClearFlags.Depth), view.ClearColor, 1f, 0);
        Bgfx.SetViewRect(view.Id, (ushort)view.ViewRect.Left, (ushort)view.ViewRect.Top, (ushort)view.ViewRect.Width, (ushort)view.ViewRect.Height);
        Bgfx.SetViewScissor(view.Id, (ushort)view.ViewRect.Left, (ushort)view.ViewRect.Top, (ushort)view.ViewRect.Width, (ushort)view.ViewRect.Height);
        Bgfx.SetViewTransform(view.Id, Unsafe.AsPointer(ref view._viewMatrix.M11), Unsafe.AsPointer(ref view._projMatrix.M11));
        Bgfx.Touch(view.Id);
    }

    public static void ApplyRenderState(RenderState state)
    {
        _renderState = state.State;
    }

    public static void SetTexture(Texture2D texture = null)
    {
        _drawTexture = texture ?? _primitiveTexture;
    }


    public static void Submit(IMesh mesh, Shader shader = null, PrimitiveType type = PrimitiveType.Triangles)
    {
        if (mesh.VertexCount == 0)
        {
            return;
        }

        shader ??= _defaultShader;

        var renderFlags = _renderState | (Bgfx.StateFlags)type;

        shader!.SetTexture(0, _drawTexture!);

        shader.Apply();

        mesh.Submit();

        Bgfx.SetState((ulong)renderFlags, 0);

        Bgfx.Submit(_currentViewId, shader.Handle, 0, (byte)Bgfx.DiscardFlags.All);
    }

    internal static void Shutdown()
    {
        foreach (var graphicsResource in _graphicsResources)
        {
            graphicsResource.Dispose();
        }

        Bgfx.Shutdown();
    }

    internal static void RegisterGraphicsResource(RenderResource resource)
    {
        _graphicsResources.Add(resource);
    }

    internal static void Present()
    {
        Bgfx.Touch(0);

        Bgfx.Frame(false);

        if (_graphicsFlagsChanged)
        {
            _graphicsFlagsChanged = false;
            Bgfx.Reset((uint)_backBufferWidth, (uint)_backBufferHeight, (uint)_graphicsFlags, Bgfx.TextureFormat.BGRA8);
        }
    }

    private static Bgfx.RendererType GetRendererType()
    {
        return Platform.PlatformId switch
        {
            RunningPlatform.Windows => Bgfx.RendererType.Direct3D11,
            RunningPlatform.Osx => Bgfx.RendererType.Metal,
            RunningPlatform.Linux => Bgfx.RendererType.OpenGL,
            _ => Bgfx.RendererType.OpenGL,
        };
    }

    private static void UpdateGraphicsFlags()
    {
        _graphicsFlagsChanged = true;

        _graphicsFlags = Bgfx.ResetFlags.None;

        if (_vSync)
        {
            _graphicsFlags |= Bgfx.ResetFlags.Vsync;
        }

        if (_msaaLevel != MSAALevel.None)
        {
            switch (_msaaLevel)
            {
                case MSAALevel.Two:
                    _graphicsFlags |= Bgfx.ResetFlags.MsaaX2;
                    break;
                case MSAALevel.Four:
                    _graphicsFlags |= Bgfx.ResetFlags.MsaaX4;
                    break;
                case MSAALevel.Eight:
                    _graphicsFlags |= Bgfx.ResetFlags.MsaaX8;
                    break;
                case MSAALevel.Sixteen:
                    _graphicsFlags |= Bgfx.ResetFlags.MsaaX16;
                    break;
            }
        }
    }
}
