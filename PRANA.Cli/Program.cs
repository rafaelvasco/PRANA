using PowerArgs;

namespace PRANACLI;

internal class Program
{
    private static void Main(string[] args)
    {
        Args.InvokeAction<CliExecutor>(args);
    }
}
