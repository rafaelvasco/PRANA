using ImGuiNET;

namespace PRANA;

internal unsafe class ImGuiRenderer
{
    private readonly Shader _shader;

    private readonly Dictionary<IntPtr, Texture2D> _loadedTextures;

    private int _textureId;
    private IntPtr? _fontTextureId;

    private readonly VertexLayout _vertexLayout;

    private readonly int _vertexByteSize = sizeof(ImDrawVert);

    private Texture2D _imguiTexture;


    public ImGuiRenderer()
    {
        _loadedTextures = new Dictionary<IntPtr, Texture2D>();

        _shader = Content.Get<Shader>("ImGui");

        _vertexLayout = new VertexLayout();

        _vertexLayout.Begin();

        _vertexLayout.Add(VertexAttribute.Position, VertexAttributeType.Float, 2, false, false);
        _vertexLayout.Add(VertexAttribute.Texture0, VertexAttributeType.Float, 2, false, false);
        _vertexLayout.Add(VertexAttribute.Color0, VertexAttributeType.UInt8, 4, true, false);

        _vertexLayout.End();

        RebuildFontAtlas();
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
        io.Fonts.ClearTexData();
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

    public void Present()
    {
        ImGui.Render();

        RenderDrawData(ImGui.GetDrawData());
    }

    /// <summary>
    /// Gets the geometry as set up by ImGui and sends it to the graphics device
    /// </summary>
    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.TotalVtxCount == 0)
        {
            return;
        }

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        RenderCommandLists(drawData);
    }


    private void RenderCommandLists(ImDrawDataPtr drawData)
    {
        TransientIndexBuffer idxBuffer;
        TransientVertexBuffer vtxBuffer;

        void UpdateBuffers()
        {
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
}