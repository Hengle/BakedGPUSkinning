using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IRuntimeBoneInfo
{
    TRS trs { get; }
}



public class SDAnimationState
{
    public IRuntimeBoneInfo[]   runtimeBoneInfos { get { return _runtimeBoneInfos; } }


    private class RuntimeBoneInfo : IRuntimeBoneInfo
    {
        public TRS                  trs { get; set; }
        public AnimationCurveQuat   rotCurve { get; set; }
        public AnimationCurveVec3   posCurve { get; set; }
        public AnimationCurveVec3   scaleCurve { get; set; }
    }


    private SkinningData        _skinningData;
    private BakedClipInfo       _clipInfo;
    private RuntimeBoneInfo[]   _runtimeBoneInfos;

    public SDAnimationState(SkinningData skinningData, int clipIdx)
    {
        _skinningData = skinningData;
        _clipInfo = _skinningData.clipInfos[clipIdx];
        Init();
    }

    public SDAnimationState(SkinningData skinningData, string clipName)
    {
        _skinningData = skinningData;
        foreach(var clip in _skinningData.clipInfos)
        {
            if(clip.name == clipName)
            {
                _clipInfo = clip;
                break;
            }
        }
        Init();
    }

    public void Evaluate(float time)
    {
        foreach(var boneInfo in _runtimeBoneInfos)
        {
            boneInfo.trs.rotation   = boneInfo.rotCurve.Evaluate(time);
            boneInfo.trs.position   = boneInfo.posCurve.Evaluate(time);
            boneInfo.trs.scale      = boneInfo.scaleCurve.Evaluate(time);
        }
    }

    private void Init()
    {
        _runtimeBoneInfos = new RuntimeBoneInfo[_skinningData.boneNames.Length];
        for(int i = 0; i < _runtimeBoneInfos.Length; i++)
        {
            var curveData = _clipInfo.curveDatas[i];
            RuntimeBoneInfo info = _runtimeBoneInfos[i];

            info.trs        = new TRS();
            info.rotCurve   = new AnimationCurveQuat(curveData.rotationCurve);
            info.posCurve   = new AnimationCurveVec3(curveData.positionCurve);
            info.scaleCurve = new AnimationCurveVec3(curveData.scaleCurve);
        }
    }
}
