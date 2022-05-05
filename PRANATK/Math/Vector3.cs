using System.Diagnostics;
using System.Text;

namespace PRANA;

/// <summary>
/// Describes a 3D-vector.
/// </summary>
[Serializable]
public struct Vector3 : IEquatable<Vector3>
{
    #region Public Static Properties

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 0, 0.
    /// </summary>
    public static Vector3 Zero => zero;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 1, 1, 1.
    /// </summary>
    public static Vector3 One => one;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 1, 0, 0.
    /// </summary>
    public static Vector3 UnitX => unitX;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 1, 0.
    /// </summary>
    public static Vector3 UnitY => unitY;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 0, 1.
    /// </summary>
    public static Vector3 UnitZ => unitZ;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 1, 0.
    /// </summary>
    public static Vector3 Up => up;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, -1, 0.
    /// </summary>
    public static Vector3 Down => down;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 1, 0, 0.
    /// </summary>
    public static Vector3 Right => right;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components -1, 0, 0.
    /// </summary>
    public static Vector3 Left => left;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 0, -1.
    /// </summary>
    public static Vector3 Forward => forward;

    /// <summary>
    /// Returns a <see cref="Vector3"/> with components 0, 0, 1.
    /// </summary>
    public static Vector3 Backward => backward;

    #endregion


    #region Private Static Fields

    // These are NOT readonly, for weird performance reasons -flibit
    private static Vector3 zero = new(0f, 0f, 0f);
    private static Vector3 one = new(1f, 1f, 1f);
    private static Vector3 unitX = new(1f, 0f, 0f);
    private static Vector3 unitY = new(0f, 1f, 0f);
    private static Vector3 unitZ = new(0f, 0f, 1f);
    private static Vector3 up = new(0f, 1f, 0f);
    private static Vector3 down = new(0f, -1f, 0f);
    private static Vector3 right = new(1f, 0f, 0f);
    private static Vector3 left = new(-1f, 0f, 0f);
    private static Vector3 forward = new(0f, 0f, -1f);
    private static Vector3 backward = new(0f, 0f, 1f);

    #endregion

    #region Public Fields

    /// <summary>
    /// The x coordinate of this <see cref="Vector3"/>.
    /// </summary>
    public float X;

    /// <summary>
    /// The y coordinate of this <see cref="Vector3"/>.
    /// </summary>
    public float Y;

