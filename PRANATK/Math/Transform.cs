using System.Runtime.InteropServices;

namespace PRANA;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Transform : IEquatable<Transform>
{
    public float M11;
    public float M12;
    public float M13;
    public float M14;
    public float M21;
    public float M22;
    public float M23;
    public float M24;
    public float M31;
    public float M32;
    public float M33;
    public float M34;
    public float M41;
    public float M42;
    public float M43;
    public float M44;

    public static Transform Identity => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    private Transform(
        float a0, float a1, float a2, float a3,
        float a4, float a5, float a6, float a7,
        float a8, float a9, float a10, float a11,
        float a12, float a13, float a14, float a15
    )
    {
        M11 = a0;
        M12 = a1;
        M13 = a2;
        M14 = a3;
        M21 = a4;
        M22 = a5;
        M23 = a6;
        M24 = a7;
        M31 = a8;
        M32 = a9;
        M33 = a10;
        M34 = a11;
        M41 = a12;
        M42 = a13;
        M43 = a14;
        M44 = a15;
    }

    public Transform(
        float a00, float a01, float a02,
        float a10, float a11, float a12,
        float a20, float a21, float a22
    )
    {
        M11 = a00;
        M12 = a10;
        M13 = 0f;
        M14 = a20;
        M21 = a01;
        M22 = a11;
        M23 = 0f;
        M24 = a21;
        M31 = 0f;
        M32 = 0f;
        M33 = -1f;
        M34 = 0f;
        M41 = a02;
        M42 = a12;
        M43 = 0f;
        M44 = a22;
    }

    public Transform GetInverse()
    {
        float determinant =
            M11 * (M44 * M22 - M24 * M42) -
            M12 * (M44 * M21 - M24 * M41) +
            M14 * (M42 * M21 - M22 * M41);

        if (determinant != 0f)
        {
            return new Transform(
                (M44 * M22 - M24 * M42) / determinant,
                -(M44 * M21 - M24 * M41) / determinant,
                (M42 * M21 - M22 * M41) / determinant,
                -(M44 * M12 - M14 * M42) / determinant,
                (M44 * M11 - M14 * M41) / determinant,
                -(M42 * M11 - M12 * M41) / determinant,
                (M24 * M12 - M14 * M22) / determinant,
                -(M24 * M11 - M14 * M21) / determinant,
                (M22 * M11 - M12 * M21) / determinant
            );
        }

        return Identity;
    }

    public static void TransformPoint(ref Vector2 point, ref Transform transform, out Vector2 result)
    {
        var x = transform.M11 * point.X + transform.M21 * point.Y + transform.M41;
        var y = transform.M12 * point.X + transform.M22 * point.Y + transform.M42;

        result.X = x;
        result.Y = y;
    }

    public static void TransformRect(ref RectangleF rect, ref Transform transform, out RectangleF result)
    {
        // Transform the 4 corners of the rect
        Span<Vector2> points = stackalloc Vector2[4];

        var corner1 = rect.TopLeft;
        var corner2 = rect.TopRight;
        var corner3 = rect.BottomLeft;
        var corner4 = rect.BottomRight;

        TransformPoint(ref corner1, ref transform, out points[0]);
        TransformPoint(ref corner2, ref transform, out points[1]);
        TransformPoint(ref corner3, ref transform, out points[2]);
        TransformPoint(ref corner4, ref transform, out points[3]);

        // Compute the bounding rect of the transformed points
        float left = points[0].X;
        float top = points[0].Y;
        float right = points[0].X;
        float bottom = points[0].Y;

        for (int i = 1; i < 4; ++i)
        {
            if (points[i].X < left)
            {
                left = points[i].X;
            }
            else if (points[i].X > right)
            {
                right = points[i].X;
            }

            if (points[i].Y < top)
            {
                top = points[i].Y;
            }
            else if (points[i].Y > bottom)
            {
                bottom = points[i].Y;
            }
        }

        result = new RectangleF(left, top, right - left, bottom - top);
    }

