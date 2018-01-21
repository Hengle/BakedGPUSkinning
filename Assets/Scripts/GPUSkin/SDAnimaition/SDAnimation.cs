using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BakedAnimation))]
public class SDAnimation : MonoBehaviour
{
    private SkinningData        _skinningData;
    private SDAnimationState    _currState;
    private float               _currTime;

    private VirtualBoneTransform[]  _boneTransforms;
    private TRS                     _lastTRS;

    private void Start()
    {
        _skinningData = GetComponent<BakedAnimation>().skinningData;
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


    }

    private void BuildBoneHierarchy()
    {
        _boneTransforms = new VirtualBoneTransform[_skinningData.boneNames.Length];

    }
}
