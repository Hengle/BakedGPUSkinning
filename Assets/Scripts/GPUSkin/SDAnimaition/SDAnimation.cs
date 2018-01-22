using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDAnimation : MonoBehaviour
{
    private SkinningData        _skinningData;
    private SDAnimationState    _currState;
    private float               _currTime;
    private float               _currWeight;

    private VirtualBoneTransform[]          _boneTransforms;
    private TRS                             _lastTRS;

    private List<SDSkinnedMeshRenderer>     _sdAnimRenderers;

    private void Start()
    {
        _skinningData = GetComponent<BakedAnimation>().skinningData;
        _sdAnimRenderers = new List<SDSkinnedMeshRenderer>();
        BuildBoneHierarchy();
    }

    public void Play(int idx)
    {
        _currState = new SDAnimationState(_skinningData, idx);
        _currTime = 0f;
        enabled = true;
    }

    public void Stop()
    {
        _currTime = 0f;
        enabled = false;
    }

    private void Update()
    {
        _currTime += Time.deltaTime;
        _currState.Evaluate(_currTime);

        UpdateBoneTransforms();

        foreach (var renderer in _sdAnimRenderers)
        {
            renderer.Update();
        }
    }

    private void UpdateBoneTransforms()
    {
        IRuntimeBoneInfo[] stateValues = _currState.runtimeBoneInfos;
        Debug.Assert(stateValues.Length == _boneTransforms.Length);
        for (int i = 0; i < _boneTransforms.Length; i++)
        {
            TRS trs = stateValues[i].trs;
            _boneTransforms[i].localToParentMatrix = Matrix4x4.TRS(trs.position * _currWeight, trs.rotation.MultyScalar(_currWeight), trs.scale * _currWeight);
        }

        VirtualBoneTransform rootNode = _boneTransforms[0];

        System.Action<VirtualBoneTransform> updateRecursive = null;
        updateRecursive = node =>
        {
            node.Update();
            foreach (var child in node.children)
                updateRecursive(child);

            return;
        };

        updateRecursive(rootNode);
    }

    public void AddSDMeshRenderer(SkinnedMeshRenderer smr, int[] boneIdxMap)
    {
        SDSkinnedMeshRenderer renderer = new SDSkinnedMeshRenderer();
        renderer.Init(this, smr, boneIdxMap);
        _sdAnimRenderers.Add(renderer);
    }

    private void BuildBoneHierarchy()
    {
        BoneInfo[] boneInfos = _skinningData.boneInfos;
        _boneTransforms = new VirtualBoneTransform[_skinningData.boneInfos.Length];
        for(int i = 0; i < _boneTransforms.Length; i++)
        {
            _boneTransforms[i] = new VirtualBoneTransform();
            _boneTransforms[i].name = boneInfos[i].name;
        }
        for (int i = 0; i < _boneTransforms.Length; i++)
        {
            int parentIdx = boneInfos[i].parentIdx;
            if (parentIdx != -1)
            {
                _boneTransforms[i].parent = _boneTransforms[parentIdx];
                _boneTransforms[parentIdx].children.Add(_boneTransforms[i]);
            }
        }
    }
}
