using System.Runtime.CompilerServices;
using PRANA.Common;
using PRANA.Foundation.BGFX;

namespace PRANA;

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

    public static int BackBufferWidth => _backBufferWidth;

    public static int BackBufferHeight => _backBufferHeight;

    public static RenderView CreateDefaultView()
    {
        var defaultView = new RenderView()
        {
            ClearColor = Color.Black,
            ViewRect = new Rectangle(0, 0, _backBufferWidth, _backBufferHeight),
            ProjectionMatrix =
                Matrix.CreateOrthographicOffCenter(0f, _backBufferWidth, _backBufferHeight, 0f, 0.0f, 1000.0f),
            Transform = Matrix.Identity
        };

        return defaultView;
    }

    private static int _backBufferWidth;

    private static int _backBufferHeight;

    private static MSAALevel _msaaLevel = MSAALevel.None;

    private static bool _vSync = true;

    private static RenderState _renderState = RenderState.Default;

    private static Bgfx.ResetFlags _graphicsFlags = Bgfx.ResetFlags.None;

    private static bool _graphicsFlagsChanged;

    private static ushort _currentViewId;

    private static Texture2D _primitiveTexture;

    private static Shader _defaultShader;

    private static Shader _currentShader;

    private static readonly List<RenderResource> _renderResources = new(16);


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

        GraphicsBackend = init.type switch
        {
            Bgfx.RendererType.Direct3D11 => GraphicsBackend.Direct3D11,
            Bgfx.RendererType.Direct3D12 => GraphicsBackend.Direct3D12,
            Bgfx.RendererType.OpenGL => GraphicsBackend.OpenGL,
            Bgfx.RendererType.Vulkan => GraphicsBackend.Vulkan,
            Bgfx.RendererType.Metal => GraphicsBackend.Metal,
            _ => GraphicsBackend
        };

        init.vendorId = (ushort)Bgfx.PciIdFlags.None;
        init.resolution.width = (uint)_backBufferWidth;
        init.resolution.height = (uint)_backBufferHeight;
        init.resolution.format = Bgfx.TextureFormat.BGRA8;

        Bgfx.Init(&init);

        Bgfx.SetViewRect(0, 0, 0, (ushort)_backBufferWidth, (ushort)_backBufferHeight);
        Bgfx.SetViewClear(0, (ushort)Bgfx.ClearFlags.Color, Color.Black.Rgba, 0f, 1);

#if DEBUG

        Bgfx.SetDebug((uint)Bgfx.DebugFlags.Text);

#endif
    }

    internal static void LoadDefaultResources()
    {
        _defaultShader = AssetLoader.LoadEmbedded<Shader>("BaseSprite");

        _currentShader = _defaultShader;

        AssetLoader.LoadEmbedded<Texture2D>("Logo");
        AssetLoader.LoadEmbedded<Font>("MonoGram");
        AssetLoader.LoadEmbedded<Shader>("ImGui");

        _primitiveTexture = Texture2D.Create("primitiveTexture", 1, 1);

        _primitiveTexture.SetPixels(new byte[] { 255, 255, 255, 255 });
    }

    public static void SetBackbufferSize(int width, int height)
    {
        _backBufferWidth = width;
        _backBufferHeight = height;
        _graphicsFlagsChanged = true;
    }

    public static void SetClip(int x, int y, int w, int h)
    {
        Bgfx.SetScissor((ushort)x, (ushort)y, (ushort)w, (ushort)h);
    }

    public static void SetClip()
    {
        Bgfx.SetScissor(0, 0, 0, 0);
    }


    public static void ApplyRenderView(RenderView view)
    {
        _currentViewId = view.Id;
        Bgfx.SetViewClear(view.Id, (ushort)(Bgfx.ClearFlags.Color | Bgfx.ClearFlags.Depth), view.ClearColor.Rgba, 1f,
            1);
        Bgfx.SetViewRect(view.Id, (ushort)view.ViewRect.Left, (ushort)view.ViewRect.Top, (ushort)view.ViewRect.Width,
            (ushort)view.ViewRect.Height);
        Bgfx.SetViewScissor(view.Id, (ushort)view.ViewRect.Left, (ushort)view.ViewRect.Top, (ushort)view.ViewRect.Width,
            (ushort)view.ViewRect.Height);
        Bgfx.SetViewTransform(view.Id, Unsafe.AsPointer(ref view.Transform.M11),
            Unsafe.AsPointer(ref view.ProjectionMatrix.M11));
        Bgfx.Touch(view.Id);
    }

    public static void ApplyRenderState(RenderState state)
    {
        _renderState = state;
    }

    public static void SetModelTransform(ref Matrix transform)
    {
        Bgfx.SetTransform(Unsafe.AsPointer(ref transform.M11), 1);
    }

    private static void SetTexture(int slot, Texture2D texture)
    {
        _currentShader.SetTexture(slot, texture ?? _primitiveTexture);
    }

    public static void SetShader(Shader shader)
    {
        _currentShader = shader ?? _defaultShader;
    }

    public static void Submit(
        IVertexStream vertexStream,
        Shader shader = null,
        Texture2D texture = null)
    {
        if (vertexStream.VertexCount == 0)
        {
            return;
        }

        SetShader(shader);

        SetTexture(0, texture);

        SubmitShader();

        vertexStream.Submit();

        Bgfx.SetState((ulong)_renderState.State, 0);

        Bgfx.Submit(_currentViewId, _currentShader.Handle, 0, (byte)Bgfx.DiscardFlags.All);
    }

    public static void Submit(
        ref TransientVertexBuffer vertexBuffer,
        ref TransientIndexBuffer indexBuffer,
        int numVertices,
        int numIndices,
        int indexOffset,
        int vertexOffset,
        Texture2D texture = null,
        Shader shader = null
    )
    {
        SetShader(shader);

        SetTexture(0, texture);

        SubmitShader();

        var idxHandle = indexBuffer.Handle;
        var vtxHandle = vertexBuffer.Handle;

        Bgfx.SetTransientIndexBuffer(&idxHandle, (uint)indexOffset, (uint)numIndices);
        Bgfx.SetTransientVertexBuffer(0, &vtxHandle, (uint)vertexOffset, (uint)numVertices);

        Bgfx.SetState((ulong)_renderState.State, 0);

        Bgfx.Submit(_currentViewId, _currentShader.Handle, 0, (byte)Bgfx.DiscardFlags.All);
    }

    public static void SubmitRange(
        IVertexStream vertexStream,
        Shader shader = null,
        Texture2D texture = null,
        int startingVertexIndex = 0,
        int vertexCount = -1,
        int startingIndiceIndex = 0,
        int indexCount = -1)
    {
        if (vertexStream.VertexCount == 0)
        {
            return;
        }

        SetShader(shader);

        SetTexture(0, texture);

        SubmitShader();

        vertexStream.Submit(startingVertexIndex, vertexCount, startingIndiceIndex, indexCount);

        Bgfx.SetState((ulong)_renderState.State, 0);

        Bgfx.Submit(_currentViewId, _currentShader.Handle, 0, (byte)Bgfx.DiscardFlags.All);
    }

    internal static void Shutdown()
    {
        foreach (var renderResource in _renderResources)
        {
            Console.WriteLine($"Disposing RenderResource: {renderResource.Id}");
            renderResource.Dispose();
        }

        Bgfx.Shutdown();
    }

    internal static void RegisterRenderResource(RenderResource resource)
    {
        _renderResources.Add(resource);
    }

    internal static void Present()
    {
        Bgfx.Touch(0);

        _ = Bgfx.Frame(false);

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