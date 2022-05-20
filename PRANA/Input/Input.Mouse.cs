using PRANA.Common;

namespace PRANA;

[Flags]
public enum MouseButton
{
    None = 0,
    Left = 1,
    Middle = 2,
    Right = 3,
}

/// <summary>
/// Represents a mouse state with cursor position and button press information.
/// </summary>
public struct MouseState
{
    /// <summary>
    /// Gets horizontal position of the cursor.
    /// </summary>
    public int X
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets vertical position of the cursor.
    /// </summary>
    public int Y
    {
        get;
        internal set;
    }

    public Vector2 Position => new Vector2(X, Y);

    /// <summary>
    /// Gets state of the left mouse button.
    /// </summary>
    public bool Left
    {
        get;
        internal set;
    }

    public bool LeftPressed
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets state of the right mouse button.
    /// </summary>
    public bool Right
    {
        get;
        internal set;
    }

    public bool RightPressed
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets state of the middle mouse button.
    /// </summary>
    public bool Middle
    {
        get;
        internal set;
    }

    public bool MiddlePressed
    {
        get;
        internal set;
    }

    /// <summary>
    /// Returns cumulative scroll wheel value since the game start.
    /// </summary>
    public int ScrollWheelValue
    {
        get;
        internal set;
    }

    /// <summary>
    /// Initializes a new instance of the MouseState.
    /// </summary>
    /// <param name="x">Horizontal position of the mouse.</param>
    /// <param name="y">Vertical position of the mouse.</param>
    /// <param name="scrollWheel">Mouse scroll wheel's value.</param>
    /// <param name="left">Left mouse button's state.</param>
    /// <param name="middle">Middle mouse button's state.</param>
    /// <param name="right">Right mouse button's state.</param>
    public MouseState(
        int x,
        int y,
        int scrollWheel,
        bool left,
        bool middle,
        bool right
    ) : this()
    {
        X = x;
        Y = y;
        ScrollWheelValue = scrollWheel;
        Left = left;
        Middle = middle;
        Right = right;
    }

    /// <summary>
    /// Compares whether two MouseState instances are equal.
    /// </summary>
    /// <param name="left">MouseState instance on the left of the equal sign.</param>
    /// <param name="right">MouseState instance on the right of the equal sign.</param>
    /// <returns>true if the instances are equal; false otherwise.</returns>
    public static bool operator ==(MouseState left, MouseState right)
    {
        return (left.X == right.X &&
                left.Y == right.Y &&
                left.Left == right.Left &&
                left.Middle == right.Middle &&
                left.Right == right.Right &&
                left.ScrollWheelValue == right.ScrollWheelValue);
    }

    /// <summary>
    /// Compares whether two MouseState instances are not equal.
    /// </summary>
    /// <param name="left">MouseState instance on the left of the equal sign.</param>
    /// <param name="right">MouseState instance on the right of the equal sign.</param>
    /// <returns>true if the objects are not equal; false otherwise.</returns>
    public static bool operator !=(MouseState left, MouseState right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Compares whether current instance is equal to specified object.
    /// </summary>
    /// <param name="obj">The MouseState to compare.</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return (obj is MouseState state) && (this == state);
    }

    public bool Equals(MouseState other)
    {
        return X == other.X && Y == other.Y && Left == other.Left && Right == other.Right && Middle == other.Middle && ScrollWheelValue == other.ScrollWheelValue;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Left, Right, Middle, ScrollWheelValue);
    }


    /// <summary>
    /// Returns a string describing the mouse state.
    /// </summary>
    public override string ToString()
    {
        string buttons = string.Empty;
        if (Left)
        {
            buttons = "Left";
        }
        if (Right )
        {
            if (buttons.Length > 0)
            {
                buttons += " ";
            }
            buttons += "Right";
        }
        if (Middle)
        {
            if (buttons.Length > 0)
            {
                buttons += " ";
            }
            buttons += "Middle";
        }
        if (string.IsNullOrEmpty(buttons))
        {
            buttons = "None";
        }
        return $"[MouseState X={X}, Y={Y}, Buttons={buttons}, Wheel={ScrollWheelValue}]";
    }

}

public static partial class Input
{
    public delegate void MouseInputEvent(MouseButton button);
    public delegate void MouseMotionEvent(int x, int y);

    public static event GenericInputEvent OnMouseEnter;
    public static event GenericInputEvent OnMouseLeave;
    public static event MouseInputEvent OnMouseDown;
    public static event MouseInputEvent OnMouseUp;
    public static event MouseMotionEvent OnMouseMove;

    public static bool IsMouseOver { get; internal set; }

    public static bool ProcessMouseMoveEvents
    {
        get => Platform.ButtonPosEventPoolEnabled;
        set => Platform.ButtonPosEventPoolEnabled = value;
    }

    public static bool EnableMouse { get; set; } = true;

    public static int CurrentMouseWheel => _msState.ScrollWheelValue;

    public static ref MouseState Mouse => ref _msState;

    private static MouseState _msState;

    private static void InitMouse()
    {
        _msState = Platform.GetMouseState();

        Platform.MouseDown = button =>
        {
            OnMouseDown?.Invoke(button);
        };

        Platform.MouseUp = button =>
        {
            OnMouseUp?.Invoke(button);
        };

        Platform.MouseMove = (x, y) =>
        {
            OnMouseMove?.Invoke(x, y);
        };

        Platform.MouseEnterWindow = () =>
        {
            OnMouseEnter?.Invoke();
        };

        Platform.MouseLeaveWindow = () =>
        {
            OnMouseLeave?.Invoke();
        };
    }

    private static void UpdateMouseState()
    {
        var prev = _msState;

        _msState = Platform.GetMouseState();

        _msState.LeftPressed = _msState.Left && !prev.Left;
        _msState.RightPressed = _msState.Right && !prev.Right;
        _msState.MiddlePressed = _msState.Middle && !prev.Middle;
    }
}