using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// 骨骼采样数据(SkinnedMeshRender.bones 引用到的)
/// </summary>
public class BoneSampleData
{
    public Transform        transform;
    public Transform        parent;
    public string           path;
    public int              boneIdx;
    public Matrix4x4        bindPose;       // bone to model or model to bone?
    public Matrix4x4[][]    matrixes;       // 每一帧的变换矩阵, [clipIdx][frameIdx], (transform.localToWorldMatrix * bindPose)

    public BoneSampleData()
    {
        bindPose = Matrix4x4.identity;
    }
}

/// <summary>
/// 挂点采样数据(可能 SkinnedMeshRender.bones 没有使用但是会被 AnimationClip 控制并且需要挂载物品的节点)
/// </summary>
public class JointSampleData
{
    public Transform                transform;
    public int                      jointIdx;
    /// <summary>
    /// 每一帧的位置模型坐标系内的位置旋转数据, [clipIdx][frameIdx]
    /// </summary>
    public PosRot[][]            matrixes;
}

[System.Serializable]
public struct PosRot
{
    public Vector3      position;
    public Quaternion   rotation;
}

/// <summary>
/// RootMotion 数据
/// </summary>
public class RootMotionData
{
    public Transform    transform;
    /// <summary>
    /// 每一帧的变换矩阵, [clipIdx][frameIdx], (transform.localToWorldMatrix)
    /// </summary>
    public PosRot[][]   matrixes;
}

public class SampleParam
{
    public short            clipIdx;
    public short            currFrameIdx;
    public short            frameCount;
    public float            frameRate;
    public AnimationState   aniState;
    public AnimationClip    clip;
}
#endif
