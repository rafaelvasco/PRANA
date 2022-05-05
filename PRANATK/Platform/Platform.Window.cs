using static PRANA.Foundation.SDL;

namespace PRANA;

internal static partial class Platform
{
    public static IntPtr WindowHandle { get; private set; }

    private static uint _windowId;


    public static Action MouseEnterWindow;
    public static Action MouseLeaveWindow;
    public static Action<Size> WindowResized;
    public static Action LostFocus;
    public static Action GainedFocus;
    public static Action Minimized;
    public static Action Restored;

    public static void ProcessWindowEvent(SDL_Event ev)
    {
        switch (ev.window.windowEvent)
        {
            case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:

                var newW = ev.window.data1;
                var newH = ev.window.data2;
                WindowResized!.Invoke(new Size(newW, newH));
                break;

            case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                OnQuit!.Invoke();
                break;

            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                LostFocus?.Invoke();
                break;

            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                GainedFocus?.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                Minimized!.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                Restored!.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                MouseEnterWindow!.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                MouseLeaveWindow!.Invoke();
                break;
        }
    }

    public static void CreateWindow(GameSettings settings)
    {
        var windowFlags =
            SDL_WindowFlags.SDL_WINDOW_HIDDEN |
            SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
            SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS |
            SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

        if (settings.StartFullscreen)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }

        switch (settings.WindowBorderMode)
        {
            case BorderMode.Resizable:
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
                break;
            case BorderMode.Invisible:
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
                break;
        }

        WindowHandle = SDL_CreateWindow(
            settings.WindowTitle,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            settings.WindowSize.Width,
            settings.WindowSize.Height,
            windowFlags);

        if (WindowHandle == IntPtr.Zero)
        {
            throw new ApplicationException("Error while creating SDL2 Window");
        }

        _windowId = SDL_GetWindowID(WindowHandle);
    }

    public static void ShowWindow(bool show)
    {
        if (show)
        {
            SDL_ShowWindow(WindowHandle);
        }
        else
        {
            SDL_HideWindow(WindowHandle);
        }
    }

    public static IntPtr GetRenderSurfaceHandle()
    {
        var info = new SDL_SysWMinfo();

        SDL_GetWindowWMInfo(WindowHandle, ref info);

        switch (PlatformId)
        {
            case RunningPlatform.Windows:
                return info.info.win.window;

            case RunningPlatform.Linux:
                return info.info.x11.window;

            case RunningPlatform.Osx:
                return info.info.cocoa.window;
            case RunningPlatform.Unknown:
                break;
        }

        throw new ApplicationException(
            "Could not retrive native renderer surface handle.");
    }

    public static bool IsFullscreen()
    {
        return GetWindowFlags().HasFlag(WindowFlags.Fullscreen);
    }

    public static void SetWindowFullscreen(bool fullscreen)
    {
        if (IsFullscreen() != fullscreen)
        {
            _ = SDL_SetWindowFullscreen(WindowHandle, (uint)(fullscreen ? WindowFlags.FullscreenDesktop : 0));
        }
    }

    public static void SetWindowSize(int width, int height)
    {
        if (IsFullscreen())
        {
            return;
        }

        SDL_SetWindowSize(WindowHandle, width, height);
        SDL_SetWindowPosition(WindowHandle, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED);
    }

    public static (int width, int height) GetWindowSize()
    {
        SDL_GetWindowSize(WindowHandle, out var w, out var h);
        return (w, h);
    }

    public static void SetWindowBorderless(bool borderless)
    {
        SDL_SetWindowBordered(WindowHandle, borderless ? SDL_bool.SDL_FALSE : SDL_bool.SDL_TRUE);
    }

    public static void SetWindowResizable(bool resizable)
    {
        SDL_SetWindowResizable(WindowHandle, resizable ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
    }

    public static void SetWindowTitle(string title)
    {
        SDL_SetWindowTitle(WindowHandle, title);
    }

    public static string GetWindowTitle()
    {
        return SDL_GetWindowTitle(WindowHandle);
    }

    public static void ShowCursor(bool show)
    {
        _ = SDL_ShowCursor(show ? 1 : 0);
    }

    public static bool CursorVisible()
    {
        var state = SDL_ShowCursor(SDL_QUERY);
        return state == SDL_ENABLE;
    }

    private static void DestroyWindow()
    {
        if (WindowHandle != IntPtr.Zero)
        {
            SDL_DestroyWindow(WindowHandle);
        }
    }

    public static WindowFlags GetWindowFlags()
    {
        return (WindowFlags)SDL_GetWindowFlags(WindowHandle);
    }

    [Flags]
    public enum WindowFlags
    {
        Fullscreen = 0x00000001,
        Shown = 0x00000004,
        Hidden = 0x00000008,
        Borderless = 0x00000010,
        Resizable = 0x00000020,
        Minimized = 0x00000040,
        Maximized = 0x00000080,
        InputFocus = 0x00000200,
        MouseFocus = 0x00000400,
        FullscreenDesktop = 0x00001001,
        AllowHighDPI = 0x00002000,
        MouseCapture = 0x00004000
    }
}