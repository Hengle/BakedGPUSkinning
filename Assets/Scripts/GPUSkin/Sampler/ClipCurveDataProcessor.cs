using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// 从 AnimationClip 中提取 AnimationCurveData 的类
/// </summary>
public class ClipCurveDataProcessor
{
    enum CurveType
    {
        None,
        Rotation,
        Position,
        Scale,
        Float,
    }


    public SDClipCurveData curveData { get; private set; }

    private AnimationClip   _clip;

    /// <summary>
    /// key: bone path, value: curve
    /// </summary>
    private Dictionary<string, RotationCurve>   _rotCurves = new Dictionary<string, RotationCurve>();
    private Dictionary<string, Vector3Curve>    _posCurves = new Dictionary<string, Vector3Curve>();
    private Dictionary<string, Vector3Curve>    _scaleCurves = new Dictionary<string, Vector3Curve>();

    public ClipCurveDataProcessor(AnimationClip clip)
    {
        _clip = clip;
        curveData = new SDClipCurveData();

        ProcessClip();

        curveData.rotationCurves = _rotCurves.Values.ToArray();
        curveData.positionCurves = _posCurves.Values.ToArray();
        if (CheckIfHasScaleCurves())
            curveData.scaleCurves = _scaleCurves.Values.ToArray();
        else
            curveData.scaleCurves = null;
        
    }

