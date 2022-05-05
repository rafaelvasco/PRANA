namespace PRANA;

public static partial class Input
{
    public delegate void GenericInputEvent();

    internal static void Init()
    {
        InitKeyboard();

        InitMouse();

        InitGamepad();
    }

    internal static void Update()
    {
        if (EnableKeyboard)
        {
            UpdateKbState();
        }

        if (EnableMouse)
        {
            UpdateMouseState();
        }

        if (EnableGamepad && ConnectedGamePads > 0)
        {
            UpdateGpState();
        }
    }
}