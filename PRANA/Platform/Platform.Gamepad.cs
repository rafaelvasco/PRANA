using PRANA.Common;
using static PRANA.Common.SDL.SDL;

namespace PRANA;

internal static partial class Platform
{
    private static readonly IntPtr[] _gamepadDevices = new IntPtr[Input.GamepadMaxCount];
    private static readonly Dictionary<int, int> GamepadInstances = new();
    private static readonly string[] GamepadGuids = GenStringArray();

    private static readonly string[] GamepadLightBars = GenStringArray();

    private static readonly GamePadState[] GamepadStates = new GamePadState[Input.GamepadMaxCount];
    private static readonly GamePadCapabilities[] GamepadCaps = new GamePadCapabilities[Input.GamepadMaxCount];

    private static readonly GamePadType[] GamepadTypes = {
            GamePadType.Unknown,
            GamePadType.GamePad,
            GamePadType.Wheel,
            GamePadType.ArcadeStick,
            GamePadType.FlightStick,
            GamePadType.DancePad,
            GamePadType.Guitar,
            GamePadType.DrumKit,
            GamePadType.BigButtonPad
    };


    public static GamePadCapabilities GetGamePadCapabilities(int index)
    {
        return _gamepadDevices[index] == IntPtr.Zero ? new GamePadCapabilities() : GamepadCaps[index];
    }

