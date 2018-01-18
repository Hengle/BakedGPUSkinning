using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SDClipCurveData
{
    public RotationCurve[]  rotationCurves;
    public Vector3Curve[]   positionCurves;
    public Vector3Curve[]   scaleCurves;
}

[System.Serializable]
public class RotationCurve
{
    public string               path;
    public RotationKeyFrame[]   keys;
    public byte                 preInfinity;
    public byte                 postInfinity;
    public byte                 rotationOrder;
}

[System.Serializable]
public class Vector3Curve
{
    public string               path;
    public Vector3KeyFrame[]    keys;
    public byte                 preInfinity;
    public byte                 postInfinity;
    public byte                 rotationOrder;
}

[System.Serializable]
public struct RotationKeyFrame
{
    public float        time;
    public Quaternion   value;
    public Quaternion   inSlope;
    public Quaternion   outSlope;
}

[System.Serializable]
public struct Vector3KeyFrame
{
    public float    time;
    public Vector3  value;
    public Vector3  inSlope;
    public Vector3  outSlope;
}