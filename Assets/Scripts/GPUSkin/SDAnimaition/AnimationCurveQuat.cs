using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotation 曲线, 目前先不做 cache
/// </summary>
public class AnimationCurveQuat
{
    private RotationCurve _curveData;
    private short _keyCount;

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
        return Quaternion.identity;
    }

    private float WrapTime(float curveT)
    {
        float beginTime = _curveData.keys[0].time;
        float endTime = _curveData.keys[_keyCount - 1].time;

        if(curveT < beginTime)
        {

        }else if(curveT > endTime)
        {

        }
        return curveT;
    }
}
