namespace PRANA.Common;

/// <summary>
/// Describes a float type 2D-rectangle.
/// </summary>
[Serializable]
public struct RectangleF : IEquatable<RectangleF>
{
    #region Public Properties

    /// <summary>
    /// Returns the x coordinate of the left edge of this <see cref="Rectangle"/>.
    /// </summary>
    public float Left => X;

    /// <summary>
    /// Returns the x coordinate of the right edge of this <see cref="Rectangle"/>.
    /// </summary>
    public float Right => (X + Width);

    /// <summary>
    /// Returns the y coordinate of the top edge of this <see cref="Rectangle"/>.
    /// </summary>
    public float Top => Y;

    /// <summary>
    /// Returns the y coordinate of the bottom edge of this <see cref="Rectangle"/>.
    /// </summary>
    public float Bottom => (Y + Height);

    public Vector2 TopLeft => new (X, Y);

    public Vector2 TopRight => new (X + Width, Y);

    public Vector2 BottomLeft => new (X, Y + Height);

    public Vector2 BottomRight => new (X + Width, Y + Height);

    /// <summary>
    /// The top-left coordinates of this <see cref="Rectangle"/>.
    /// </summary>
    public Vector2 Location
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    /// A <see cref="Point"/> located in the center of this <see cref="Rectangle"/>'s bounds.
    /// </summary>
    /// <remarks>
    /// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
    /// the center point will be rounded down.
    /// </remarks>
    public Vector2 Center =>
        new(
            X + (Width / 2),
            Y + (Height / 2)
        );

    /// <summary>
    /// Whether or not this <see cref="Rectangle"/> has a width and
    /// height of 0, and a position of (0, 0).
    /// </summary>
    public bool IsEmpty =>
        (	(Width == 0) &&
            (Height == 0) &&
            (X == 0) &&
            (Y == 0)	);

    #endregion

    #region Public Static Properties

    /// <summary>
    /// Returns a <see cref="Rectangle"/> with X=0, Y=0, Width=0, and Height=0.
    /// </summary>
    public static RectangleF Empty => emptyRectangle;

    #endregion

    #region Public Fields

    /// <summary>
    /// The x coordinate of the top-left corner of this <see cref="Rectangle"/>.
    /// </summary>
    public float X;

    /// <summary>
    /// The y coordinate of the top-left corner of this <see cref="Rectangle"/>.
    /// </summary>
    public float Y;

    /// <summary>
    /// The width of this <see cref="Rectangle"/>.
    /// </summary>
    public float Width;

    /// <summary>
    /// The height of this <see cref="Rectangle"/>.
    /// </summary>
    public float Height;

    #endregion

    #region Private Static Fields

    private static RectangleF emptyRectangle = new();

    #endregion

    #region Public Constructors

    /// <summary>
    /// Creates a <see cref="RectangleF"/> with the specified
    /// position, width, and height.
    /// </summary>
    /// <param name="x">The x coordinate of the top-left corner of the created <see cref="Rectangle"/>.</param>
    /// <param name="y">The y coordinate of the top-left corner of the created <see cref="Rectangle"/>.</param>
    /// <param name="width">The width of the created <see cref="Rectangle"/>.</param>
    /// <param name="height">The height of the created <see cref="Rectangle"/>.</param>
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="x">The x coordinate of the point to check for containment.</param>
    /// <param name="y">The y coordinate of the point to check for containment.</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
    public bool Contains(float x, float y)
    {
        return (	(this.X <= x) &&
                    (x < (this.X + this.Width)) &&
                    (this.Y <= y) &&
                    (y < (this.Y + this.Height))	);
    }

    /// <summary>
    /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
    public bool Contains(Vector2 value)
    {
        var (x, y) = value;
        return (	(this.X <= x) &&
                    (x < (this.X + this.Width)) &&
                    (this.Y <= y) &&
                    (y < (this.Y + this.Height))	);
    }

    /// <summary>
    /// Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="Rectangle"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
    public bool Contains(RectangleF value)
    {
        return (	(this.X <= value.X) &&
                    ((value.X + value.Width) <= (this.X + this.Width)) &&
                    (this.Y <= value.Y) &&
                    ((value.Y + value.Height) <= (this.Y + this.Height))	);
    }

    public void Contains(ref Vector2 value, out bool result)
    {
        result = (	(this.X <= value.X) &&
                    (value.X < (this.X + this.Width)) &&
                    (this.Y <= value.Y) &&
                    (value.Y < (this.Y + this.Height))	);
    }