    public static void Combine(ref Transform transformA, ref Transform transformB, out Transform result)
    {
        result.M11 = transformA.M11 * transformB.M11 + transformA.M21 * transformB.M12 +
                     transformA.M41 * transformB.M14;
        result.M12 = transformA.M11 * transformB.M21 + transformA.M21 * transformB.M22 +
                     transformA.M41 * transformB.M24;
        result.M13 = 0f;
        result.M14 = transformA.M11 * transformB.M41 + transformA.M21 * transformB.M42 +
                     transformA.M41 * transformB.M44;
        result.M21 = transformA.M12 * transformB.M11 + transformA.M22 * transformB.M12 +
                     transformA.M42 * transformB.M14;
        result.M22 = transformA.M12 * transformB.M21 + transformA.M22 * transformB.M22 +
                     transformA.M42 * transformB.M24;
        result.M23 = 0f;
        result.M24 = transformA.M12 * transformB.M41 + transformA.M22 * transformB.M42 +
                     transformA.M42 * transformB.M44;
        result.M31 = 0f;
        result.M32 = 0f;
        result.M33 = -1f;
        result.M34 = 0f;
        result.M41 = transformA.M14 * transformB.M11 + transformA.M24 * transformB.M12 +
                     transformA.M44 * transformB.M14;
        result.M42 = transformA.M14 * transformB.M21 + transformA.M24 * transformB.M22 +
                     transformA.M44 * transformB.M24;
        result.M43 = 0f;
        result.M44 = transformA.M14 * transformB.M41 + transformA.M24 * transformB.M42 +
                     transformA.M44 * transformB.M44;
    }

    public static void Translate(ref Transform transform, float x, float y)
    {
        var translation = new Transform(1f, 0f, x, 0f, 1f, y, 0f, 0f, 1f);

        Combine(ref transform, ref translation, out transform);
    }

    public static void Translate(ref Transform transform, ref Vector2 offset)
    {
        var translation = new Transform(1f, 0f, offset.X, 0f, 1f, offset.Y, 0f, 0f, 1f);
        Combine(ref transform, ref translation, out transform);
    }


    public static void Rotate(ref Transform transform, float angle)
    {
        float rad = Calc.ToRadians(angle);
        float cos = Calc.Cos(rad);
        float sin = Calc.Sin(rad);

        var rotation = new Transform(cos, -sin, 0f, sin, cos, 0f, 0f, 0f, 1f);

        Combine(ref transform, ref rotation, out transform);
    }

    public static void Rotate(ref Transform transform, float angle, float centerX, float centerY)
    {
        float rad = Calc.ToRadians(angle);
        float cos = Calc.Cos(rad);
        float sin = Calc.Sin(rad);

        var rotation = new Transform(
            cos, -sin, centerX * (1 - cos) + centerY * sin,
            sin, cos, centerY * (1 - cos) - centerX * sin,
            0f, 0f, 1f
        );

        Combine(ref transform, ref rotation, out transform);
    }


    public static void Rotate(ref Transform transform, float angle, Vector2 center)
    {
        Rotate(ref transform, angle, center.X, center.Y);
    }

    public static void Scale(ref Transform transform, float scaleX, float scaleY)
    {
        var scaling = new Transform(
            scaleX, 0f, 0f,
            0, scaleY, 0f,
            0f, 0f, 1f
        );

        Combine(ref transform, ref scaling, out transform);
    }

    public static void Scale(ref Transform transform, float scaleX, float scaleY, float centerX, float centerY)
    {
        var scaling = new Transform(
            scaleX, 0f, centerX * (1 - scaleX),
            0f, scaleY, centerY * (1 - scaleY),
            0f, 0f, 1f
        );

        Combine(ref transform, ref scaling, out transform);
    }

    public static void Scale(ref Transform transform, Vector2 factors)
    {
        Scale(ref transform, factors.X, factors.Y);
    }

    public static void Scale(ref Transform transform, Vector2 factors, Vector2 center)
    {
        Scale(ref transform, factors.X, factors.Y, center.X, center.Y);
    }

    public static bool operator ==(Transform left, Transform right)
    {
        return (
            left.M11 == right.M11 &&
            left.M12 == right.M12 &&
            left.M13 == right.M13 &&
            left.M14 == right.M14 &&
            left.M21 == right.M21 &&
            left.M22 == right.M22 &&
            left.M23 == right.M23 &&
            left.M24 == right.M24 &&
            left.M31 == right.M31 &&
            left.M32 == right.M32 &&
            left.M33 == right.M33 &&
            left.M34 == right.M34 &&
            left.M41 == right.M41 &&
            left.M42 == right.M42 &&
            left.M43 == right.M43 &&
            left.M44 == right.M44
        );
    }

