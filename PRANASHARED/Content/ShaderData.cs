namespace PRANA;

public struct ShaderData
{
    public string Id { get; set; }

    public byte[] VertexShader { get; set; }

    public byte[] FragmentShader { get; set; }

    public string[] Samplers { get; set; }

    public string[] Params { get; set; }

    public string Datetime { get; set; }
}