    private void ProcessClip()
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_clip);
        foreach (var binding in bindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(_clip, binding);
            ProcessCurve(curve, binding);
        }
    }

    private void ProcessCurve(AnimationCurve curve, EditorCurveBinding binding)
    {
        CurveType curveType = GetCurveType(binding);
        switch (curveType)
        {
            case CurveType.Rotation:
                RotationCurve rotCurve = GetOrAddRotationCurve(binding.path);
                SetRotCurveData(rotCurve, curve, binding.propertyName);
                break;
            case CurveType.Position:
                Vector3Curve posCurve = GetOrAddPositionCurve(binding.path);
                SetPosCurveData(posCurve, curve, binding.propertyName);
                break;
            case CurveType.Scale:
                Vector3Curve scaleCurve = GetOrAddScaleCurve(binding.path);
                SetScaleCurveData(scaleCurve, curve, binding.propertyName);
                break;
            default:
                break;
        }
    }


    private CurveType GetCurveType(EditorCurveBinding binding)
    {
        if (binding.propertyName.StartsWith("m_LocalRotation"))
            return CurveType.Rotation;
        if (binding.propertyName.StartsWith("m_LocalPosition"))
            return CurveType.Position;
        if (binding.propertyName.StartsWith("m_LocalScale"))
            return CurveType.Scale;
        return CurveType.None;
    }

    private void SetRotCurveData(RotationCurve rotCurve, AnimationCurve curve, string propertyName)
    {
        int cnt = curve.keys.Length;
        Keyframe[] srcKeys = curve.keys;

        if(rotCurve.keys == null)
            rotCurve.keys = new RotationKeyFrame[cnt];
        
        if(propertyName.EndsWith(".x"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                rotCurve.keys[i].time       = keyFrame.time;
                rotCurve.keys[i].value.x    = keyFrame.value;
                rotCurve.keys[i].inSlope.x  = keyFrame.inTangent;
                rotCurve.keys[i].outSlope.x = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".y"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                rotCurve.keys[i].time       = keyFrame.time;
                rotCurve.keys[i].value.y    = keyFrame.value;
                rotCurve.keys[i].inSlope.y  = keyFrame.inTangent;
                rotCurve.keys[i].outSlope.y = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".z"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                rotCurve.keys[i].time       = keyFrame.time;
                rotCurve.keys[i].value.z    = keyFrame.value;
                rotCurve.keys[i].inSlope.z  = keyFrame.inTangent;
                rotCurve.keys[i].outSlope.z = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".w"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                rotCurve.keys[i].time       = keyFrame.time;
                rotCurve.keys[i].value.w    = keyFrame.value;
                rotCurve.keys[i].inSlope.w  = keyFrame.inTangent;
                rotCurve.keys[i].outSlope.w = keyFrame.outTangent;
            }
        }
    }

    private void SetPosCurveData(Vector3Curve posCurve, AnimationCurve curve, string propertyName)
    {
        int cnt = curve.keys.Length;
        Keyframe[] srcKeys = curve.keys;

        if(posCurve.keys == null)
            posCurve.keys = new Vector3KeyFrame[cnt];
        
        if(propertyName.EndsWith(".x"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                posCurve.keys[i].time       = keyFrame.time;
                posCurve.keys[i].value.x    = keyFrame.value;
                posCurve.keys[i].inSlope.x  = keyFrame.inTangent;
                posCurve.keys[i].outSlope.x = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".y"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                posCurve.keys[i].time       = keyFrame.time;
                posCurve.keys[i].value.y    = keyFrame.value;
                posCurve.keys[i].inSlope.y  = keyFrame.inTangent;
                posCurve.keys[i].outSlope.y = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".z"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame           = srcKeys[i];
                posCurve.keys[i].time       = keyFrame.time;
                posCurve.keys[i].value.z    = keyFrame.value;
                posCurve.keys[i].inSlope.z  = keyFrame.inTangent;
                posCurve.keys[i].outSlope.z = keyFrame.outTangent;
            }
        }
    }

    private void SetScaleCurveData(Vector3Curve scaleCurve, AnimationCurve curve, string propertyName)
    {
        int cnt = curve.keys.Length;
        Keyframe[] srcKeys = curve.keys;

        if(scaleCurve.keys == null)
            scaleCurve.keys = new Vector3KeyFrame[cnt];
        
        if(propertyName.EndsWith(".x"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame             = srcKeys[i];
                scaleCurve.keys[i].time       = keyFrame.time;
                scaleCurve.keys[i].value.x    = keyFrame.value;
                scaleCurve.keys[i].inSlope.x  = keyFrame.inTangent;
                scaleCurve.keys[i].outSlope.x = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".y"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame             = srcKeys[i];
                scaleCurve.keys[i].time       = keyFrame.time;
                scaleCurve.keys[i].value.y    = keyFrame.value;
                scaleCurve.keys[i].inSlope.y  = keyFrame.inTangent;
                scaleCurve.keys[i].outSlope.y = keyFrame.outTangent;
            }
        }else if (propertyName.EndsWith(".z"))
        {
            for(int i = 0; i < cnt; i++)
            {
                Keyframe keyFrame             = srcKeys[i];
                scaleCurve.keys[i].time       = keyFrame.time;
                scaleCurve.keys[i].value.z    = keyFrame.value;
                scaleCurve.keys[i].inSlope.z  = keyFrame.inTangent;
                scaleCurve.keys[i].outSlope.z = keyFrame.outTangent;
            }
        }
    }


    private RotationCurve GetOrAddRotationCurve(string path)
    {
        RotationCurve ret;
        if(!_rotCurves.TryGetValue(path, out ret))
        {
            ret = new RotationCurve();
            ret.path = path;
            _rotCurves.Add(path, ret);
        }
        return ret;
    }

    private Vector3Curve GetOrAddPositionCurve(string path)
    {
        Vector3Curve ret;
        if (!_posCurves.TryGetValue(path, out ret))
        {
            ret = new Vector3Curve();
            ret.path = path;
            _posCurves.Add(path, ret);
        }
        return ret;
    }

    private Vector3Curve GetOrAddScaleCurve(string path)
    {
        Vector3Curve ret;
        if (!_scaleCurves.TryGetValue(path, out ret))
        {
            ret = new Vector3Curve();
            ret.path = path;
            _scaleCurves.Add(path, ret);
        }
        return ret;
    }

    /// <summary>
    /// 绝大部分动画都没有 Scale Curve
    /// </summary>
    /// <returns></returns>
    private bool CheckIfHasScaleCurves()
    {
        bool ret = false;
        foreach(Vector3Curve curve in _scaleCurves.Values)
        {
            if (curve.keys == null || curve.keys.Length == 0)
                continue;

            if(curve.keys.Length == 2)
            {
                if(curve.keys[0].value != Vector3.one || curve.keys[1].value != Vector3.one)
                {
                    ret = true;
                    break;
                }
            }
        }

        return ret;
    }
}
