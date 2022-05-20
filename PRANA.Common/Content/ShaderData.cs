namespace PRANA.Common;

public readonly struct ShaderData
{
    public string Id { get; init; }

    public byte[] VertexShader { get; init; }

    public byte[] FragmentShader { get; init; }

    public string[] Samplers { get; init; }

    public string[] Params { get; init; }

    public string Datetime { get; init; }
}
