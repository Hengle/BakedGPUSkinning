using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BakedSkinningMeshRenderer
#if UNITY_EDITOR
    : MonoBehaviour
#endif
{
    struct RunningAnimData
    {
        public int              frameIdx;
        public int              clipIdx;
        public int              clipPixelOffset;
        public BakedClipInfo    clipInfo;
    }

    #region public
    public Bounds bounds { get { return GetBounds(); } }
    public Bounds localBounds { get; set; }
    public bool isVisible { get; private set; }
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
    private BakedAnimation          _bakedAnimation;
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
    private RunningAnimData         _fadingOutAnimData;
    private bool                    _isCrossFading;
    private float                   _fadingOutPercent;
    private MaterialPropertyBlock   _mbp;
    private int                     _AnimParamId;
    private int                     _pixelPerFrame;
    private GPUSkinRuntimeData      _runtimeData;
    #endregion

    public void Init(BakedAnimation animation, SkinnedMeshRenderer smr)
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

        _currAnimData       = new RunningAnimData();
        _fadingOutAnimData  = new RunningAnimData();

        // 同一个原始 mesh 可以使用相同的 AdditionalMesh 和 Material
        _runtimeData = GPUSkinRuntimeDataCache.Instance.GetDataByID(smr.sharedMesh.GetInstanceID());
        if(_runtimeData == null)
        {
            _runtimeData = new GPUSkinRuntimeData();
            _runtimeData.additionalMesh = CreateSkinMesh(smr);
            var mats = CreateMaterialBySmr(smr);
            _runtimeData.material = mats[0];
            _runtimeData.crossFadeMaterial = mats[1];
        }

        _meshRenderer.additionalVertexStreams = _runtimeData.additionalMesh;
        _meshRenderer.sharedMaterial = _runtimeData.material;

        _AnimParamId = Shader.PropertyToID("_AnimParam");

        _mbp = new MaterialPropertyBlock();
        _meshRenderer.SetPropertyBlock(_mbp);
    }

    public void UpdateFrameIndex(int frameIndex, int fadeOutFrameIndex, float fadeOutPercent)
    {
        _currAnimData.frameIdx = frameIndex;
        _fadingOutAnimData.frameIdx = fadeOutFrameIndex;
        _fadingOutPercent = fadeOutPercent;
        UpdateRendererParams();
        UpdateMaterial();
    }

    public void StartPlay(int clipIdx)
    {
        _currAnimData.clipIdx = clipIdx;
        _currAnimData.clipPixelOffset = _skinningData.clipInfos[clipIdx].clipPixelOffset;
        _currAnimData.clipInfo = _skinningData.clipInfos[clipIdx];
    }

    public void BeginCrossFade()
    {
        _isCrossFading = true;
        _meshRenderer.sharedMaterial = _runtimeData.crossFadeMaterial;
        _fadingOutAnimData = _currAnimData;
    }

    public void EndCrossFade()
    {
        _isCrossFading = false;
        _meshRenderer.sharedMaterial = _runtimeData.material;
    }


    private Material[] CreateMaterialBySmr(SkinnedMeshRenderer smr)
    {
        Texture2D animTex = _bakedAnimation.animTexture;

        Material srcMat = smr.sharedMaterial;

        Material newMat = new Material(Shader.Find("GPUSkinning/BakedGPUSkinning"));
        newMat.SetTexture("_MainTex", srcMat.mainTexture);
        newMat.SetTexture("_BakedAnimTex", animTex);
        newMat.SetVector("_BakedAnimTexWH", new Vector4(_skinningData.width, _skinningData.height, 0, 0));
        newMat.enableInstancing = true;

        Material crossFadeMat = new Material(newMat);
        crossFadeMat.SetTexture("_MainTex", srcMat.mainTexture);
        crossFadeMat.SetTexture("_BakedAnimTex", animTex);
        crossFadeMat.SetVector("_BakedAnimTexWH", new Vector4(_skinningData.width, _skinningData.height, 0, 0));
        crossFadeMat.EnableKeyword("CROSS_FADING");
        crossFadeMat.enableInstancing = true;
        return new Material[] { newMat, crossFadeMat};
    }

    

    /// <summary>
    /// 计算骨骼索引映射表(smr.bones 并不使用所有骨骼，索引顺序也与全局骨骼索引不一致)
    /// </summary>
    /// <param name="smr"></param>
    private int[] CalcBoneIdxMap(SkinnedMeshRenderer smr)
    {
        Transform[] bones = smr.bones;
        int boneCount = bones.Length;
        int skinnedBoneCount = _skinningData.boneInfos.Length;

        int[] boneIdxMap = new int[boneCount];

        for(int i = 0; i < boneCount; i++)
        {
#if UNITY_EDITOR
            bool found = false;
#endif
            string boneName = bones[i].name;
            for(int j = 0; j < skinnedBoneCount; j++)
            {
                if(_skinningData.boneInfos[j].name == boneName)
                {
                    boneIdxMap[i] = j;
#if UNITY_EDITOR
                    found = true;
#endif
                    break;
                }
            }

#if UNITY_EDITOR
            if(found == false)
            {
                Debug.LogErrorFormat("can not find bone {0}", boneName);
            }
#endif
        }

        return boneIdxMap;
    }

    /// <summary>
    /// 创建 GPUSkin 所需的 Mesh, 强制每个顶点只有两根骨骼
    /// 
    /// 由于 Unity 没有开放 BLENDINDICES 和 BLENDWEIGHT 语义，我们又不想修改资源内的原始mesh，只能自己创建一个 mesh 来存储,
    /// 缺点就是每个顶点多出了 4 * 4 个字节的体积, 假设每个模型 4000 个顶点，共缓存了 30 套模型，那么将多出
    /// 16 * 4000 * 30 = 1920000 = 1.83MB, 可以接受
    /// </summary>
    /// <param name="smr"></param>
    private Mesh CreateSkinMesh(SkinnedMeshRenderer smr)
    {
        Mesh smrMesh = smr.sharedMesh;
        int[] boneIdxMap = CalcBoneIdxMap(smr);

        Mesh addMesh = new Mesh();
        BoneWeight[] oriBoneWeights = smrMesh.boneWeights;
        int weightCount = oriBoneWeights.Length;
        List<Vector4> blendIndices = new List<Vector4>(weightCount);
        List<Vector4> blendWeights = new List<Vector4>(weightCount);

        for(int i = 0; i < weightCount; i++)
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

        _meshRenderer.shadowCastingMode     = shadowCastingMode;
        _meshRenderer.receiveShadows        = receiveShadow;
        _meshRenderer.lightProbeUsage       = lightProbeUsage;
        _meshRenderer.reflectionProbeUsage  = reflectionProbeusage;

        _rendererParamDirty = false;
    }


    #region Material
    private static int _frameIdx = 0;
    private static int _cfFrameIdx = 0;
    private void UpdateMaterial()
    {
        int frameOffset = _currAnimData.clipPixelOffset + _pixelPerFrame * _currAnimData.frameIdx;
        int fadeoutFrameOffset = 0;
        if (_isCrossFading)
            fadeoutFrameOffset = _fadingOutAnimData.clipPixelOffset + _pixelPerFrame * _fadingOutAnimData.frameIdx;

        if(!_isCrossFading)// for test
            _mbp.SetVector(_AnimParamId, new Vector4(frameOffset, fadeoutFrameOffset, _fadingOutPercent, 0));
        else
            _mbp.SetVector(_AnimParamId, new Vector4(frameOffset, fadeoutFrameOffset, _fadingOutPercent, 0));
        _meshRenderer.SetPropertyBlock(_mbp);

        if (_isCrossFading)
        {
            if (_currAnimData.frameIdx != _frameIdx)
            {
                Debug.LogFormat("frameIdx Changed from {0} to {1} at {2}", _frameIdx, _currAnimData.frameIdx, Time.time);
                _frameIdx = _currAnimData.frameIdx;
            }

            if (_fadingOutAnimData.frameIdx != _cfFrameIdx)
            {
                Debug.LogFormat("CFFrameIdx Changed from {0} to {1} at {2}", _cfFrameIdx, _fadingOutAnimData.frameIdx, Time.time);
                _cfFrameIdx = _fadingOutAnimData.frameIdx;
            }
        }
        else
        {
            //if (_currAnimData.frameIdx >= 54)
            //{
            //    Debug.LogFormat("Normal Frame <color=#55cd60>{0}</color>", _currAnimData.frameIdx);
            //    UnityEditor.EditorApplication.isPaused = true;
            //}
        }

    }
    #endregion


}