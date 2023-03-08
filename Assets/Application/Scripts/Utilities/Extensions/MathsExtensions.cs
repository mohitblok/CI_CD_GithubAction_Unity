using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class MathsExtensions
{
    public static Vector3 WrapTo180(this Quaternion q) => q.eulerAngles.WrapTo180();
    public static Vector3 WrapTo360(this Quaternion q) => q.eulerAngles.WrapTo360();

    public static Vector3 WrapTo180(this Vector3 euler)
    {
        euler.x = euler.x.WrapTo180();
        euler.y = euler.y.WrapTo180();
        euler.z = euler.z.WrapTo180();
        return euler;
    }

    public static Vector3 WrapTo360(this Vector3 euler)
    {
        euler.x = euler.x.WrapTo360();
        euler.y = euler.y.WrapTo360();
        euler.z = euler.z.WrapTo360();
        return euler;
    }

    public static float WrapTo180(this float angle)
    {
        while (angle >= 180)
        {
            angle -= 360f;
        }
        while (angle < -180)
        {
            angle += 360f;
        }
        return angle;
    }

    public static float WrapTo360(this float angle)
    {
        while (angle >= 360)
        {
            angle -= 360f;
        }
        while (angle < 0)
        {
            angle += 360f;
        }
        return angle;
    }

    public static float AngleSigned(this Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            n.Dot(v1.Cross(v2)),
            v1.Dot(v2)).ToDegrees();
    }

    public static Vector3 Cross(this Vector3 v1, Vector3 v2) => Vector3.Cross(v1, v2);
    public static float Dot(this Vector3 v1, Vector3 v2) => Vector3.Dot(v1, v2);

    public static Quaternion Delta(this Quaternion q1, Quaternion q2) => q1.Inverse() * q2;
    public static Quaternion Inverse(this Quaternion q) => Quaternion.Inverse(q);
    public static Quaternion InverseTransform(this Quaternion q, Transform t) => t.rotation.Inverse() * q;

    public static float ToDegrees(this float radians) => radians * Mathf.Rad2Deg;
    public static float ToRadians(this float degress) => degress * Mathf.Deg2Rad;
    public static double ToDegrees(this double radians) => radians * Mathf.Rad2Deg;
    public static double ToRadians(this double degrees) => degrees * Mathf.Deg2Rad;

    public static float GetClosest(this List<float> list, float target)
    {
        int index;
        return list.GetClosest(target, out index);
    }

    public static float GetClosest(this List<float> list, float target, out int index)
    {
        // list must be pre-sorted in ascending order
        float delta = float.MaxValue;
        float value = 0;
        index = 0;
        for (int i = 0; i < list.Count; ++i)
        {
            float d = (list[i] - target).Abs();
            if (d < delta)
            {
                delta = d;
                value = list[i];
                index = i;
            }
            else
            {
                break;
            }
        }
        return value;
    }

    public static float[] ToFloatArray(this Matrix4x4 m)
    {
        return new float[]
        {
            m[0,0], m[0,1], m[0,2], m[0,3],
            m[1,0], m[1,1], m[1,2], m[1,3],
            m[2,0], m[2,1], m[2,2], m[2,3],
            m[3,0], m[3,1], m[3,2], m[3,3],
        };
    }

    public static float[] ToFloatArray(this Vector3 v) => new float[] { v.x, v.y, v.z };

    public static Vector2 Inverse(this Vector2 v)
    {
        v.x = 1f / v.x;
        v.y = 1f / v.y;
        return v;
    }

    public static Vector3 Inverse(this Vector3 v)
    {
        v.x = 1f / v.x;
        v.y = 1f / v.y;
        v.z = 1f / v.z;
        return v;
    }

    public static Vector2 Multiply(this Vector2 v, Vector2 scale) => v.Multiply(scale.x, scale.y);

    public static Vector2 Multiply(this Vector2 v, float x, float y)
    {
        v.x *= x;
        v.y *= y;
        return v;
    }

    public static Vector3 Multiply(this Vector3 v, float x, float y, float z) => v.Multiply(new Vector3(x, y, z));
    public static Vector3 Multiply(this Vector3 v, Vector3 scale)
    {
        for (int i = 0; i < 3; ++i)
        {
            v[i] *= scale[i];
        }
        return v;
    }

    public static Vector2 Divide(this Vector2 v, Vector2 scale) => v.Divide(scale.x, scale.y);

    public static Vector2 Divide(this Vector2 v, float x, float y)
    {
        v.x /= x;
        v.y /= y;
        return v;
    }

    public static Vector3 Divide(this Vector3 v, float x, float y, float z) => v.Divide(new Vector3(x, y, z));
    public static Vector3 Divide(this Vector3 v, Vector3 scale)
    {
        for (int i = 0; i < 3; ++i)
        {
            v[i] /= scale[i];
        }
        return v;
    }

    public static Vector2 NextPowerOfTwo(this Vector2 v)
    {
        v.x = v.x.NextPowerOfTwo();
        v.y = v.y.NextPowerOfTwo();
        return v;
    }

    public static int NextPowerOfTwo(this float f) => NextPowerOfTwo((int)f);
    public static int NextPowerOfTwo(this int i)
    {
        i--; // comment out to always take the next biggest power of two, even if x is already a power of two
        i |= (i >> 1);
        i |= (i >> 2);
        i |= (i >> 4);
        i |= (i >> 8);
        i |= (i >> 16);
        return (i + 1);
    }

    public static Vector2 Floor(this Vector2 v) => new Vector2(v.x.Floor(), v.y.Floor());
    public static Vector3 Floor(this Vector3 v) => new Vector3(v.x.Floor(), v.y.Floor(), v.z.Floor());
    public static Vector2 Ceil(this Vector2 v) => new Vector2(v.x.Ceil(), v.y.Ceil());
    public static Vector3 Ceil(this Vector3 v) => new Vector3(v.x.Ceil(), v.y.Ceil(), v.z.Ceil());

    public static Vector3 Max(this Vector3 a, Vector3 b) => new Vector3(a.x.Max(b.x), a.y.Max(b.y), a.z.Max(b.z));
    public static Vector3 Min(this Vector3 a, Vector3 b) => new Vector3(a.x.Min(b.x), a.y.Min(b.y), a.z.Min(b.z));

    public static Vector2 ToVector2(this float f) => new Vector2(f, f);
    public static Vector3 ToVector3(this float f) => new Vector3(f, f, f);

    public static Vector2 Clamp(this Vector2 v, float min, float max) => v.Clamp(min.ToVector2(), max.ToVector2());

    public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
    {
        v.x = v.x.Clamp(min.x, max.x);
        v.y = v.y.Clamp(min.y, max.y);
        return v;
    }

    public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
    {
        v.x = v.x.Clamp(min.x, max.x);
        v.y = v.y.Clamp(min.y, max.y);
        v.z = v.z.Clamp(min.z, max.z);
        return v;
    }

    public static double Clamp(this double d, double min, double max)
    {
        if (d < min) { return min; }
        if (d > max) { return max; }
        return d;
    }

    public static float Clamp(this float f, float min, float max)
    {
        if (f < min) { return min; }
        if (f > max) { return max; }
        return f;
    }

    public static int Clamp(this int i, int min, int max)
    {
        if (i < min) { return min; }
        if (i > max) { return max; }
        return i;
    }

    public static int Clamp(this int index, IList list) => index.Clamp(0, list.Count - 1);

    public static int Wrap(this int index, IList list) => index.Wrap(list.Count);
    public static int Wrap(this int index, int count) => (index + count) % count;

    public static double Min(this double a, double b) => (a < b) ? a : b;
    public static double Max(this double a, double b) => (a > b) ? a : b;

    public static float Min(this float a, float b) => (a < b) ? a : b;
    public static float Max(this float a, float b) => (a > b) ? a : b;

    public static int Min(this int a, int b) => (a < b) ? a : b;
    public static int Max(this int a, int b) => (a > b) ? a : b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(this float f) => Mathf.Abs(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(this int i) => Mathf.Abs(i);

    public static Vector3 Abs(this Vector3 v)
    {
        v.x = v.x.Abs();
        v.y = v.y.Abs();
        v.z = v.z.Abs();
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this Vector3 v, Func<Vector3, float> x, Func<Vector3, float> y)
    {
        return new Vector2(x(v), y(v));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MinComponent(this Vector3 v) => v.x.Min(v.y).Min(v.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MaxComponent(this Vector3 v) => v.x.Max(v.y).Max(v.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sign(this int f) => (int)Mathf.Sign(f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sign(this float f) => Mathf.Sign(f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Log10(this float f)
    {
        if (f == 0) { return 0; }
        return Mathf.Log10(f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Log(this float f, float baseNumber)
    {
        if (f == 0) { return 0; }
        return Mathf.Log(f, baseNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Log10(this double d) => Math.Log10(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Sqrt(this double d) => Math.Sqrt(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Exp(this double d) => Math.Exp(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow(this int i, float pow) => Mathf.Pow(i, pow);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow(this float f, float pow) => Mathf.Pow(f, pow);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow(this double d, double pow) => Math.Pow(d, pow);

    public static float Acos(this float f) => (float)Math.Acos(f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(this float t, float f0, float f1)
    {
        if (f0 < f1)
        {
            if (t <= 0f)
                return f0;
            else if (t >= 1f)
                return f1;
        }
        else
        {
            if (t >= 1f)
                return f1;
            else if (t <= 0f)
                return f0;
        }
        return ExtraLerp(t, f0, f1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ExtraLerp(this float t, float f0, float f1) => (1f - t) * f0 + t * f1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ExtraLerp(this float t, Vector3 v0, Vector3 v1) => (1f - t) * v0 + t * v1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(this float f, float min, float max)
    {
        return Mathf.InverseLerp(min, max, f);
    }

    public static float InverseLerp(this double d, double min, double max)
    {
        return ((float)d).InverseLerp((float)min, (float)max);
    }

    public static float InverseLerp(this float f, List<float> values)
    {
        if (values.Count > 1)
        {
            if (f < values.First())
            {
                return 0.0f;
            }
            else if (f > values.Last())
            {
                return 1.0f;
            }
        }
        if (values.Count > 2)
        {
            int next = 1;
            for (int i = 1; i < values.Count; ++i)
            {
                if (values[i] >= f)
                {
                    next = i;
                    break;
                }
            }
            int start = next - 1;

            float len = values.Count;
            float s = values[start];
            float n = values[next];
            float fT = f.InverseLerp(s, n);
            float t = fT.Lerp(start, next);
            return (t / len);
        }
        return 0.0f;
    }

    public static Vector3 InverseLerp(this Vector3 v, Vector3 min, Vector3 max)
    {
        for (int i = 0; i < 3; ++i)
        {
            v[i] = v[i].InverseLerp(min[i], max[i]);
        }
        return v;
    }

    public static Vector3 InverseLerp(this Vector3 v, float min, float max)
    {
        for (int i = 0; i < 3; ++i)
        {
            v[i] = v[i].InverseLerp(min, max);
        }
        return v;
    }

    public static bool IsInRange(this float value, float min, float max)
    {
        return (value >= min && value <= max);
    }

    public static Vector2 Frac(this Vector2 v)
    {
        v.x = Frac(v.x);
        v.y = Frac(v.y);
        return v;
    }

    public static double Frac(this double d) => d - Math.Truncate(d);
    public static float Frac(this float f) => (float)Frac((double)f);

    public static double Round(this double d, int places) => Math.Round(d, places);
    public static float Round(this float f, int places) => (float)Round((double)f, places);

    public static Vector3 Round(this Vector3 v, int places)
    {
        v.x = v.x.Round(places);
        v.y = v.y.Round(places);
        v.z = v.z.Round(places);
        return v;
    }

    public static Vector3 RoundToNearest(this Vector3 v, float increment)
    {
        v.x = v.x.RoundToNearest(increment);
        v.y = v.y.RoundToNearest(increment);
        v.z = v.z.RoundToNearest(increment);
        return v;
    }
    public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
    {
        Vector3 c = current.eulerAngles;
        Vector3 t = target.eulerAngles;
        return Quaternion.Euler(
            Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
            Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
            Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
        );
    }
    public static float RoundDownTo(this float f, float increment) => (f / increment).Floor() * increment;
    public static float RoundDownTo(this int i, float increment) => RoundDownTo((float)i, increment);

    public static float RoundUpTo(this float f, float increment) => (f / increment).Ceil() * increment;
    public static float RoundUpTo(this int i, float increment) => RoundUpTo((float)i, increment);

    public static float RoundToNearest(this double f, float increment) => (f / increment).Round() * increment;
    public static float RoundToNearest(this float f, float increment) => RoundToNearest((double)f, increment);
    public static float RoundToNearest(this int i, float increment) => RoundToNearest((double)i, increment);

    public static float RoundDownToNearestEvenNumber(this float f) => (int)(f * 0.5f) * 2f;

    public static int Round(this double d) => Convert.ToInt32(d);
    public static int Round(this float f) => Convert.ToInt32(f);
    public static int Floor(this float f) => Mathf.FloorToInt(f);
    public static int Ceil(this float f) => Mathf.CeilToInt(f);

    public static float LinearToDecibels(this float m) => (float)LinearToDecibels((double)m);
    public static double LinearToDecibels(this double m) => 20.0 * m.Log10();

    public static float DecibelsToLinear(this float db) => (float)DecibelsToLinear((double)db);
    public static double DecibelsToLinear(this double db) => (10.0).Pow(db / 20.0);
}
