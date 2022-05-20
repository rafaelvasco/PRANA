namespace PRANA;

internal struct ShaderCompileResult
{
    public readonly byte[] VsBytes;
    public readonly byte[] FsBytes;
    public readonly string[] Samplers;
    public readonly string[] Params;

    public ShaderCompileResult(byte[] vsBytes, byte[] fsBytes, string[] samplers, string[] @params)
    {
        this.VsBytes = vsBytes;
        this.FsBytes = fsBytes;
        this.Samplers = samplers;
        this.Params = @params;
    }
}