    public static Transform Add(Transform matrix1, Transform matrix2)
    {
        matrix1.M11 += matrix2.M11;
        matrix1.M12 += matrix2.M12;
        matrix1.M13 += matrix2.M13;
        matrix1.M14 += matrix2.M14;
        matrix1.M21 += matrix2.M21;
        matrix1.M22 += matrix2.M22;
        matrix1.M23 += matrix2.M23;
        matrix1.M24 += matrix2.M24;
        matrix1.M31 += matrix2.M31;
        matrix1.M32 += matrix2.M32;
        matrix1.M33 += matrix2.M33;
        matrix1.M34 += matrix2.M34;
        matrix1.M41 += matrix2.M41;
        matrix1.M42 += matrix2.M42;
        matrix1.M43 += matrix2.M43;
        matrix1.M44 += matrix2.M44;
        return matrix1;
    }

    public static void Add(ref Transform matrix1, ref Transform matrix2, out Transform result)
    {
        result.M11 = matrix1.M11 + matrix2.M11;
        result.M12 = matrix1.M12 + matrix2.M12;
        result.M13 = matrix1.M13 + matrix2.M13;
        result.M14 = matrix1.M14 + matrix2.M14;
        result.M21 = matrix1.M21 + matrix2.M21;
        result.M22 = matrix1.M22 + matrix2.M22;
        result.M23 = matrix1.M23 + matrix2.M23;
        result.M24 = matrix1.M24 + matrix2.M24;
        result.M31 = matrix1.M31 + matrix2.M31;
        result.M32 = matrix1.M32 + matrix2.M32;
        result.M33 = matrix1.M33 + matrix2.M33;
        result.M34 = matrix1.M34 + matrix2.M34;
        result.M41 = matrix1.M41 + matrix2.M41;
        result.M42 = matrix1.M42 + matrix2.M42;
        result.M43 = matrix1.M43 + matrix2.M43;
        result.M44 = matrix1.M44 + matrix2.M44;

    }

    public static Transform CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
    {
        CreateOrthographic(width, height, zNearPlane, zFarPlane, out var matrix);
        return matrix;
    }

