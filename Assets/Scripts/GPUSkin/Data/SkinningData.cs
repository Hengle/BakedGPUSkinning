using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bake的骨骼数据
/// </summary>
public class SkinningData : ScriptableObject
{
    public byte             frameRate;
    public BoneInfo[]       boneInfos;
    /// <summary>
    /// 某些挂点受 AnimationClip 控制，但不在 SkinnedMeshRenderer.bones 内, 所以不能用 Bone 索引来记录
    /// </summary>
    public string[]         jointNames;
    /// <summary>
    /// Clip 基础数据以及 Event 和 Curve 数据(用于CrossFade 等不适合运行 Bake 动画的场合)
    /// </summary>
    public BakedClipInfo[]  clipInfos;
    public short            width;
    public short            height;

    /// <summary>
    /// bake 后的骨骼帧变换矩阵数据(LocalToWorld * bindPose), 用于 VertexShader, layout: [clipIdx][frameIdx][boneIdx]
    /// </summary>
    [HideInInspector]
    public byte[]           boneDatas;

    /// <summary>
    /// bake 后的绑点的模型坐标系数据, layout: [clipIdx][frameIdx][JointIdx]
    /// </summary>
    //public PosRot[][][]     jointDatas;
    /// Unity 序列化不支持多维数组和 List 嵌套，临时处理一下
    public List<JointClipData> jointDatas;

    /// <summary>
    /// rootMotion 的模型坐标系内数据(如果有的话), layout: [clipIdx][frameIdx]
    /// </summary>
    public List<RootMotionClipData>       rootMotions;
}

[System.Serializable]
public struct BoneInfo
{
    public string       name;
    public int          parentIdx;
    public Matrix4x4    bindPose;
}

[System.Serializable]
public class JointClipData
{
    public string clipName;
    public List<JointFrameData> frameData = new List<JointFrameData>();
}

[System.Serializable]
public class JointFrameData
{
    public List<PosRot> jointData = new List<PosRot>();
}

[System.Serializable]
public class RootMotionClipData
{
    public List<PosRot> data = new List<PosRot>();
}

[System.Serializable]
public class BakedClipInfo
{
    public string       name;
    public short        frameCount;
    public byte         frameRate;
    public float        duration;
    public WrapMode     wrapMode;
    public Bounds       localBounds;
    /// <summary>
    /// boneDatas 字段内的起始偏移
    /// </summary>
    public int                  clipPixelOffset;
    public EventInfo[]          events;
    /// <summary>
    /// 运行时无法获取 AnimationCurve, 因此需要我们自己存一份，代价是文件体积和运行时内存占用均翻倍
    /// </summary>
    public SDClipCurveData[]    curveDatas;
}

[System.Serializable]
public struct EventInfo
{
    public float    time;
    public string   functionName;
    public string   stringParameter;
    public float    floatParameter;
    public int      intParameter;
    //public Object objectReferenceParameter;

#if UNITY_EDITOR
    public EventInfo(AnimationEvent aniEvent)
    {
        time            = aniEvent.time;
        functionName    = aniEvent.functionName;
        stringParameter = aniEvent.stringParameter;
        floatParameter  = aniEvent.floatParameter;
        intParameter    = aniEvent.intParameter;
    }
#endif
}
