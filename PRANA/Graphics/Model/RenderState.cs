using PRANA.Foundation.BGFX;

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

    private const Bgfx.StateFlags _base = 
        Bgfx.StateFlags.WriteRgb | 
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
        _blendState = mode switch
        {
            BlendMode.Solid => 0x0,
            BlendMode.Mask => Bgfx.StateFlags.BlendAlphaToCoverage,
            BlendMode.Add => Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendSrcAlpha,
                Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendOne),
            BlendMode.Alpha => Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendSrcAlpha,
                Bgfx.StateFlags.BlendInvSrcAlpha, Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendInvSrcAlpha),
            BlendMode.AlphaPre =>
                Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendInvSrcAlpha),
            BlendMode.Multiply => Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendDstColor, Bgfx.StateFlags.BlendZero),
            BlendMode.Light => Bgfx.BGFX_STATE_BLEND_FUNC_SEPARATE(Bgfx.StateFlags.BlendDstColor,
                Bgfx.StateFlags.BlendOne, Bgfx.StateFlags.BlendZero, Bgfx.StateFlags.BlendOne),
            BlendMode.Invert => Bgfx.BGFX_STATE_BLEND_FUNC(Bgfx.StateFlags.BlendInvDstColor,
                Bgfx.StateFlags.BlendInvSrcColor),
            _ => _blendState
        };

        State = _base | _blendState;
    }
}