using PRANA.Common;

namespace PRANA;

public partial class Game
{
    public delegate void FileDropEventHandler(FileDropEventArgs args);

    public static event FileDropEventHandler OnFileDrop; 


    private static void Platform_WindowResized(Size size)
    {
        Graphics.SetBackbufferSize(size.Width, size.Height);
    }

    private static void Platform_LostFocus()
    {
        Console.WriteLine("Lost Focus");
        IsActive = false;
    }

    private static void Platform_RestoredFocus()
    {
        Console.WriteLine("Regained Focus");
        IsActive = true;
    }

    private static void Platform_OnQuit()
    {
        Exit();
    }

}