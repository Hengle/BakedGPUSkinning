using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public static class CurveUtils
    {
        public static float Repeat(float t, float length)
        {
            return t - Mathf.Floor(t / length) * length; // TODO t % length ?
        }

        public static float Repeat(float t, float begin, float end)
        {
            return Repeat(t - begin, end - begin) + begin;
        }

        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            t = length - Mathf.Abs(t - length);
            return t;
        }

        public static float PingPong(float t, float begin, float end)
        {
            return PingPong(t - begin, end - begin) + begin;
        }

        public static Quaternion MultyScalar(this Quaternion lhs, float rhs)
        {
            return new Quaternion(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
        }

        public static Quaternion Add(this Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
        }

        public static Quaternion HermiteInterpolate(float t, Quaternion p0, Quaternion m0, Quaternion m1, Quaternion p1)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float a = 2f * t3 - 3f * t2 + 1f;
            float b = t3 - 2f * t2 + t;
            float c = t3 - t2;
            float d = -2f * t3 + 3f * t2;

            return p0.MultyScalar(a).Add(m0.MultyScalar(b)).Add(m1.MultyScalar(c)).Add(p1.MultyScalar(d));
        }

        public static Vector3 HermiteInterpolate(float t, Vector3 p0, Vector3 m0, Vector3 m1, Vector3 p1)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float a = 2f * t3 - 3f * t2 + 1f;
            float b = t3 - 2f * t2 + t;
            float c = t3 - t2;
            float d = -2f * t3 + 3f * t2;

            return p0 * a + m0 * b + m1 * c + p1 * d;
        }

        public static void HandleSteppedCurve(ref RotationKeyFrame lhs, ref RotationKeyFrame rhs, ref Quaternion value)
        {
            if (lhs.outSlope[0] == Mathf.Infinity || rhs.inSlope[0] == Mathf.Infinity
                || lhs.outSlope[1] == Mathf.Infinity || rhs.inSlope[1] == Mathf.Infinity
                || lhs.outSlope[2] == Mathf.Infinity || rhs.inSlope[2] == Mathf.Infinity
                || lhs.outSlope[3] == Mathf.Infinity || rhs.inSlope[3] == Mathf.Infinity)
                value = lhs.value;
        }

        public static void HandleSteppedCurve(ref Vector3KeyFrame lhs, ref Vector3KeyFrame rhs, ref Vector3 value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (lhs.outSlope[i] == Mathf.Infinity || rhs.inSlope[i] == Mathf.Infinity)
                    value[i] = lhs.value[i];
            }
        }
    }

}
