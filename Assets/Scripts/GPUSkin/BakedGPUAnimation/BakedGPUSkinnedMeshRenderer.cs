using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUSkinning
{
    public class BakedGPUSkinnedMeshRenderer
    {
        struct RunningBakedAnimData
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

        private RunningBakedAnimData    _runningBakedAnimData;
        private MaterialPropertyBlock   _mbp;
        private int                     _AnimParamId;
        private int                     _pixelPerFrame;
        private GPURendererRes          _rendererRes;
        #endregion

        public void Init(BakedGPUAnimation animation, GPURendererRes res)
        {
            _bakedAnimation = animation;
            _rendererRes = res;
            _skinningData = animation.skinningData;
            _pixelPerFrame = _skinningData.boneInfos.Length * 3;

            GameObject go = animation.gameObject;
            Transform t = go.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            _meshRenderer = go.GetOrAddComponent<MeshRenderer>();
            _meshFilter = go.GetOrAddComponent<MeshFilter>();

            _meshFilter.sharedMesh = res.mesh;
            _rendererParamDirty = true;

            _runningBakedAnimData = new RunningBakedAnimData();

            _meshRenderer.additionalVertexStreams = _rendererRes.additionalMesh;
            _meshRenderer.sharedMaterial = _rendererRes.bakedGPUMaterial;

            _AnimParamId = Shader.PropertyToID("_AnimParam");

            _mbp = new MaterialPropertyBlock();
            _meshRenderer.SetPropertyBlock(_mbp);
        }

        public void UpdateFrameIndex(int frameIndex, int fadeOutFrameIndex, float fadeOutPercent)
        {
            _runningBakedAnimData.frameIdx = frameIndex;
            UpdateRendererParams();
            UpdateMaterial();
        }

        public void StartPlay(int clipIdx)
        {
            _runningBakedAnimData.clipIdx = clipIdx;
            _runningBakedAnimData.clipPixelOffset = _skinningData.clipInfos[clipIdx].clipPixelOffset;
            _runningBakedAnimData.clipInfo = _skinningData.clipInfos[clipIdx];
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
            int frameOffset = _runningBakedAnimData.clipPixelOffset + _pixelPerFrame * _runningBakedAnimData.frameIdx;
            _mbp.SetVector(_AnimParamId, new Vector4(frameOffset, 0, 0, 0));
            _meshRenderer.SetPropertyBlock(_mbp);
        }
        #endregion


    }
}
