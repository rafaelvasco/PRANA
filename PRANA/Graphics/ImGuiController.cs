using ImGuiNET;
using PRANA.Common;

namespace PRANA;

public unsafe class ImGuiController : IDisposable
{
    private readonly Shader _shader;
    private RenderView _viewport;

    private readonly Dictionary<IntPtr, Texture2D> _loadedTextures;

    private int _textureId;
    private IntPtr? _fontTextureId;

    private int _scrollWheelValue;

    private readonly VertexLayout _vertexLayout;

    private readonly List<int> _keys = new();

    private readonly IntPtr _imguiContext;

    private readonly int _vertexByteSize = sizeof(ImDrawVert);

    private Texture2D _imguiTexture;


    public ImGuiController()
    {
        _imguiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(_imguiContext);

        _loadedTextures = new Dictionary<IntPtr, Texture2D>();

        _viewport = Graphics.CreateDefaultView();

        _shader = Content.Get<Shader>("ImGui");

        _vertexLayout = new VertexLayout();

        _vertexLayout.Begin();

        _vertexLayout.Add(VertexAttribute.Position, VertexAttributeType.Float, 2, false, false);
        _vertexLayout.Add(VertexAttribute.Texture0, VertexAttributeType.Float, 2, false, false);
        _vertexLayout.Add(VertexAttribute.Color0, VertexAttributeType.UInt8, 4, true, false);

        _vertexLayout.End();

        RebuildFontAtlas();

        SetupInput();
    }

    private void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out _);

        if (_imguiTexture != null)
        {
            Content.Free(_imguiTexture);
        }

        _imguiTexture = Texture2D.Create("imguiTexture", (IntPtr)pixelData, width, height);

        if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

        _fontTextureId = BindTexture(_imguiTexture);

        io.Fonts.SetTexID(_fontTextureId.Value);
        io.Fonts.ClearTexData(); // Clears CPU side texture data
    }

    public IntPtr BindTexture(Texture2D texture)
    {
        var id = new IntPtr(_textureId++);

        _loadedTextures.Add(id, texture);

        return id;
    }

    public void UnbindTexture(IntPtr textureId)
    {
        _loadedTextures.Remove(textureId);
    }

    public void Begin(GameTime gameTime)
    {
        PerFrameUpdate(gameTime);

        ImGui.NewFrame();
    }

    public void End()
    {
        ImGui.Render();

        RenderDrawData(ImGui.GetDrawData());
    }


    private void SetupInput()
    {
        var io = ImGui.GetIO();

        _keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab);
        _keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left);
        _keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right);
        _keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up);
        _keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down);
        _keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp);
        _keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home);
        _keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Key.End);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Back);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Space] = (int)Key.Space);
        _keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Key.A);
        _keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Key.C);
        _keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Key.V);
        _keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Key.X);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y);
        _keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z);

        Input.OnTextInput += args =>
        {
            Console.WriteLine($"Char Input: {args.Character}");

            if (args.Character == '\t')
            {
                return;
            }

            io.AddInputCharacter(args.Character);
        };

        ImGui.GetIO().Fonts.AddFontDefault();
    }


    private void PerFrameUpdate(GameTime gameTime)
    {
        var io = ImGui.GetIO();

        ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < _keys.Count; i++)
        {
            io.KeysDown[_keys[i]] = Input.KeyDown((Key)_keys[i]);
        }

        io.KeyShift = Input.KeyDown(Key.LeftShift) || Input.KeyDown(Key.RightShift);
        io.KeyCtrl = Input.KeyDown(Key.LeftControl) || Input.KeyDown(Key.RightControl);
        io.KeyAlt = Input.KeyDown(Key.LeftAlt) || Input.KeyDown(Key.RightAlt);
        io.KeySuper = Input.KeyDown(Key.LeftWindows) || Input.KeyDown(Key.RightWindows);

        io.DisplaySize = new System.Numerics.Vector2(Graphics.BackBufferWidth, Graphics.BackBufferHeight);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

        io.MousePos = new System.Numerics.Vector2(Input.Mouse.X, Input.Mouse.Y);

        io.MouseDown[0] = Input.Mouse.Left;
        io.MouseDown[1] = Input.Mouse.Right;
        io.MouseDown[2] = Input.Mouse.Middle;

        var scrollDelta = Input.Mouse.ScrollWheelValue - _scrollWheelValue;
        io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
        _scrollWheelValue = Input.Mouse.ScrollWheelValue;
    }

    /// <summary>
    /// Gets the geometry as set up by ImGui and sends it to the graphics device
    /// </summary>
    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        var io = ImGui.GetIO();

        Graphics.ApplyRenderState(RenderState.Default);

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        _viewport.ProjectionMatrix =
            Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
        _viewport.ViewRect = new Rectangle(0, 0, Graphics.BackBufferWidth, Graphics.BackBufferHeight);

        Graphics.ApplyRenderView(_viewport);

        RenderCommandLists(drawData);
    }


    private void RenderCommandLists(ImDrawDataPtr drawData)
    {
        TransientIndexBuffer idxBuffer = default;
        TransientVertexBuffer vtxBuffer = default;

        void UpdateBuffers()
        {
            if (drawData.TotalVtxCount == 0)
            {
                return;
            }


            int vtxOffset = 0;
            int idxOffset = 0;

            if (!Graphics.AllocateTransientBuffers(drawData.TotalVtxCount, _vertexLayout, drawData.TotalIdxCount,
                    out vtxBuffer, out idxBuffer))
            {
                throw new ApplicationException("Could not allocate graphics data for ImGui rendering");
            }

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                void* vtxDstPtr = vtxBuffer.Handle.data + (vtxOffset * _vertexByteSize);
                void* idxDstPtr = idxBuffer.Handle.data + (idxOffset * sizeof(ushort));

                Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, drawData.TotalVtxCount * _vertexByteSize,
                    cmdList.VtxBuffer.Size * _vertexByteSize);
                Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, drawData.TotalIdxCount * sizeof(ushort),
                    cmdList.IdxBuffer.Size * sizeof(ushort));

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

        }


        if (drawData.TotalVtxCount == 0)
        {
            return;
        }

        UpdateBuffers();

        int vtxOffset = 0;
        int idxOffset = 0;

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdListsRange[n];

            for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
            {
                ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

                if (drawCmd.ElemCount == 0)
                {
                    continue;
                }

                if (!_loadedTextures.ContainsKey(drawCmd.TextureId))
                {
                    throw new InvalidOperationException(
                        $"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                }


                Graphics.SetClip(
                    (int)drawCmd.ClipRect.X,
                    (int)drawCmd.ClipRect.Y,
                    (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                    (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                );

                Graphics.Submit(
                    ref vtxBuffer,
                    ref idxBuffer,
                    cmdList.VtxBuffer.Size,
                    (int)drawCmd.ElemCount,
                    (int)(drawCmd.IdxOffset + idxOffset),
                    (int)(drawCmd.VtxOffset + vtxOffset),
                    _loadedTextures[drawCmd.TextureId],
                    _shader
                );
            }

            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ImGui.DestroyContext(_imguiContext);
    }
}