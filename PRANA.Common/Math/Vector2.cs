namespace PRANA.Common;

/// <summary>
/// Describes a 2D-vector.
/// </summary>
[Serializable]
public struct Vector2 : IEquatable<Vector2>
{
    #region Public Static Properties

    /// <summary>
    /// Returns a <see cref="Vector2"/> with components 0, 0.
    /// </summary>
    public static Vector2 Zero => zeroVector;

    /// <summary>
    /// Returns a <see cref="Vector2"/> with components 1, 1.
    /// </summary>
    public static Vector2 One => unitVector;

    /// <summary>
    /// Returns a <see cref="Vector2"/> with components 1, 0.
    /// </summary>
    public static Vector2 UnitX => unitXVector;

    /// <summary>
    /// Returns a <see cref="Vector2"/> with components 0, 1.
    /// </summary>
    public static Vector2 UnitY => unitYVector;

    #endregion

    #region Internal Properties

    internal string DebugDisplayString =>
        string.Concat(
            X.ToString(), " ",
            Y.ToString()
        );

    #endregion

    #region Public Fields

    /// <summary>
    /// The x coordinate of this <see cref="Vector2"/>.
    /// </summary>
    public float X;

    /// <summary>
    /// The y coordinate of this <see cref="Vector2"/>.
    /// </summary>
    public float Y;

    #endregion

    #region Private Static Fields

    // These are NOT readonly, for weird performance reasons -flibit
    private static Vector2 zeroVector = new(0f, 0f);
    private static Vector2 unitVector = new(1f, 1f);
    private static Vector2 unitXVector = new(1f, 0f);
    private static Vector2 unitYVector = new(0f, 1f);

    #endregion

    #region Public Constructors

    /// <summary>
    /// Constructs a 2d vector with X and Y from two values.
    /// </summary>
    /// <param name="x">The x coordinate in 2d-space.</param>
    /// <param name="y">The y coordinate in 2d-space.</param>
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Constructs a 2d vector with X and Y set to the same value.
    /// </summary>
    /// <param name="value">The x and y coordinates in 2d-space.</param>
    public Vector2(float value)
    {
        X = value;
        Y = value;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Compares whether current instance is equal to specified <see cref="Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public override bool Equals(object obj)
    {
        return obj is Vector2 vector2 && Equals(vector2);
    }

    /// <summary>
    /// Compares whether current instance is equal to specified <see cref="Vector2"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vector2"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public bool Equals(Vector2 other)
    {
        return Calc.ApproximatelyEqual(X, other.X) &&
               Calc.ApproximatelyEqual(Y, other.Y);
    }

    /// <summary>
    /// Gets the hash code of this <see cref="Vector2"/>.
    /// </summary>
    /// <returns>Hash code of this <see cref="Vector2"/>.</returns>
    public override int GetHashCode()
    {
        return X.GetHashCode() + Y.GetHashCode();
    }

    /// <summary>
    /// Returns the length of this <see cref="Vector2"/>.
    /// </summary>
    /// <returns>The length of this <see cref="Vector2"/>.</returns>
    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }

    /// <summary>
    /// Returns the squared length of this <see cref="Vector2"/>.
    /// </summary>
    /// <returns>The squared length of this <see cref="Vector2"/>.</returns>
    public float LengthSquared()
    {
        return X * X + Y * Y;
    }

    /// <summary>
    /// Turns this <see cref="Vector2"/> to a unit vector with the same direction.
    /// </summary>
    public void Normalize()
    {
        var val = 1.0f / (float)Math.Sqrt(X * X + Y * Y);
        X *= val;
        Y *= val;
    }

