using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    [System.Serializable]
    public struct PosRot
    {
        public Vector3      position;
        public Quaternion   rotation;
    }

    /// <summary>
    /// Bake的骨骼数据
    /// </summary>
    public class SkinningData : ScriptableObject
    {
        public byte         frameRate;
        public BoneInfo[]   boneInfos;
        /// <summary>
        /// 去除 Joint 后的骨骼数量
        /// </summary>
        [System.NonSerialized]
        public int          realBoneCount;
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
        public byte[]           bakedBoneDatas;

        [System.NonSerialized]
        public Texture2D        bakedTexture;

        /// <summary>
        /// bake 后的绑点的模型坐标系数据, layout: [clipIdx][frameIdx][JointIdx]
        /// </summary>
        //public PosRot[][][]     jointDatas;
        /// Unity 序列化不支持多维数组和 List 嵌套，临时处理一下
        public List<JointClipData>          bakedJointDatas;

        /// <summary>
        /// rootMotion 的模型坐标系内数据(如果有的话), layout: [clipIdx][frameIdx]
        /// </summary>
        public List<RootMotionClipData>     rootMotions;
    }

    [System.Serializable]
    public struct BoneInfo
    {
        public string       name;
        public int          parentIdx;
        public bool         isJoint;
        /// <summary>
        /// 纯粹是个绑点，不会影响 Mesh 的顶点，此类节点不会存储数据到 bakedBoneDatas，计算索引时会被跳过
        /// </summary>
        public bool         isPureJoint;
        /// <summary>
        /// 此绑点是否要暴露出去(否则可能只是中间节点)
        /// </summary>
        public bool         exposed;
        public Matrix4x4    bindPose; // pureJoint 时用来存储默认 TRS
        /// <summary>
        /// 忽略绑点后的只统计骨骼的索引(AdditionalMesh 里使用此字段作为骨骼索引)
        /// </summary>
        [System.NonSerialized]
        public int          realBoneIdx;
    }

    [System.Serializable]
    public class JointClipData
    {
        public string               clipName;
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
        public int          clipPixelOffset;
        public EventInfo[]  events;
        /// <summary>
        /// 运行时无法获取 AnimationCurve, 因此需要我们自己存一份，代价是文件体积和运行时内存占用均翻倍
        /// </summary>
        public ClipCurveData[] curveDatas;
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

}
