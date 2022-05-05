using PRANA.Foundation;

namespace PRANA;

public enum BlendMode : ulong
{
    Solid,
    Mask,
    Add,
    Alpha,
    AlphaPre,
    Multiply,
    Light,
    Invert
}

public struct RenderState
{
    public static RenderState Default => new(BlendMode.Alpha);

    internal Bgfx.StateFlags State { get; private set; }

    private readonly Bgfx.StateFlags _base = Bgfx.StateFlags.WriteRgb |
                                             Bgfx.StateFlags.WriteA |
                                             Bgfx.StateFlags.WriteZ |
                                             Bgfx.StateFlags.DepthTestLequal |
                                             Bgfx.StateFlags.Msaa;

    private Bgfx.StateFlags _blendState;

    public RenderState(BlendMode mode)
    {
        _blendState = Bgfx.StateFlags.None;
        State = _base;
        SetBlendMode(mode);
    }

    public void SetBlendMode(BlendMode mode)
    {
        switch (mode)
        {
            case BlendMode.Solid:
                _blendState = 0x0;
                break;
            case BlendMode.Mask:
                _blendState = Bgfx.StateFlags.BlendAlphaToCoverage;
                break;
            case BlendMode.Add:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendSrcAlpha, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendOne);
                break;
            case BlendMode.Alpha:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendSrcAlpha, Bgfx.StateFlags.BlendInvSrcAlpha, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendInvSrcAlpha);
                break;
            case BlendMode.AlphaPre:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendInvSrcAlpha);
                break;
            case BlendMode.Multiply:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendDstColor, Bgfx.StateFlags.BlendZero);
                break;
            case BlendMode.Light:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendDstColor, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendZero, Bgfx.StateFlags.BlendOne);
                break;
            case BlendMode.Invert:
                _blendState = Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendInvDstColor, Bgfx.StateFlags.BlendInvSrcColor);
                break;
        }

        State = _base | _blendState;

    }
}