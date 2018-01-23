﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUSkinning
{
    public class BakedGPUSkinnedMeshRenderer
    {
        struct RunningAnimData
        {
            public int              frameIdx;
            public int              clipIdx;
            public int              clipPixelOffset;
            public BakedClipInfo    clipInfo;
        }

        #region public
        public Bounds   bounds { get { return GetBounds(); } }
        public Bounds   localBounds { get; set; }
        public bool     isVisible { get; private set; }
        /// <summary>
        /// The maximum number of bones affecting a single vertex.
        /// </summary>
        public SkinQuality quality { get; set; }

        public ShadowCastingMode shadowCastingMode { get { return _shadowCastingMode; } set { _shadowCastingMode = value; _rendererParamDirty = true; } }
        public bool receiveShadow { get { return _receiveShadow; } set { _receiveShadow = value; _rendererParamDirty = true; } }
        public bool updateWhenOffscreen { get; set; }
        public LightProbeUsage lightProbeUsage { get { return _lightProbeUsage; } set { _lightProbeUsage = value; _rendererParamDirty = true; } }
        public ReflectionProbeUsage reflectionProbeusage { get { return _reflectionProbeusage; } set { _reflectionProbeusage = value; _rendererParamDirty = true; } }
        #endregion

        #region private
        private BakedGPUAnimation       _bakedAnimation;
        private SkinningData            _skinningData;

        private MeshRenderer            _meshRenderer;
        private MeshFilter              _meshFilter;

        private bool                    _rendererParamDirty;
        private bool                    _receiveShadow;
        private bool                    _updateWhenOffscreen;
        private ShadowCastingMode       _shadowCastingMode;
        private LightProbeUsage         _lightProbeUsage;
        private ReflectionProbeUsage    _reflectionProbeusage;

        private RunningAnimData         _currAnimData;
        private MaterialPropertyBlock   _mbp;
        private int                     _AnimParamId;
        private int                     _pixelPerFrame;
        private GPURendererRes      _runtimeData;
        #endregion

        public void Init(BakedGPUAnimation animation, SkinnedMeshRenderer smr, int[] boneIdxMap)
        {
            _bakedAnimation = animation;
            _skinningData = animation.skinningData;
            _pixelPerFrame = _skinningData.boneInfos.Length * 3;

            GameObject go = smr.gameObject;
            Transform t = go.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            _meshRenderer = go.GetOrAddComponent<MeshRenderer>();
            _meshFilter = go.GetOrAddComponent<MeshFilter>();

            _meshFilter.sharedMesh = smr.sharedMesh;
            _rendererParamDirty = true;

            _currAnimData = new RunningAnimData();

            // 同一个原始 mesh 可以使用相同的 AdditionalMesh 和 Material
            _runtimeData = GPUSkinRuntimeResCache.Instance.GetDataByID(smr.sharedMesh.GetInstanceID());
            if (_runtimeData == null)
            {
                _runtimeData = new GPURendererRes();
                _runtimeData.additionalMesh = CreateSkinMesh(smr, boneIdxMap);
                _runtimeData.bakedGPUMaterial = CreateMaterialBySmr(smr); ;
            }

            _meshRenderer.additionalVertexStreams = _runtimeData.additionalMesh;
            _meshRenderer.sharedMaterial = _runtimeData.bakedGPUMaterial;

            _AnimParamId = Shader.PropertyToID("_AnimParam");

            _mbp = new MaterialPropertyBlock();
            _meshRenderer.SetPropertyBlock(_mbp);
        }

        public void UpdateFrameIndex(int frameIndex, int fadeOutFrameIndex, float fadeOutPercent)
        {
            _currAnimData.frameIdx = frameIndex;
            UpdateRendererParams();
            UpdateMaterial();
        }

        public void StartPlay(int clipIdx)
        {
            _currAnimData.clipIdx = clipIdx;
            _currAnimData.clipPixelOffset = _skinningData.clipInfos[clipIdx].clipPixelOffset;
            _currAnimData.clipInfo = _skinningData.clipInfos[clipIdx];
        }


        private Material CreateMaterialBySmr(SkinnedMeshRenderer smr)
        {
            Texture2D animTex = _bakedAnimation.animTexture;

            Material srcMat = smr.sharedMaterial;

            Material newMat = new Material(Shader.Find("SDAnim/BakedGPUSkinning"));
            newMat.SetTexture("_MainTex", srcMat.mainTexture);
            newMat.SetTexture("_BakedAnimTex", animTex);
            newMat.SetVector("_BakedAnimTexWH", new Vector4(_skinningData.width, _skinningData.height, 0, 0));
            newMat.enableInstancing = true;

            return newMat;
        }


        /// <summary>
        /// 创建 GPUSkin 所需的 Mesh, 强制每个顶点只有两根骨骼
        /// 
        /// 由于 Unity 没有开放 BLENDINDICES 和 BLENDWEIGHT 语义，我们又不想修改资源内的原始mesh，只能自己创建一个 mesh 来存储,
        /// 缺点就是每个顶点多出了 4 * 4 个字节的体积, 假设每个模型 4000 个顶点，共缓存了 30 套模型，那么将多出
        /// 16 * 4000 * 30 = 1920000 = 1.83MB, 可以接受
        /// </summary>
        /// <param name="smr"></param>
        private Mesh CreateSkinMesh(SkinnedMeshRenderer smr, int[] boneIdxMap)
        {
            Mesh smrMesh = smr.sharedMesh;

            Mesh addMesh = new Mesh();
            BoneWeight[] oriBoneWeights = smrMesh.boneWeights;
            int weightCount = oriBoneWeights.Length;
            List<Vector4> blendIndices = new List<Vector4>(weightCount);
            List<Vector4> blendWeights = new List<Vector4>(weightCount);

            for (int i = 0; i < weightCount; i++)
            {
                BoneWeight weight = oriBoneWeights[i];
                Vector4 indices = new Vector4();
                indices.x = boneIdxMap[weight.boneIndex0]; // 骨骼索引重新映射下
                indices.y = boneIdxMap[weight.boneIndex1];
                indices.z = boneIdxMap[weight.boneIndex2];
                indices.w = boneIdxMap[weight.boneIndex3];
                blendIndices.Add(indices);

                Vector4 weights = new Vector4();
                weights.x = weight.weight0;
                weights.y = weight.weight1;
                weights.z = weight.weight2;
                weights.w = weight.weight3;
                blendWeights.Add(weights);

                //float sum = weight.weight0 + weight.weight1;
                //blendWeights[i].x = weight.weight0 / sum;
                //blendWeights[i].y = weight.weight1 /sum;
            }

            addMesh.vertices = smrMesh.vertices; // 由于 Unity 有判断要求其它 channel 长度必须与 vertices 相等，这个内存只能浪费掉了
            addMesh.SetUVs(2, blendIndices);
            addMesh.SetUVs(3, blendWeights);
            //addMesh.uv3      = blendIndices;
            //addMesh.uv4      = blendWeights;
            addMesh.UploadMeshData(true); // warning!, DeviceLost 时可能无法恢复数据

            _meshRenderer.additionalVertexStreams = addMesh;
            return addMesh;
        }

        private Bounds GetBounds()
        {
            return new Bounds();
        }

        private void UpdateRendererParams()
        {
            if (!_rendererParamDirty)
                return;

            _meshRenderer.shadowCastingMode = shadowCastingMode;
            _meshRenderer.receiveShadows = receiveShadow;
            _meshRenderer.lightProbeUsage = lightProbeUsage;
            _meshRenderer.reflectionProbeUsage = reflectionProbeusage;

            _rendererParamDirty = false;
        }


        #region Material
        private static int _frameIdx = 0;
        private static int _cfFrameIdx = 0;
        private void UpdateMaterial()
        {
            int frameOffset = _currAnimData.clipPixelOffset + _pixelPerFrame * _currAnimData.frameIdx;
            _mbp.SetVector(_AnimParamId, new Vector4(frameOffset, 0, 0, 0));
            _meshRenderer.SetPropertyBlock(_mbp);
        }
        #endregion


    }
}