    public static GamePadState GetGamePadState(int index, GamePadDeadZone deadZoneMode)
    {
        if (!Input.EnableGamepad)
        {
            return new GamePadState();
        }

        IntPtr device = _gamepadDevices[index];
        if (device == IntPtr.Zero)
        {
            return new GamePadState();
        }

        GamePadButtons gc_buttonState = 0;

        // Sticks
        var stickLeft = new Vector2(
            SDL_GameControllerGetAxis(
                device,
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
            ) / 32767.0f,
            SDL_GameControllerGetAxis(
                device,
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
            ) / -32767.0f
        );
        var stickRight = new Vector2(
            SDL_GameControllerGetAxis(
                device,
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
            ) / 32767.0f,
            SDL_GameControllerGetAxis(
                device,
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
            ) / -32767.0f
        );

        gc_buttonState |= ConvertStickValuesToButtons(
            stickLeft,
            GamePadButtons.LeftThumbstickLeft,
            GamePadButtons.LeftThumbstickRight,
            GamePadButtons.LeftThumbstickUp,
            GamePadButtons.LeftThumbstickDown,
            Input.LEFT_DEAD_ZONE
        );
        gc_buttonState |= ConvertStickValuesToButtons(
            stickRight,
            GamePadButtons.RightThumbstickLeft,
            GamePadButtons.RightThumbstickRight,
            GamePadButtons.RightThumbstickUp,
            GamePadButtons.RightThumbstickDown,
            Input.RIGHT_DEAD_ZONE
        );

        // Triggers
        float triggerLeft = SDL_GameControllerGetAxis(
            device,
            SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
        ) / 32767.0f;
        float triggerRight = SDL_GameControllerGetAxis(
            device,
            SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
        ) / 32767.0f;
        if (triggerLeft > Input.TRIGGER_THRESHOLD)
        {
            gc_buttonState |= GamePadButtons.LeftTrigger;
        }
        if (triggerRight > Input.TRIGGER_THRESHOLD)
        {
            gc_buttonState |= GamePadButtons.RightTrigger;
        }

        // Buttons
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) != 0)
        {
            gc_buttonState |= GamePadButtons.A;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) != 0)
        {
            gc_buttonState |= GamePadButtons.B;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) != 0)
        {
            gc_buttonState |= GamePadButtons.X;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) != 0)
        {
            gc_buttonState |= GamePadButtons.Y;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) != 0)
        {
            gc_buttonState |= GamePadButtons.Back;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE) != 0)
        {
            gc_buttonState |= GamePadButtons.BigButton;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) != 0)
        {
            gc_buttonState |= GamePadButtons.Start;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) != 0)
        {
            gc_buttonState |= GamePadButtons.LeftStick;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) != 0)
        {
            gc_buttonState |= GamePadButtons.RightStick;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) != 0)
        {
            gc_buttonState |= GamePadButtons.LeftShoulder;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) != 0)
        {
            gc_buttonState |= GamePadButtons.RightShoulder;
        }

        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) != 0)
        {
            gc_buttonState |= GamePadButtons.DPadUp;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) != 0)
        {
            gc_buttonState |= GamePadButtons.DPadDown;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) != 0)
        {
            gc_buttonState |= GamePadButtons.DPadLeft;
        }
        if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) != 0)
        {
            gc_buttonState |= GamePadButtons.DPadRight;
        }

        // Build the GamePadState, increment PacketNumber if state changed.

        GamePadState gc_builtState = new GamePadState(
            new GamePadThumbSticks(stickLeft, stickRight, deadZoneMode),
            new GamePadTriggers(triggerLeft, triggerRight, deadZoneMode),
            gc_buttonState
        )
        {
            IsConnected = true,
            PacketNumber = GamepadStates[index].PacketNumber
        };
        if (gc_builtState != GamepadStates[index])
        {
            gc_builtState.PacketNumber += 1;
            GamepadStates[index] = gc_builtState;
        }

        return gc_builtState;
    }

    public static bool SetGamePadVibration(int index, float leftMotor, float rightMotor)
    {
        if (!Input.EnableGamepad)
        {
            return false;
        }

        IntPtr device = _gamepadDevices[index];
        if (device == IntPtr.Zero)
        {
            return false;
        }

        return SDL_GameControllerRumble(
            device,
            (ushort)(Calc.Clamp(leftMotor, 0.0f, 1.0f) * 0xFFFF),
            (ushort)(Calc.Clamp(rightMotor, 0.0f, 1.0f) * 0xFFFF),
            0
        ) == 0;
    }

    public static string GetGamePadGuid(int index)
    {
        return GamepadGuids[index];
    }

    public static void SetGamePadLightBar(int index, Color color)
    {
        if (!Input.EnableGamepad)
        {
            return;
        }

        if (string.IsNullOrEmpty(GamepadLightBars[index]))
        {
            return;
        }

        string baseDir = GamepadLightBars[index];
        try
        {
            File.WriteAllText(baseDir + "red/brightness", color.R.ToString());
            File.WriteAllText(baseDir + "green/brightness", color.G.ToString());
            File.WriteAllText(baseDir + "blue/brightness", color.B.ToString());
        }
        catch
        {
            // If something went wrong, assume the worst and just remove it.
            GamepadLightBars[index] = string.Empty;
        }
    }

    public static void SetGamePadMappingsFile(string fileContent)
    {
        SDL_GameControllerAddMapping(fileContent);
    }

    public static void PreLookForGamepads()
    {
        var evt = new SDL_Event[1];
        SDL_PumpEvents();
        while (SDL_PeepEvents(
            evt,
            1,
            SDL_eventaction.SDL_GETEVENT,
            SDL_EventType.SDL_CONTROLLERDEVICEADDED,
            SDL_EventType.SDL_CONTROLLERDEVICEADDED
        ) == 1)
        {
            AddGamePadInstance(evt[0].cdevice.which);
        }
    }

    private static void AddGamePadInstance(int deviceId)
    {
        if (Input.ConnectedGamePads == Input.GamepadMaxCount)
        {
            return;
        }

        Console.WriteLine("GamePad Added");

        SDL_ClearError();

        int which = Input.ConnectedGamePads++;

        // Open the device!
        _gamepadDevices[which] = SDL_GameControllerOpen(deviceId);

        // We use this when dealing with GUID initialization.
        IntPtr thisJoystick = SDL_GameControllerGetJoystick(_gamepadDevices[which]);

        int thisInstance = SDL_JoystickInstanceID(thisJoystick);

        GamepadInstances.Add(thisInstance, which);

        // Start with a fresh state.
        GamepadStates[which] = new GamePadState
        {
            IsConnected = true
        };

        // Initialize the haptics for the joystick, if applicable.
        bool hasRumble = SDL_GameControllerRumble(
            _gamepadDevices[which],
            0,
            0,
            0
        ) == 0;

        var caps = new GamePadCapabilities
        {
            IsConnected = true,
            GamePadType = GamepadTypes[(int)SDL_JoystickGetType(thisJoystick)],
            HasAButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasBButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasXButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasYButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasBackButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasBigButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasStartButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftStickButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasRightStickButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftShoulderButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasRightShoulderButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasDPadUpButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasDPadDownButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasDPadLeftButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasDPadRightButton = SDL_GameControllerGetBindForButton(
                _gamepadDevices[which],
                SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftXThumbStick = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftYThumbStick = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasRightXThumbStick = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasRightYThumbStick = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftTrigger = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasRightTrigger = SDL_GameControllerGetBindForAxis(
                _gamepadDevices[which],
                SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
            ).bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE,
            HasLeftVibrationMotor = hasRumble,
            HasRightVibrationMotor = hasRumble,
            HasVoiceSupport = false
        };

        GamepadCaps[which] = caps;

        ushort vendor = SDL_JoystickGetVendor(thisJoystick);
        ushort product = SDL_JoystickGetProduct(thisJoystick);
        if (vendor == 0x00 && product == 0x00)
        {
            GamepadGuids[which] = "xinput";
        }
        else
        {
            GamepadGuids[which] = $"{vendor & 0xFF:x2}{vendor >> 8:x2}{product & 0xFF:x2}{product >> 8:x2}";
        }

        // Initialize light bar
        if (PlatformId == RunningPlatform.Linux &&
            (GamepadGuids[which].Equals("4c05c405") ||
                GamepadGuids[which].Equals("4c05cc09")))
        {
            // Get all of the individual PS4 LED instances
            var ledList = new List<string>();
            string[] dirs = Directory.GetDirectories("/sys/class/leds/");
            foreach (string dir in dirs)
            {
                if (dir.EndsWith("blue") &&
                    (dir.Contains("054C:05C4") ||
                        dir.Contains("054C:09CC")))
                {
                    ledList.Add(dir[..(dir.LastIndexOf(':') + 1)]);
                }
            }
            // Find how many of these are already in use
            int numLights = 0;
            for (int i = 0; i < GamepadLightBars.Length; i += 1)
            {
                if (!string.IsNullOrEmpty(GamepadLightBars[i]))
                {
                    numLights += 1;
                }
            }
            // If all are not already in use, use the first unused light
            if (numLights < ledList.Count)
            {
                GamepadLightBars[which] = ledList[numLights];
            }
        }
    }

    private static void RemoveGamePadInstance(int dev)
    {
        Console.WriteLine("GamePad Removed");
        if (!GamepadInstances.TryGetValue(dev, out int output))
        {
            return;
        }
        GamepadInstances.Remove(dev);
        SDL_GameControllerClose(_gamepadDevices[output]);
        _gamepadDevices[output] = IntPtr.Zero;
        GamepadStates[output] = new GamePadState();
        GamepadGuids[output] = string.Empty;

        SDL_ClearError();

        Input.ConnectedGamePads--;

    }

    private static GamePadButtons ConvertStickValuesToButtons(Vector2 stick, GamePadButtons left, GamePadButtons right, GamePadButtons up, GamePadButtons down, float deadZoneSize)
    {
        GamePadButtons b = 0;

        var (x, y) = stick;
        if (x > deadZoneSize)
        {
            b |= right;
        }
        if (x < -deadZoneSize)
        {
            b |= left;
        }
        if (y > deadZoneSize)
        {
            b |= up;
        }
        if (y < -deadZoneSize)
        {
            b |= down;
        }

        return b;
    }

    private static string[] GenStringArray()
    {
        string[] result = new string[Input.GamepadMaxCount];
        for (int i = 0; i < result.Length; i += 1)
        {
            result[i] = string.Empty;
        }
        return result;
    }

    private static void InitGamePad()
    {
        string hint = SDL_GetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS);
        if (string.IsNullOrEmpty(hint))
        {
            SDL_SetHint(
                SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS,
                "1"
            );
        }

        SDL_SetHintWithPriority(
            SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS,
            "0",
            SDL_HintPriority.SDL_HINT_OVERRIDE
        );
    }

    private static void ProcessGamePadEvent(SDL_Event evt)
    {
        if (!Input.EnableGamepad)
        {
            return;
        }

        switch (evt.type)
        {
            case SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                AddGamePadInstance(evt.cdevice.which);
                break;
            case SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                RemoveGamePadInstance(evt.cdevice.which);
                break;
        }
    }
}