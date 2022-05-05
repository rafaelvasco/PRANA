using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PRANA;

public struct ShaderCompileResult
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

internal static class ShaderCompiler
{
    private const string CompilerPath = "Binaries/win-x64/shaderc.exe";
    private const string IncludePath = "Compilation";
    private const string SamplerRegexVar = "sampler";
    private const string SamplerRegex = @"SAMPLER2D\s*\(\s*(?<sampler>\w+)\s*\,\s*(?<index>\d+)\s*\)\s*\;";
    private const string ParamRegexVar = "param";
    private const string VecParamRegex = @"uniform\s+vec4\s+(?<param>\w+)\s*\;";

    public static ShaderCompileResult Compile(string vsSrcPath, string fsSrcPath)
    {
        string temp_vs_bin_output = string.Empty;
        string temp_fs_bin_output = string.Empty;

        string vs_build_result = string.Empty;
        string fs_build_result = string.Empty;

        var process_info = new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = CompilerPath
        };

        try
        {
            var vs_args = "--platform windows -p vs_5_0 -O 3 --type vertex -f $path -o $output -i $include";

            vs_args = vs_args.Replace("$path", vsSrcPath);

            temp_vs_bin_output = Path.Combine(Path.GetTempPath(),
                "dx_" + Path.GetFileNameWithoutExtension(vsSrcPath) + ".bin");

            vs_args = vs_args.Replace("$output", temp_vs_bin_output);

            vs_args = vs_args.Replace("$include", IncludePath);

            process_info.Arguments = vs_args;

            var proc_vs = Process.Start(process_info);

            proc_vs?.WaitForExit();

            var output = proc_vs?.ExitCode ?? -1;

            if (output != 0 && output != -1)
            {
                using var reader = proc_vs?.StandardError;
                vs_build_result = reader?.ReadToEnd();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            var fs_args = "--platform windows -p ps_5_0 -O 3 --type fragment -f $path -o $output -i $include";

            fs_args = fs_args.Replace("$path", fsSrcPath);

            temp_fs_bin_output = Path.Combine(Path.GetTempPath(),
                "dx_" + Path.GetFileNameWithoutExtension(fsSrcPath) + ".bin");

            fs_args = fs_args.Replace("$output", temp_fs_bin_output);

            fs_args = fs_args.Replace("$include", IncludePath);

            process_info.Arguments = fs_args;

            var proc_fs = Process.Start(process_info);

            proc_fs?.WaitForExit();

            var output = proc_fs?.ExitCode ?? -1;

            if (output != 0 && output != -1)
            {
                using var reader = proc_fs?.StandardError;
                fs_build_result = reader?.ReadToEnd();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        bool vs_ok = File.Exists(temp_vs_bin_output);
        bool fs_ok = File.Exists(temp_fs_bin_output);

        if (vs_ok && fs_ok)
        {
            var vs_bytes = File.ReadAllBytes(temp_vs_bin_output);
            var fs_bytes = File.ReadAllBytes(temp_fs_bin_output);

            var fs_stream = File.OpenRead(fsSrcPath);

            ParseUniforms(fs_stream, out var samplers, out var @params);

            var result = new ShaderCompileResult(vs_bytes, fs_bytes, samplers, @params);

            File.Delete(temp_vs_bin_output);
            File.Delete(temp_fs_bin_output);

            return result;
        }
        else
        {
            if (vs_ok)
            {
                File.Delete(temp_vs_bin_output);
            }

            if (fs_ok)
            {
                File.Delete(temp_fs_bin_output);
            }

            if (!vs_ok)
            {
                throw new Exception("Error building vertex shader on " + vsSrcPath + " : " + vs_build_result);
            }

            throw new Exception("Error building fragment shader on " + fsSrcPath + " : " + fs_build_result);
        }
    }

    public static void ParseUniforms(Stream fsStream, out string[] samplers, out string[] @params)
    {
        var sampler_regex = new Regex(SamplerRegex);
        var param_regex = new Regex(VecParamRegex);

        var samplers_list = new List<string>();
        var params_list = new List<string>();

        using (var reader = new StreamReader(fsStream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Match sampler_match = sampler_regex.Match(line);


                if (sampler_match.Success)
                {
                    string sampler_name = sampler_match.Groups[SamplerRegexVar].Value;
                    samplers_list.Add(sampler_name);
                }
                else
                {
                    Match param_match = param_regex.Match(line);

                    if (param_match.Success)
                    {
                        string param_name = param_match.Groups[ParamRegexVar].Value;

                        params_list.Add(param_name);
                    }
                }
            }
        }

        samplers = samplers_list.Count > 0 ? samplers_list.ToArray() : Array.Empty<string>();

        @params = params_list.Count > 0 ? params_list.ToArray() : Array.Empty<string>();
    }
}

internal static partial class AssetBuilder
{
    public static ShaderData BuildShader(ShaderManifestInfo shaderManifest, string assetsFolder)
    {
        var vsFullPath = Path.Combine(assetsFolder, shaderManifest.VsPath);

        var fsFullPath = Path.Combine(assetsFolder , shaderManifest.FsPath);

        var compileResult = ShaderCompiler.Compile(vsFullPath, fsFullPath);

        var data = new ShaderData()
        {
            FragmentShader = compileResult.FsBytes,
            VertexShader = compileResult.VsBytes,
            Samplers = compileResult.Samplers,
            Params = compileResult.Params,
            Id = shaderManifest.Id,
            Datetime = DateTime.UtcNow.ToString("O")
        };

        return data;
    }
}