    /// <summary>
    /// The z coordinate of this <see cref="Vector3"/>.
    /// </summary>
    public float Z;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Constructs a 3d vector with X, Y and Z from three values.
    /// </summary>
    /// <param name="x">The x coordinate in 3d-space.</param>
    /// <param name="y">The y coordinate in 3d-space.</param>
    /// <param name="z">The z coordinate in 3d-space.</param>
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Constructs a 3d vector with X, Y and Z set to the same value.
    /// </summary>
    /// <param name="value">The x, y and z coordinates in 3d-space.</param>
    public Vector3(float value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    /// <summary>
    /// Constructs a 3d vector with X, Y from <see cref="Vector2"/> and Z from a scalar.
    /// </summary>
    /// <param name="value">The x and y coordinates in 3d-space.</param>
    /// <param name="z">The z coordinate in 3d-space.</param>
    public Vector3(Vector2 value, float z)
    {
        X = value.X;
        Y = value.Y;
        Z = z;
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
        return obj is Vector3 && Equals((Vector3)obj);
    }

    /// <summary>
    /// Compares whether current instance is equal to specified <see cref="Vector3"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vector3"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public bool Equals(Vector3 other)
    {
        return Calc.ApproximatelyEqual(X, other.X) &&
               Calc.ApproximatelyEqual(Y, other.Y) &&
               Calc.ApproximatelyEqual(Z, other.Z);
    }

    /// <summary>
    /// Gets the hash code of this <see cref="Vector3"/>.
    /// </summary>
    /// <returns>Hash code of this <see cref="Vector3"/>.</returns>
    public override int GetHashCode()
    {
        return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
    }

    /// <summary>
    /// Returns the length of this <see cref="Vector3"/>.
    /// </summary>
    /// <returns>The length of this <see cref="Vector3"/>.</returns>
    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    /// <summary>
    /// Returns the squared length of this <see cref="Vector3"/>.
    /// </summary>
    /// <returns>The squared length of this <see cref="Vector3"/>.</returns>
    public float LengthSquared()
    {
        return X * X + Y * Y + Z * Z;
    }

    /// <summary>
    /// Turns this <see cref="Vector3"/> to a unit vector with the same direction.
    /// </summary>
    public void Normalize()
    {
        var factor = 1.0f / (float)Math.Sqrt(
            X * X +
            Y * Y +
            Z * Z
        );
        X *= factor;
        Y *= factor;
        Z *= factor;
    }

    /// <summary>
    /// Returns a <see cref="String"/> representation of this <see cref="Vector3"/> in the format:
    /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>]}
    /// </summary>
    /// <returns>A <see cref="String"/> representation of this <see cref="Vector3"/>.</returns>
    public override string ToString()
    {
        StringBuilder sb = new(32);
        sb.Append("{X:");
        sb.Append(X);
        sb.Append(" Y:");
        sb.Append(Y);
        sb.Append(" Z:");
        sb.Append(Z);
        sb.Append("}");
        return sb.ToString();
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
    /// </summary>
    /// <param name="value1">The first vector to add.</param>
    /// <param name="value2">The second vector to add.</param>
    /// <returns>The result of the vector addition.</returns>
    public static Vector3 Add(Vector3 value1, Vector3 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        value1.Z += value2.Z;
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
    public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = value1.X + value2.X;
        result.Y = value1.Y + value2.Y;
        result.Z = value1.Z + value2.Z;
    }


    /// <summary>
    /// Clamps the specified value within a range.
    /// </summary>
    /// <param name="value1">The value to clamp.</param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <returns>The clamped value.</returns>
    public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Calc.Clamp(value1.X, min.X, max.X),
            Calc.Clamp(value1.Y, min.Y, max.Y),
            Calc.Clamp(value1.Z, min.Z, max.Z)
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
        ref Vector3 value1,
        ref Vector3 min,
        ref Vector3 max,
        out Vector3 result
    )
    {
        result.X = Calc.Clamp(value1.X, min.X, max.X);
        result.Y = Calc.Clamp(value1.Y, min.Y, max.Y);
        result.Z = Calc.Clamp(value1.Z, min.Z, max.Z);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cross product of two vectors.</returns>
    public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
    {
        Cross(ref vector1, ref vector2, out vector1);
        return vector1;
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="result">The cross product of two vectors as an output parameter.</param>
    public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
    {
        var x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
        var y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
        var z = vector1.X * vector2.Y - vector2.X * vector1.Y;
        result.X = x;
        result.Y = y;
        result.Z = z;
    }

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The distance between two vectors.</returns>
    public static float Distance(Vector3 vector1, Vector3 vector2)
    {
        DistanceSquared(ref vector1, ref vector2, out var result);
        return (float)Math.Sqrt(result);
    }

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The distance between two vectors as an output parameter.</param>
    public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
    {
        DistanceSquared(ref value1, ref value2, out result);
        result = (float)Math.Sqrt(result);
    }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The squared distance between two vectors.</returns>
    public static float DistanceSquared(Vector3 value1, Vector3 value2)
    {
        return (value1.X - value2.X) * (value1.X - value2.X) +
               (value1.Y - value2.Y) * (value1.Y - value2.Y) +
               (value1.Z - value2.Z) * (value1.Z - value2.Z);
    }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The squared distance between two vectors as an output parameter.</param>
    public static void DistanceSquared(
        ref Vector3 value1,
        ref Vector3 value2,
        out float result
    )
    {
        result = (value1.X - value2.X) * (value1.X - value2.X) +
                 (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                 (value1.Z - value2.Z) * (value1.Z - value2.Z);
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector3"/>.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector3 Divide(Vector3 value1, Vector3 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        value1.Z /= value2.Z;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector3"/>.</param>
    /// <param name="result">The result of dividing the vectors as an output parameter.</param>
    public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = value1.X / value2.X;
        result.Y = value1.Y / value2.Y;
        result.Z = value1.Z / value2.Z;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Divisor scalar.</param>
    /// <returns>The result of dividing a vector by a scalar.</returns>
    public static Vector3 Divide(Vector3 value1, float value2)
    {
        var factor = 1 / value2;
        value1.X *= factor;
        value1.Y *= factor;
        value1.Z *= factor;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Divisor scalar.</param>
    /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
    public static void Divide(ref Vector3 value1, float value2, out Vector3 result)
    {
        var factor = 1 / value2;
        result.X = value1.X * factor;
        result.Y = value1.Y * factor;
        result.Z = value1.Z * factor;
    }

    /// <summary>
    /// Returns a dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product of two vectors.</returns>
    public static float Dot(Vector3 vector1, Vector3 vector2)
    {
        return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
    }

    /// <summary>
    /// Returns a dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="result">The dot product of two vectors as an output parameter.</param>
    public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
    {
        result = vector1.X * vector2.X +
                 vector1.Y * vector2.Y +
                 vector1.Z * vector2.Z;
    }


    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector3"/> with maximal values from the two vectors.</returns>
    public static Vector3 Max(Vector3 value1, Vector3 value2)
    {
        return new Vector3(
            Calc.Max(value1.X, value2.X),
            Calc.Max(value1.Y, value2.Y),
            Calc.Max(value1.Z, value2.Z)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector3"/> with maximal values from the two vectors as an output parameter.</param>
    public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = Calc.Max(value1.X, value2.X);
        result.Y = Calc.Max(value1.Y, value2.Y);
        result.Z = Calc.Max(value1.Z, value2.Z);
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector3"/> with minimal values from the two vectors.</returns>
    public static Vector3 Min(Vector3 value1, Vector3 value2)
    {
        return new Vector3(
            Calc.Min(value1.X, value2.X),
            Calc.Min(value1.Y, value2.Y),
            Calc.Min(value1.Z, value2.Z)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector3"/> with minimal values from the two vectors as an output parameter.</param>
    public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = Calc.Min(value1.X, value2.X);
        result.Y = Calc.Min(value1.Y, value2.Y);
        result.Z = Calc.Min(value1.Z, value2.Z);
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <returns>The result of the vector multiplication.</returns>
    public static Vector3 Multiply(Vector3 value1, Vector3 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        value1.Z *= value2.Z;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a multiplication of <see cref="Vector3"/> and a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <returns>The result of the vector multiplication with a scalar.</returns>
    public static Vector3 Multiply(Vector3 value1, float scaleFactor)
    {
        value1.X *= scaleFactor;
        value1.Y *= scaleFactor;
        value1.Z *= scaleFactor;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a multiplication of <see cref="Vector3"/> and a scalar.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
    public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
    {
        result.X = value1.X * scaleFactor;
        result.Y = value1.Y * scaleFactor;
        result.Z = value1.Z * scaleFactor;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <param name="result">The result of the vector multiplication as an output parameter.</param>
    public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = value1.X * value2.X;
        result.Y = value1.Y * value2.Y;
        result.Z = value1.Z * value2.Z;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains the specified vector inversion.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/>.</param>
    /// <returns>The result of the vector inversion.</returns>
    public static Vector3 Negate(Vector3 value)
    {
        value = new Vector3(-value.X, -value.Y, -value.Z);
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains the specified vector inversion.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/>.</param>
    /// <param name="result">The result of the vector inversion as an output parameter.</param>
    public static void Negate(ref Vector3 value, out Vector3 result)
    {
        result.X = -value.X;
        result.Y = -value.Y;
        result.Z = -value.Z;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a normalized values from another vector.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/>.</param>
    /// <returns>Unit vector.</returns>
    public static Vector3 Normalize(Vector3 value)
    {
        var factor = 1.0f / (float)Math.Sqrt(
            value.X * value.X +
            value.Y * value.Y +
            value.Z * value.Z
        );
        return new Vector3(
            value.X * factor,
            value.Y * factor,
            value.Z * factor
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a normalized values from another vector.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/>.</param>
    /// <param name="result">Unit vector as an output parameter.</param>
    public static void Normalize(ref Vector3 value, out Vector3 result)
    {
        var factor = 1.0f / (float)Math.Sqrt(
            value.X * value.X +
            value.Y * value.Y +
            value.Z * value.Z
        );
        result.X = value.X * factor;
        result.Y = value.Y * factor;
        result.Z = value.Z * factor;
    }


    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains cubic interpolation of the specified vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <param name="amount">Weighting value.</param>
    /// <returns>Cubic interpolation of the specified vectors.</returns>
    public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
    {
        return new Vector3(
            Calc.SmoothStep(value1.X, value2.X, amount),
            Calc.SmoothStep(value1.Y, value2.Y, amount),
            Calc.SmoothStep(value1.Z, value2.Z, amount)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains cubic interpolation of the specified vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <param name="amount">Weighting value.</param>
    /// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
    public static void SmoothStep(
        ref Vector3 value1,
        ref Vector3 value2,
        float amount,
        out Vector3 result
    )
    {
        result.X = Calc.SmoothStep(value1.X, value2.X, amount);
        result.Y = Calc.SmoothStep(value1.Y, value2.Y, amount);
        result.Z = Calc.SmoothStep(value1.Z, value2.Z, amount);
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains subtraction of on <see cref="Vector3"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <returns>The result of the vector subtraction.</returns>
    public static Vector3 Subtract(Vector3 value1, Vector3 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        value1.Z -= value2.Z;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains subtraction of on <see cref="Vector3"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/>.</param>
    /// <param name="value2">Source <see cref="Vector3"/>.</param>
    /// <param name="result">The result of the vector subtraction as an output parameter.</param>
    public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
        result.X = value1.X - value2.X;
        result.Y = value1.Y - value2.Y;
        result.Z = value1.Z - value2.Z;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="PRANA.Transform"/>.
    /// </summary>
    /// <param name="position">Source <see cref="Vector3"/>.</param>
    /// <param name="matrix">The transformation <see cref="PRANA.Transform"/>.</param>
    /// <returns>Transformed <see cref="Vector3"/>.</returns>
    public static Vector3 Transform(Vector3 position, Transform matrix)
    {
        Transform(ref position, ref matrix, out position);
        return position;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="PRANA.Transform"/>.
    /// </summary>
    /// <param name="position">Source <see cref="Vector3"/>.</param>
    /// <param name="matrix">The transformation <see cref="PRANA.Transform"/>.</param>
    /// <param name="result">Transformed <see cref="Vector3"/> as an output parameter.</param>
    public static void Transform(
        ref Vector3 position,
        ref Transform matrix,
        out Vector3 result
    )
    {
        var x = position.X * matrix.M11 +
                position.Y * matrix.M21 +
                position.Z * matrix.M31 +
                matrix.M41;
        var y = position.X * matrix.M12 +
                position.Y * matrix.M22 +
                position.Z * matrix.M32 +
                matrix.M42;
        var z = position.X * matrix.M13 +
                position.Y * matrix.M23 +
                position.Z * matrix.M33 +
                matrix.M43;
        result.X = x;
        result.Y = y;
        result.Z = z;
    }

    /// <summary>
    /// Apply transformation on all vectors within array of <see cref="Vector3"/> by the specified <see cref="PRANA.Transform"/> and places the results in an another array.
    /// </summary>
    /// <param name="sourceArray">Source array.</param>
    /// <param name="matrix">The transformation <see cref="PRANA.Transform"/>.</param>
    /// <param name="destinationArray">Destination array.</param>
    public static void Transform(
        Vector3[] sourceArray,
        ref Transform matrix,
        Vector3[] destinationArray
    )
    {
        Debug.Assert(
            destinationArray.Length >= sourceArray.Length,
            "The destination array is smaller than the source array."
        );

       
        for (var i = 0; i < sourceArray.Length; i += 1)
        {
            var position = sourceArray[i];
            destinationArray[i] = new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 +
                position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 +
                position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 +
                position.Z * matrix.M33 + matrix.M43
            );
        }
    }

    /// <summary>
    /// Apply transformation on vectors within array of <see cref="Vector3"/> by the specified <see cref="PRANA.Transform"/> and places the results in an another array.
    /// </summary>
    /// <param name="sourceArray">Source array.</param>
    /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
    /// <param name="matrix">The transformation <see cref="PRANA.Transform"/>.</param>
    /// <param name="destinationArray">Destination array.</param>
    /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3"/> should be written.</param>
    /// <param name="length">The number of vectors to be transformed.</param>
    public static void Transform(
        Vector3[] sourceArray,
        int sourceIndex,
        ref Transform matrix,
        Vector3[] destinationArray,
        int destinationIndex,
        int length
    )
    {
        Debug.Assert(
            sourceArray.Length - sourceIndex >= length,
            "The source array is too small for the given sourceIndex and length."
        );
        Debug.Assert(
            destinationArray.Length - destinationIndex >= length,
            "The destination array is too small for " +
            "the given destinationIndex and length."
        );

        /* TODO: Are there options on some platforms to implement a
         * vectorized version of this?
         */

        for (var i = 0; i < length; i += 1)
        {
            var position = sourceArray[sourceIndex + i];
            destinationArray[destinationIndex + i] = new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 +
                position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 +
                position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 +
                position.Z * matrix.M33 + matrix.M43
            );
        }
    }

    #endregion

    #region Public Static Operators

    /// <summary>
    /// Compares whether two <see cref="Vector3"/> instances are equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector3"/> instance on the left of the equal sign.</param>
    /// <param name="value2"><see cref="Vector3"/> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(Vector3 value1, Vector3 value2)
    {
        return Calc.ApproximatelyEqual(value1.X, value2.X) &&
               Calc.ApproximatelyEqual(value1.Y, value2.Y) &&
               Calc.ApproximatelyEqual(value1.Z, value2.Z);
    }

    /// <summary>
    /// Compares whether two <see cref="Vector3"/> instances are not equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector3"/> instance on the left of the not equal sign.</param>
    /// <param name="value2"><see cref="Vector3"/> instance on the right of the not equal sign.</param>
    /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(Vector3 value1, Vector3 value2)
    {
        return !(value1 == value2);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/> on the left of the add sign.</param>
    /// <param name="value2">Source <see cref="Vector3"/> on the right of the add sign.</param>
    /// <returns>Sum of the vectors.</returns>
    public static Vector3 operator +(Vector3 value1, Vector3 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        value1.Z += value2.Z;
        return value1;
    }

    /// <summary>
    /// Inverts values in the specified <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/> on the right of the sub sign.</param>
    /// <returns>Result of the inversion.</returns>
    public static Vector3 operator -(Vector3 value)
    {
        value = new Vector3(-value.X, -value.Y, -value.Z);
        return value;
    }

    /// <summary>
    /// Subtracts a <see cref="Vector3"/> from a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/> on the left of the sub sign.</param>
    /// <param name="value2">Source <see cref="Vector3"/> on the right of the sub sign.</param>
    /// <returns>Result of the vector subtraction.</returns>
    public static Vector3 operator -(Vector3 value1, Vector3 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        value1.Z -= value2.Z;
        return value1;
    }

    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/> on the left of the mul sign.</param>
    /// <param name="value2">Source <see cref="Vector3"/> on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication.</returns>
    public static Vector3 operator *(Vector3 value1, Vector3 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        value1.Z *= value2.Z;
        return value1;
    }

    /// <summary>
    /// Multiplies the components of vector by a scalar.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/> on the left of the mul sign.</param>
    /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication with a scalar.</returns>
    public static Vector3 operator *(Vector3 value, float scaleFactor)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        value.Z *= scaleFactor;
        return value;
    }

    /// <summary>
    /// Multiplies the components of vector by a scalar.
    /// </summary>
    /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
    /// <param name="value">Source <see cref="Vector3"/> on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication with a scalar.</returns>
    public static Vector3 operator *(float scaleFactor, Vector3 value)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        value.Z *= scaleFactor;
        return value;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3"/> on the left of the div sign.</param>
    /// <param name="value2">Divisor <see cref="Vector3"/> on the right of the div sign.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector3 operator /(Vector3 value1, Vector3 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        value1.Z /= value2.Z;
        return value1;
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3"/> by a scalar.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3"/> on the left of the div sign.</param>
    /// <param name="divider">Divisor scalar on the right of the div sign.</param>
    /// <returns>The result of dividing a vector by a scalar.</returns>
    public static Vector3 operator /(Vector3 value, float divider)
    {
        var factor = 1 / divider;
        value.X *= factor;
        value.Y *= factor;
        value.Z *= factor;
        return value;
    }

    #endregion
}