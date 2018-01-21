using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurveVec3
{
       private Vector3Curve _curveData;
    private short _keyCount;

    public AnimationCurveVec3(Vector3Curve curveData)
    {
        _curveData = curveData;
        _keyCount = (short)_curveData.keys.Length;
    }

    public KeyValuePair<float, float> GetRange()
    {
        return new KeyValuePair<float, float>();
    }

    public Vector3 Evaluate(float curveT)
    {
        if (_keyCount == 0)
            return Vector3.zero;

        if(_keyCount == 1)
        {
            return _curveData.keys[0].value;
        }

        curveT = WrapTime(curveT);
        int lhsIndex = 0, rhsIndex = 0;
        FindIndexForSampling(curveT, ref lhsIndex, ref rhsIndex);
        Vector3KeyFrame lhsKey = _curveData.keys[lhsIndex];
        Vector3KeyFrame rhsKey = _curveData.keys[rhsIndex];

        float dx = rhsKey.time - lhsKey.time;
        Vector3 m1, m2;
        float t;
        if(dx != 0f)
        {
            t = (curveT - lhsKey.time) / dx;
            m1 = lhsKey.outSlope * dx;
            m2 = rhsKey.inSlope * dx;
        }
        else
        {
            t = 0f;
            m1 = Vector3.zero;
            m2 = Vector3.zero;
        }

        Vector3 ret = CurveUtils.HermiteInterpolate(t, lhsKey.value, m1, m2, rhsKey.value);
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
        while(rangeLen > 0)
        {
            int halfLen = rangeLen >> 1;
            int mid = first + halfLen;

            if(curveT < _curveData.keys[mid].time)
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
