using PRANA.Common;
using PRANA.Foundation.BGFX;

namespace PRANA;

public class ShaderParameter
{
    internal Bgfx.UniformHandle Handle { get; }

    public string Name { get; }

    public bool Constant { get; set; } = false;

    internal bool SubmitedOnce;

    public Vector4 Value => _value;

    private Vector4 _value;


    internal ShaderParameter(Bgfx.UniformHandle handle, string name)
    {
        Name = name;
        Handle = handle;
    }

    public void SetValue(float v)
    {
        _value.X = v;
    }

    public void SetValue(Vector2 v)
    {
        _value.X = v.X;
        _value.Y = v.Y;
    }

    public void SetValue(Vector3 v)
    {
        _value.X = v.X;
        _value.Y = v.Y;
        _value.Z = v.Z;
    }

    public void SetValue(Vector4 v)
    {
        _value = v;
    }

    public void SetValue(Color color)
    {
        _value.X = color.R/255f;
        _value.Y = color.G/255f;
        _value.Z = color.B/255f;
        _value.W = color.A/255f;
    }
}

public class ShaderSampler
{
    internal Bgfx.UniformHandle Handle { get; }

    public Texture2D Texture { get; internal set; }

    internal ShaderSampler(Bgfx.UniformHandle handle)
    {
        Handle = handle;
        Texture = null;
    }
}

public class Shader : Asset
{
    internal Bgfx.ProgramHandle Handle { get; }

    internal ShaderSampler[] Samplers { get; }

    internal ShaderParameter[] Parameters { get; }

    internal int TextureSlotCount => _textureIndex;

    private readonly Dictionary<string, int> _paramsMap;

    private int _textureIndex;


    internal Shader(Bgfx.ProgramHandle handle, ShaderSampler[] samplers, ShaderParameter[] parameters)
    {
        Handle = handle;
        Samplers = samplers;
        Parameters = parameters;

        _paramsMap = new Dictionary<string, int>();

        for (int i = 0; i < parameters.Length; ++i)
        {
            _paramsMap.Add(parameters[i].Name, i);
        }
    }

    internal void SetTexture(int slot, Texture2D texture)
    {
        slot = Math.Max(slot, 0);

        Samplers[slot].Texture = texture;

        if (slot > _textureIndex)
        {
            _textureIndex = slot;
        }
    }


    public ShaderParameter GetParam(string name)
    {
        return _paramsMap.TryGetValue(name, out var index) ? Parameters[index] : null;
    }

    protected override void FreeUnmanaged()
    {
        if (!Handle.Valid)
        {
            return;
        }

        Graphics.DisposeShader(this);
    }
}
