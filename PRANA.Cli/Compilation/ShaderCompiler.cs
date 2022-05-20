using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using PRANA.Common;

namespace PRANA;

internal static class ShaderCompiler
{
    private const string CompilerPath = "Binaries/win-x64/shaderc.exe";
    private const string IncludePath = "Compilation";
    private const string SamplerRegexVar = "sampler";
    private const string SamplerRegex = @"SAMPLER2D\s*\(\s*(?<sampler>\w+)\s*\,\s*(?<index>\d+)\s*\)\s*\;";
    private const string ParamRegexVar = "param";
    private const string VecParamRegex = @"uniform\s+vec4\s+(?<param>\w+)\s*\;";

    private const string D3DCompileParams =
        "--platform windows -p $profile_5_0 -O 3 --type $type -f $path -o $output -i $include";

    private const string GLSLCompileParams =
        "--platform linux --type $type -f $path -o $output -i $include";

    public static ShaderCompileResult Compile(string vsSrcPath, string fsSrcPath, GraphicsBackend graphicsBackend)
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

        var compileParams = graphicsBackend switch
        {
            GraphicsBackend.Direct3D11 => D3DCompileParams,
            GraphicsBackend.OpenGL => GLSLCompileParams,
            _ => throw new ArgumentOutOfRangeException(nameof(graphicsBackend), graphicsBackend, null)
        };

        try
        {
            var vs_args = new StringBuilder(compileParams);

            vs_args.Replace("$path", vsSrcPath);
            vs_args.Replace("$type", "vertex");

            if (graphicsBackend == GraphicsBackend.Direct3D11)
            {
                vs_args.Replace("$profile", "vs");    
            }

            temp_vs_bin_output = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(vsSrcPath) + ".bin");

            vs_args.Replace("$output", temp_vs_bin_output);

            vs_args.Replace("$include", IncludePath);

            process_info.Arguments = vs_args.ToString();

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
            var fs_args = new StringBuilder(compileParams);

            fs_args.Replace("$path", fsSrcPath);
            fs_args.Replace("$type", "fragment");

            if (graphicsBackend == GraphicsBackend.Direct3D11)
            {
                fs_args.Replace("$profile", "ps");    
            }

            temp_fs_bin_output = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(fsSrcPath) + ".bin");

            fs_args.Replace("$output", temp_fs_bin_output);

            fs_args.Replace("$include", IncludePath);

            process_info.Arguments = fs_args.ToString();

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
