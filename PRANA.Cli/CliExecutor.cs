using PowerArgs;
using PRANA;
using PRANA.Common;

namespace PRANACLI;

public struct BuildActionArgs
{
    [ArgRequired(PromptIfMissing = true), ArgDescription("Game Folder"), ArgPosition(1), ArgShortcut("-g")]
    public string GameFolder { get; set; }
}


[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
internal class CliExecutor
{
    [HelpHook, ArgShortcut("-?"), ArgDescription("Shows Usage Options")]
    public bool Help { get; set; }

    [ArgActionMethod, ArgDescription("Builds Game Assets"), ArgShortcut("b")]
    public void Build(BuildActionArgs args)
    {

        var gameFolderArg = args.GameFolder!;

        try
        {
            var assetsFullPath = Path.Combine(gameFolderArg, ContentProperties.AssetsFolder);

            if (!Directory.Exists(assetsFullPath))
            {
                throw new ApplicationException("Could not find Assets folder");
            }

            AssetBuilder.BuildAssets(assetsFullPath);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}