    public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Transform result)
    {
        result.M11 = 2f / width;
        result.M12 = result.M13 = result.M14 = 0f;
        result.M22 = 2f / height;
        result.M21 = result.M23 = result.M24 = 0f;
        result.M33 = 1f / (zNearPlane - zFarPlane);
        result.M31 = result.M32 = result.M34 = 0f;
        result.M41 = result.M42 = 0f;
        result.M43 = zNearPlane / (zNearPlane - zFarPlane);
        result.M44 = 1f;
    }

    public static Transform CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
    {
        CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane, out var matrix);
        return matrix;
    }

    public static Transform CreateOrthographicOffCenter(Rectangle viewingVolume, float zNearPlane, float zFarPlane)
    {
        CreateOrthographicOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, zNearPlane, zFarPlane, out var matrix);
        return matrix;
    }

    public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Transform result)
    {
        result.M11 = (float)(2.0 / (right - (double)left));
        result.M12 = 0.0f;
        result.M13 = 0.0f;
        result.M14 = 0.0f;
        result.M21 = 0.0f;
        result.M22 = (float)(2.0 / (top - (double)bottom));
        result.M23 = 0.0f;
        result.M24 = 0.0f;
        result.M31 = 0.0f;
        result.M32 = 0.0f;
        result.M33 = (float)(1.0 / (zNearPlane - (double)zFarPlane));
        result.M34 = 0.0f;
        result.M41 = (float)((left + (double)right) / (left - (double)right));
        result.M42 = (float)((top + (double)bottom) / (bottom - (double)top));
        result.M43 = (float)(zNearPlane / (zNearPlane - (double)zFarPlane));
        result.M44 = 1.0f;
    }

    public static Transform CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
    {
        Transform matrix;
        CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance, out matrix);
        return matrix;
    }

    public static void CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance, out Transform result)
    {
        if (nearPlaneDistance <= 0f)
        {
            throw new ArgumentException("nearPlaneDistance <= 0");
        }
        if (farPlaneDistance <= 0f)
        {
            throw new ArgumentException("farPlaneDistance <= 0");
        }
        if (nearPlaneDistance >= farPlaneDistance)
        {
            throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
        }

        var negFarRange = float.IsPositiveInfinity(farPlaneDistance) ? -1.0f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

        result.M11 = (2.0f * nearPlaneDistance) / width;
        result.M12 = result.M13 = result.M14 = 0.0f;
        result.M22 = (2.0f * nearPlaneDistance) / height;
        result.M21 = result.M23 = result.M24 = 0.0f;            
        result.M33 = negFarRange;
        result.M31 = result.M32 = 0.0f;
        result.M34 = -1.0f;
        result.M41 = result.M42 = result.M44 = 0.0f;
        result.M43 = nearPlaneDistance * negFarRange;
    }

    public static Transform CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        Transform result;
        CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
        return result;
    }

    public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Transform result)
    {
        if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
        {
            throw new ArgumentException("fieldOfView <= 0 or >= PI");
        }
        if (nearPlaneDistance <= 0f)
        {
            throw new ArgumentException("nearPlaneDistance <= 0");
        }
        if (farPlaneDistance <= 0f)
        {
            throw new ArgumentException("farPlaneDistance <= 0");
        }
        if (nearPlaneDistance >= farPlaneDistance)
        {
            throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
        }

        var yScale = 1.0f / (float)Math.Tan((double)fieldOfView * 0.5f);
        var xScale = yScale / aspectRatio;
        var negFarRange = float.IsPositiveInfinity(farPlaneDistance) ? -1.0f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

        result.M11 = xScale;
        result.M12 = result.M13 = result.M14 = 0.0f;
        result.M22 = yScale;
        result.M21 = result.M23 = result.M24 = 0.0f;
        result.M31 = result.M32 = 0.0f;            
        result.M33 = negFarRange;
        result.M34 = -1.0f;
        result.M41 = result.M42 = result.M44 = 0.0f;
        result.M43 = nearPlaneDistance * negFarRange;
    }

    public static Transform CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
    {
        Transform result;
        CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance, out result);
        return result;
    }

    public static Transform CreatePerspectiveOffCenter(Rectangle viewingVolume, float nearPlaneDistance, float farPlaneDistance)
    {
        Transform result;
        CreatePerspectiveOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, nearPlaneDistance, farPlaneDistance, out result);
        return result;
    }

    public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out Transform result)
    {
        if (nearPlaneDistance <= 0f)
        {
            throw new ArgumentException("nearPlaneDistance <= 0");
        }
        if (farPlaneDistance <= 0f)
        {
            throw new ArgumentException("farPlaneDistance <= 0");
        }
        if (nearPlaneDistance >= farPlaneDistance)
        {
            throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
        }
        result.M11 = (2f * nearPlaneDistance) / (right - left);
        result.M12 = result.M13 = result.M14 = 0;
        result.M22 = (2f * nearPlaneDistance) / (top - bottom);
        result.M21 = result.M23 = result.M24 = 0;
        result.M31 = (left + right) / (right - left);
        result.M32 = (top + bottom) / (top - bottom);
        result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        result.M34 = -1;
        result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        result.M41 = result.M42 = result.M44 = 0;
    }

    public static bool operator !=(Transform left, Transform right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is Transform transform)
        {
            return Equals(transform);
        }

        return false;
    }

    public bool Equals(Transform other)
    {
        return (
            M11 == other.M11 &&
            M12 == other.M12 &&
            M13 == other.M13 &&
            M14 == other.M14 &&
            M21 == other.M21 &&
            M22 == other.M22 &&
            M23 == other.M23 &&
            M24 == other.M24 &&
            M31 == other.M31 &&
            M32 == other.M32 &&
            M33 == other.M33 &&
            M34 == other.M34 &&
            M41 == other.M41 &&
            M42 == other.M42 &&
            M43 == other.M43 &&
            M44 == other.M44
        );
    }

    public override int GetHashCode()
    {
        return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
               M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
               M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
               M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
    }
}