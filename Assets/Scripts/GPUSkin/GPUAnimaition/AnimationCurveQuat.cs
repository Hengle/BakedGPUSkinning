using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    /// <summary>
    /// Rotation 曲线, 目前先不做 cache
    /// </summary>
    public class AnimationCurveQuat
    {
        private RotationCurve   _curveData;
        private short           _keyCount;

        public AnimationCurveQuat(RotationCurve curveData)
        {
            _curveData = curveData;
            _keyCount = (short)_curveData.keys.Length;
        }

        public KeyValuePair<float, float> GetRange()
        {
            return new KeyValuePair<float, float>();
        }

        public Quaternion Evaluate(float curveT)
        {
            if (_keyCount == 0)
                return Quaternion.identity;

            if (_keyCount == 1)
            {
                return _curveData.keys[0].value;
            }

            curveT = WrapTime(curveT);
            int lhsIndex = 0, rhsIndex = 0;
            FindIndexForSampling(curveT, ref lhsIndex, ref rhsIndex);
            RotationKeyFrame lhsKey = _curveData.keys[lhsIndex];
            RotationKeyFrame rhsKey = _curveData.keys[rhsIndex];

            float dx = rhsKey.time - lhsKey.time;
            Quaternion m1, m2;
            float t;
            if (dx != 0f)
            {
                t = (curveT - lhsKey.time) / dx;
                m1 = lhsKey.outSlope.MultyScalar(dx);
                m2 = rhsKey.inSlope.MultyScalar(dx);
            }
            else
            {
                t = 0f;
                m1 = Quaternion.identity;
                m2 = Quaternion.identity;
            }

            Quaternion ret = CurveUtils.HermiteInterpolate(t, lhsKey.value, m1, m2, rhsKey.value);
            CurveUtils.HandleSteppedCurve(ref lhsKey, ref rhsKey, ref ret);

            return ret;
        }

        private float WrapTime(float curveT)
        {
            float beginTime = _curveData.keys[0].time;
            float endTime = _curveData.keys[_keyCount - 1].time;

            switch (_curveData.preWrapMode)
            {
                case WrapMode.Clamp:
                case WrapMode.ClampForever:
                    if (curveT < beginTime)
                        curveT = beginTime;
                    else if (curveT > endTime)
                        curveT = endTime;
                    break;
                case WrapMode.PingPong:
                    curveT = CurveUtils.PingPong(curveT, beginTime, endTime);
                    break;
                default:
                    curveT = CurveUtils.Repeat(curveT, beginTime, endTime);
                    break;
            }

            return curveT;
        }

        /// <summary>
        /// 找到当前时间比其大的第一个索引
        /// </summary>
        /// <param name="curveT"></param>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        private void FindIndexForSampling(float curveT, ref int lhs, ref int rhs)
        {
            Debug.Assert(curveT >= _curveData.keys[0].time && curveT <= _curveData.keys[_keyCount - 1].time);

            int first = 0;
            int rangeLen = _keyCount;
            while (rangeLen > 0)
            {
                int halfLen = rangeLen >> 1;
                int mid = first + halfLen;

                if (curveT < _curveData.keys[mid].time)
                    rangeLen = halfLen;
                else
                {
                    first = mid + 1;
                    rangeLen = halfLen - 1;
                }
            }

            lhs = first - 1;
            rhs = Mathf.Min(first, _keyCount - 1);
        }
    }

}