    /// <summary>
    /// Returns a <see cref="String"/> representation of this <see cref="Vector2"/> in the format:
    /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
    /// </summary>
    /// <returns>A <see cref="String"/> representation of this <see cref="Vector2"/>.</returns>
    public override string ToString()
    {
        return "{X:" + X +
               " Y:" + Y +
               "}";
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
    /// </summary>
    /// <param name="value1">The first vector to add.</param>
    /// <param name="value2">The second vector to add.</param>
    /// <returns>The result of the vector addition.</returns>
    public static Vector2 Add(Vector2 value1, Vector2 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    /// <summary>
    /// Performs vector addition on <paramref name="value1"/> and
    /// <paramref name="value2"/>, storing the result of the
    /// addition in <paramref name="result"/>.
    /// </summary>
    /// <param name="value1">The first vector to add.</param>
    /// <param name="value2">The second vector to add.</param>
    /// <param name="result">The result of the vector addition.</param>
    public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X + value2.X;
        result.Y = value1.Y + value2.Y;
    }


    /// <summary>
    /// Clamps the specified value within a range.
    /// </summary>
    /// <param name="value1">The value to clamp.</param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <returns>The clamped value.</returns>
    public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
    {
        return new Vector2(
            Calc.Clamp(value1.X, min.X, max.X),
            Calc.Clamp(value1.Y, min.Y, max.Y)
        );
    }

    /// <summary>
    /// Clamps the specified value within a range.
    /// </summary>
    /// <param name="value1">The value to clamp.</param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <param name="result">The clamped value as an output parameter.</param>
    public static void Clamp(
        ref Vector2 value1,
        ref Vector2 min,
        ref Vector2 max,
        out Vector2 result
    )
    {
        result.X = Calc.Clamp(value1.X, min.X, max.X);
        result.Y = Calc.Clamp(value1.Y, min.Y, max.Y);
    }

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The distance between two vectors.</returns>
    public static float Distance(Vector2 value1, Vector2 value2)
    {
        float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
        return (float)Math.Sqrt(v1 * v1 + v2 * v2);
    }

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The distance between two vectors as an output parameter.</param>
    public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
    {
        float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
        result = (float)Math.Sqrt(v1 * v1 + v2 * v2);
    }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The squared distance between two vectors.</returns>
    public static float DistanceSquared(Vector2 value1, Vector2 value2)
    {
        float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
        return v1 * v1 + v2 * v2;
    }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The squared distance between two vectors as an output parameter.</param>
    public static void DistanceSquared(
        ref Vector2 value1,
        ref Vector2 value2,
        out float result
    )
    {
        float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
        result = v1 * v1 + v2 * v2;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector2"/>.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector2 Divide(Vector2 value1, Vector2 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector2"/>.</param>
    /// <param name="result">The result of dividing the vectors as an output parameter.</param>
    public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X / value2.X;
        result.Y = value1.Y / value2.Y;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="divider">Divisor scalar.</param>
    /// <returns>The result of dividing a vector by a scalar.</returns>
    public static Vector2 Divide(Vector2 value1, float divider)
    {
        var factor = 1 / divider;
        value1.X *= factor;
        value1.Y *= factor;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="divider">Divisor scalar.</param>
    /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
    public static void Divide(ref Vector2 value1, float divider, out Vector2 result)
    {
        var factor = 1 / divider;
        result.X = value1.X * factor;
        result.Y = value1.Y * factor;
    }

    /// <summary>
    /// Returns a dot product of two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The dot product of two vectors.</returns>
    public static float Dot(Vector2 value1, Vector2 value2)
    {
        return value1.X * value2.X + value1.Y * value2.Y;
    }

    /// <summary>
    /// Returns a dot product of two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The dot product of two vectors as an output parameter.</param>
    public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
    {
        result = value1.X * value2.X + value1.Y * value2.Y;
    }


    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector2"/> with maximal values from the two vectors.</returns>
    public static Vector2 Max(Vector2 value1, Vector2 value2)
    {
        return new Vector2(
            value1.X > value2.X ? value1.X : value2.X,
            value1.Y > value2.Y ? value1.Y : value2.Y
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector2"/> with maximal values from the two vectors as an output parameter.</param>
    public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X > value2.X ? value1.X : value2.X;
        result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector2"/> with minimal values from the two vectors.</returns>
    public static Vector2 Min(Vector2 value1, Vector2 value2)
    {
        return new Vector2(
            value1.X < value2.X ? value1.X : value2.X,
            value1.Y < value2.Y ? value1.Y : value2.Y
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector2"/> with minimal values from the two vectors as an output parameter.</param>
    public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X < value2.X ? value1.X : value2.X;
        result.Y = value1.Y < value2.Y ? value1.Y : value2.Y;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <returns>The result of the vector multiplication.</returns>
    public static Vector2 Multiply(Vector2 value1, Vector2 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a multiplication of <see cref="Vector2"/> and a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <returns>The result of the vector multiplication with a scalar.</returns>
    public static Vector2 Multiply(Vector2 value1, float scaleFactor)
    {
        value1.X *= scaleFactor;
        value1.Y *= scaleFactor;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a multiplication of <see cref="Vector2"/> and a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
    public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result)
    {
        result.X = value1.X * scaleFactor;
        result.Y = value1.Y * scaleFactor;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <param name="result">The result of the vector multiplication as an output parameter.</param>
    public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X * value2.X;
        result.Y = value1.Y * value2.Y;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains the specified vector inversion.
    /// direction of <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/>.</param>
    /// <returns>The result of the vector inversion.</returns>
    public static Vector2 Negate(Vector2 value)
    {
        value.X = -value.X;
        value.Y = -value.Y;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains the specified vector inversion.
    /// direction of <paramref name="value"/> in <paramref name="result"/>.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/>.</param>
    /// <param name="result">The result of the vector inversion as an output parameter.</param>
    public static void Negate(ref Vector2 value, out Vector2 result)
    {
        result.X = -value.X;
        result.Y = -value.Y;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a normalized values from another vector.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/>.</param>
    /// <returns>Unit vector.</returns>
    public static Vector2 Normalize(Vector2 value)
    {
        var val = 1.0f / (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
        value.X *= val;
        value.Y *= val;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a normalized values from another vector.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/>.</param>
    /// <param name="result">Unit vector as an output parameter.</param>
    public static void Normalize(ref Vector2 value, out Vector2 result)
    {
        var val = 1.0f / (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
        result.X = value.X * val;
        result.Y = value.Y * val;
    }


    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains cubic interpolation of the specified vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <param name="amount">Weighting value.</param>
    /// <returns>Cubic interpolation of the specified vectors.</returns>
    public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount)
    {
        return new Vector2(
            Calc.SmoothStep(value1.X, value2.X, amount),
            Calc.SmoothStep(value1.Y, value2.Y, amount)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains cubic interpolation of the specified vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <param name="amount">Weighting value.</param>
    /// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
    public static void SmoothStep(
        ref Vector2 value1,
        ref Vector2 value2,
        float amount,
        out Vector2 result
    )
    {
        result.X = Calc.SmoothStep(value1.X, value2.X, amount);
        result.Y = Calc.SmoothStep(value1.Y, value2.Y, amount);
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains subtraction of on <see cref="Vector2"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <returns>The result of the vector subtraction.</returns>
    public static Vector2 Subtract(Vector2 value1, Vector2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains subtraction of on <see cref="Vector2"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/>.</param>
    /// <param name="value2">Source <see cref="Vector2"/>.</param>
    /// <param name="result">The result of the vector subtraction as an output parameter.</param>
    public static void Subtract(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
    {
        result.X = value1.X - value2.X;
        result.Y = value1.Y - value2.Y;
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
    /// </summary>
    /// <param name="position">Source <see cref="Vector2"/>.</param>
    /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
    /// <returns>Transformed <see cref="Vector2"/>.</returns>
    public static Vector2 Transform(Vector2 position, Matrix matrix)
    {
        return new Vector2(
            position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
    /// </summary>
    /// <param name="position">Source <see cref="Vector2"/>.</param>
    /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
    /// <param name="result">Transformed <see cref="Vector2"/> as an output parameter.</param>
    public static void Transform(
        ref Vector2 position,
        ref Matrix matrix,
        out Vector2 result
    )
    {
        var x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
        var y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
        result.X = x;
        result.Y = y;
    }


    /// <summary>
    /// Apply transformation on all vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
    /// </summary>
    /// <param name="sourceArray">Source array.</param>
    /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
    /// <param name="destinationArray">Destination array.</param>
    public static void Transform(
        Vector2[] sourceArray,
        ref Matrix matrix,
        Vector2[] destinationArray
    )
    {
        Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
    }

    /// <summary>
    /// Apply transformation on vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
    /// </summary>
    /// <param name="sourceArray">Source array.</param>
    /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
    /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
    /// <param name="destinationArray">Destination array.</param>
    /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
    /// <param name="length">The number of vectors to be transformed.</param>
    public static void Transform(
        Vector2[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector2[] destinationArray,
        int destinationIndex,
        int length
    )
    {
        for (var x = 0; x < length; x += 1)
        {
            var position = sourceArray[sourceIndex + x];
            var destination = destinationArray[destinationIndex + x];
            destination.X = position.X * matrix.M11 + position.Y * matrix.M21
                                                    + matrix.M41;
            destination.Y = position.X * matrix.M12 + position.Y * matrix.M22
                                                    + matrix.M42;
            destinationArray[destinationIndex + x] = destination;
        }
    }

    #endregion

    #region Public Static Operators

    /// <summary>
    /// Inverts values in the specified <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/> on the right of the sub sign.</param>
    /// <returns>Result of the inversion.</returns>
    public static Vector2 operator -(Vector2 value)
    {
        value.X = -value.X;
        value.Y = -value.Y;
        return value;
    }

    /// <summary>
    /// Compares whether two <see cref="Vector2"/> instances are equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector2"/> instance on the left of the equal sign.</param>
    /// <param name="value2"><see cref="Vector2"/> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(Vector2 value1, Vector2 value2)
    {
        return Calc.ApproximatelyEqual(value1.X, value2.X) &&
               Calc.ApproximatelyEqual(value1.Y, value2.Y);
    }

    /// <summary>
    /// Compares whether two <see cref="Vector2"/> instances are equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector2"/> instance on the left of the equal sign.</param>
    /// <param name="value2"><see cref="Vector2"/> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(Vector2 value1, Vector2 value2)
    {
        return !(value1 == value2);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/> on the left of the add sign.</param>
    /// <param name="value2">Source <see cref="Vector2"/> on the right of the add sign.</param>
    /// <returns>Sum of the vectors.</returns>
    public static Vector2 operator +(Vector2 value1, Vector2 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/> on the left of the sub sign.</param>
    /// <param name="value2">Source <see cref="Vector2"/> on the right of the sub sign.</param>
    /// <returns>Result of the vector subtraction.</returns>
    public static Vector2 operator -(Vector2 value1, Vector2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/> on the left of the mul sign.</param>
    /// <param name="value2">Source <see cref="Vector2"/> on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication.</returns>
    public static Vector2 operator *(Vector2 value1, Vector2 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    /// <summary>
    /// Multiplies the components of vector by a scalar.
    /// </summary>
    /// <param name="value">Source <see cref="Vector2"/> on the left of the mul sign.</param>
    /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication with a scalar.</returns>
    public static Vector2 operator *(Vector2 value, float scaleFactor)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    /// <summary>
    /// Multiplies the components of vector by a scalar.
    /// </summary>
    /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
    /// <param name="value">Source <see cref="Vector2"/> on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication with a scalar.</returns>
    public static Vector2 operator *(float scaleFactor, Vector2 value)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/> on the left of the div sign.</param>
    /// <param name="value2">Divisor <see cref="Vector2"/> on the right of the div sign.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector2 operator /(Vector2 value1, Vector2 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector2"/> by a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector2"/> on the left of the div sign.</param>
    /// <param name="divider">Divisor scalar on the right of the div sign.</param>
    /// <returns>The result of dividing a vector by a scalar.</returns>
    public static Vector2 operator /(Vector2 value1, float divider)
    {
        var factor = 1 / divider;
        value1.X *= factor;
        value1.Y *= factor;
        return value1;
    }

    #endregion
    
    public static implicit operator System.Numerics.Vector2(Vector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }

    public static implicit operator Vector2(System.Numerics.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }
}