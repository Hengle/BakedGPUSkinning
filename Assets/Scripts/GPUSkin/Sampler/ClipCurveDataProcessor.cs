using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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


    public SDAnimationClipData clipData { get; private set; }

    private AnimationClip   _clip;
    private CurveType       _currCurveType;
    private int             _boneIdx;
    private string          _currPath;

    /// <summary>
    /// key: bone path, value: curve
    /// </summary>
    private Dictionary<string, RotationCurve>   _rotCurves;
    private Dictionary<string, Vector3Curve>    _posCurves;
    private Dictionary<string, Vector3Curve>    _scaleCurves;

    public ClipCurveDataProcessor(AnimationClip clip)
    {
        _clip = clip;
        clipData = new SDAnimationClipData();
        ProcessClip();
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
                SetRotCurveData(rotCurve, curve);
                break;
            case CurveType.Position:
                Vector3Curve posCurve = GetOrAddPositionCurve(binding.path);
                SetPosCurveData(posCurve, curve);
                break;
            case CurveType.Scale:
                Vector3Curve scaleCurve = GetOrAddScaleCurve(binding.path);
                SetScaleCurveData(scaleCurve, curve);
                break;
            default:
                break;
        }
    }


    private CurveType GetCurveType(EditorCurveBinding binding)
    {
        return CurveType.None;
    }

    private void SetRotCurveData(RotationCurve rotCurve, AnimationCurve curve)
    {
        if(rotCurve.keys == null)
            rotCurve.keys = new RotationKeyFrame[curve.keys.Length];
        

    }

    private void SetPosCurveData(Vector3Curve posCurve, AnimationCurve curve)
    {

    }

    private void SetScaleCurveData(Vector3Curve posCurve, AnimationCurve curve)
    {

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
}