    public void Contains(ref RectangleF value, out bool result)
    {
        result = (	(this.X <= value.X) &&
                    ((value.X + value.Width) <= (this.X + this.Width)) &&
                    (this.Y <= value.Y) &&
                    ((value.Y + value.Height) <= (this.Y + this.Height))	);
    }

    /// <summary>
    /// Increments this <see cref="Rectangle"/>'s <see cref="Location"/> by the
    /// x and y components of the provided <see cref="Point"/>.
    /// </summary>
    /// <param name="offset">The x and y components to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
    public void Offset(Vector2 offset)
    {
        X += offset.X;
        Y += offset.Y;
    }

    /// <summary>
    /// Increments this <see cref="Rectangle"/>'s <see cref="Location"/> by the
    /// provided x and y coordinates.
    /// </summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
    public void Offset(float offsetX, float offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    public void Inflate(float horizontalValue, float verticalValue)
    {
        X -= horizontalValue;
        Y -= verticalValue;
        Width += horizontalValue * 2;
        Height += verticalValue * 2;
    }

    /// <summary>
    /// Checks whether or not this <see cref="Rectangle"/> is equivalent
    /// to a provided <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rectangle"/> to test for equality.</param>
    /// <returns>
    /// <c>true</c> if this <see cref="Rectangle"/>'s x coordinate, y coordinate, width, and height
    /// match the values for the provided <see cref="Rectangle"/>. <c>false</c> otherwise.
    /// </returns>
    public bool Equals(RectangleF other)
    {
        return this == other;
    }

    /// <summary>
    /// Checks whether or not this <see cref="Rectangle"/> is equivalent
    /// to a provided object.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to test for equality.</param>
    /// <returns>
    /// <c>true</c> if the provided object is a <see cref="Rectangle"/>, and this
    /// <see cref="Rectangle"/>'s x coordinate, y coordinate, width, and height
    /// match the values for the provided <see cref="Rectangle"/>. <c>false</c> otherwise.
    /// </returns>
    public override bool Equals(object obj)
    {
        return (obj is RectangleF f) && this == f;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public override string ToString()
    {
        return (
            "{X:" + X.ToString() +
            " Y:" + Y.ToString() +
            " Width:" + Width.ToString() +
            " Height:" + Height.ToString() +
            "}"
        );
    }

    /// <summary>
    /// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
    /// </summary>
    /// <param name="value">The other rectangle for testing.</param>
    /// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
    public bool Intersects(RectangleF value)
    {
        return (	value.Left < Right &&
                    Left < value.Right &&
                    value.Top < Bottom &&
                    Top < value.Bottom	);
    }

    /// <summary>
    /// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
    /// </summary>
    /// <param name="value">The other rectangle for testing.</param>
    /// <param name="result"><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
    public void Intersects(ref RectangleF value, out bool result)
    {
        result = (	value.Left < Right &&
                    Left < value.Right &&
                    value.Top < Bottom &&
                    Top < value.Bottom	);
    }

    #endregion

    #region Public Static Methods

    public static bool operator ==(RectangleF a, RectangleF b)
    {
        return (	(a.X == b.X) &&
                    (a.Y == b.Y) &&
                    (a.Width == b.Width) &&
                    (a.Height == b.Height)	);
    }

    public static bool operator !=(RectangleF a, RectangleF b)
    {
        return !(a == b);
    }

    public static RectangleF Intersect(RectangleF value1, RectangleF value2)
    {
        RectangleF rectangle;
        Intersect(ref value1, ref value2, out rectangle);
        return rectangle;
    }

    public static void Intersect(
        ref RectangleF value1,
        ref RectangleF value2,
        out RectangleF result
    ) {
        if (value1.Intersects(value2))
        {
            float right_side = Math.Min(
                value1.X + value1.Width,
                value2.X + value2.Width
            );
            float left_side = Math.Max(value1.X, value2.X);
            float top_side = Math.Max(value1.Y, value2.Y);
            float bottom_side = Math.Min(
                value1.Y + value1.Height,
                value2.Y + value2.Height
            );
            result = new RectangleF(
                left_side,
                top_side,
                right_side - left_side,
                bottom_side - top_side
            );
        }
        else
        {
            result = Empty;
        }
    }

    public static RectangleF Union(RectangleF value1, RectangleF value2)
    {
        float x = Math.Min(value1.X, value2.X);
        float y = Math.Min(value1.Y, value2.Y);
        return new RectangleF(
            x,
            y,
            Math.Max(value1.Right, value2.Right) - x,
            Math.Max(value1.Bottom, value2.Bottom) - y
        );
    }

    public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    #endregion
}