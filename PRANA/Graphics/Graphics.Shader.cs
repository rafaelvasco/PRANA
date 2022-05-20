using PRANA.Foundation.BGFX;

namespace PRANA;

public static unsafe partial class Graphics
{
    internal static Shader CreateShader(byte[] vertexSource, byte[] fragSource, string[] samplers, string[] parameters)
    {
        static Bgfx.ShaderHandle CreateShaderSource(byte[] bytes)
        {
            var data = Bgfx.AllocGraphicsMemoryBuffer<byte>(bytes);
            var handle = Bgfx.CreateShader(data);
            return handle;
        }

        if (vertexSource.Length == 0 || fragSource.Length == 0)
        {
            throw new ArgumentException("Cannot create a Shader with Empty sources");
        }

        var vertexShader = CreateShaderSource(vertexSource);
        var fragShader = CreateShaderSource(fragSource);

        if (!vertexShader.Valid || !fragShader.Valid)
        {
            throw new ApplicationException("Could not load shader sources");
        }

        var program = Bgfx.CreateProgram(vertexShader, fragShader, _destroyShaders: true);

        var shaderSamples = new ShaderSampler[samplers.Length];

        for (int i = 0; i < samplers.Length; ++i)
        {
            var samplerHandle = Bgfx.CreateUniform(samplers[i], Bgfx.UniformType.Sampler, 1);
            shaderSamples[i] = new ShaderSampler(samplerHandle);
        }

        var shaderParameters = new ShaderParameter[parameters.Length];

        for (int i = 0; i < parameters.Length; ++i)
        {
            var parameterHandle = Bgfx.CreateUniform(parameters[i], Bgfx.UniformType.Vec4, 4);
            shaderParameters[i] = new ShaderParameter(parameterHandle, parameters[i]);
        }

        var shader = new Shader(program, shaderSamples, shaderParameters);

        return shader;
    }

    internal static void DisposeShader(Shader shader)
    {
        if (shader.Samplers != null)
        {
            for (int i = 0; i < shader.Samplers.Length; ++i)
            {
                Bgfx.DestroyUniform(shader.Samplers[i].Handle);
            }
        }

        if (shader.Parameters != null)
        {
            for (int i = 0; i < shader.Parameters.Length; ++i)
            {
                Bgfx.DestroyUniform(shader.Parameters[i].Handle);
            }
        }
    }

    internal static void SubmitShader()
    {
        var shader = _currentShader;

        for (int i = 0; i <= shader.TextureSlotCount; ++i)
        {
            var sampler = shader.Samplers[i];

            if (sampler.Texture != null)
            {
                Bgfx.SetTexture((byte)i, sampler.Handle, sampler.Texture.Handle, (uint)sampler.Texture.SamplerFlags);
            }
            else
            {
                throw new ApplicationException("Submitting shader with empty sampler texture");
            }

        }

        if (shader.Parameters.Length == 0)
        {
            return;
        }

        for (int i = 0; i < shader.Parameters.Length; ++i)
        {
            var p = shader.Parameters[i];

            if (p.Constant)
            {
                if (p.SubmitedOnce)
                {
                    continue;
                }

                p.SubmitedOnce = true;

            }

            var val = p.Value;

            Bgfx.SetUniform(shader.Samplers[i].Handle, &val, 1);
        }
    }
}