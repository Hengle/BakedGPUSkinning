using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public enum RotationOrder
    {
        XYZ,
        XZY,
        YZX,
        YXZ,
        ZXY,
        ZYX,
        Last = ZYX,
        Default = ZXY,
    }


    [System.Serializable]
    public class ClipCurveData
    {
        public string           path;
        public RotationCurve    rotationCurve;
        public Vector3Curve     positionCurve;
        public Vector3Curve     scaleCurve;
    }

    [System.Serializable]
    public class RotationCurve
    {
        public RotationKeyFrame[]   keys;
        public WrapMode             preWrapMode;
        public WrapMode             postWrapMode;
        //public RotationOrder        rotationOrder; // 只有 Editor 计算 inSlope/outSlope 时用得到
    }

    [System.Serializable]
    public class Vector3Curve
    {
        public Vector3KeyFrame[]    keys;
        public WrapMode             preWrapMode;
        public WrapMode             postWrapMode;
        //public RotationOrder        rotationOrder;
    }

    [System.Serializable]
    public struct RotationKeyFrame
    {
        public float        time;
        public Quaternion   value;
        public Quaternion   inSlope;
        public Quaternion   outSlope;

        /// <summary>
        /// 两个帧的数据是否相等, 不比较时间
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(RotationKeyFrame lhs, RotationKeyFrame rhs)
        {
            return GPUAnimUtils.IsQuaternionEqual(lhs.value, rhs.value)
                && GPUAnimUtils.IsQuaternionEqual(lhs.inSlope, rhs.inSlope)
                && GPUAnimUtils.IsQuaternionEqual(lhs.outSlope, rhs.outSlope);
        }

        public static bool operator !=(RotationKeyFrame lhs, RotationKeyFrame rhs)
        {
            return !GPUAnimUtils.IsQuaternionEqual(lhs.value, rhs.value)
                || !GPUAnimUtils.IsQuaternionEqual(lhs.inSlope, rhs.inSlope)
                || !GPUAnimUtils.IsQuaternionEqual(lhs.outSlope, rhs.outSlope);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(RotationKeyFrame))
                return false;

            RotationKeyFrame rhs = (RotationKeyFrame)obj;
            return Mathf.Approximately(rhs.time, this.time) && rhs == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [System.Serializable]
    public struct Vector3KeyFrame
    {
        public float    time;
        public Vector3  value;
        public Vector3  inSlope;
        public Vector3  outSlope;

        /// <summary>
        /// 两个帧的数据是否相等, 不比较时间
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Vector3KeyFrame lhs, Vector3KeyFrame rhs)
        {
            return GPUAnimUtils.IsVector3Equal(lhs.value, rhs.value)
                && GPUAnimUtils.IsVector3Equal(lhs.inSlope, rhs.inSlope)
                && GPUAnimUtils.IsVector3Equal(lhs.outSlope, rhs.outSlope);
        }

        public static bool operator !=(Vector3KeyFrame lhs, Vector3KeyFrame rhs)
        {
            return !GPUAnimUtils.IsVector3Equal(lhs.value, rhs.value)
                || !GPUAnimUtils.IsVector3Equal(lhs.inSlope, rhs.inSlope)
                || !GPUAnimUtils.IsVector3Equal(lhs.outSlope, rhs.outSlope);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Vector3KeyFrame))
                return false;

            Vector3KeyFrame rhs = (Vector3KeyFrame)obj;
            return Mathf.Approximately(rhs.time, this.time) && rhs == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
