using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class GPUAnimation : MonoBehaviour, IGPUAnimation
    {
        private SkinningData                    _skinningData;
        private GPUAnimationState               _fadeOutState;
        private GPUAnimationState               _fadeInState;

        private float                           _fadingLength;
        private float                           _fadingTime;

        VirtualBoneTransformController          _vbtController;
        
        private TRS                             _lastTRS;
        private Transform[]                     _jointTrans;

        private List<GPUSkinnedMeshRenderer>    _GPUMeshRenderers = new List<GPUSkinnedMeshRenderer>();

        private void Start()
        {
            _skinningData = GetComponent<GPUAnimationPlayer>().skinningData;
            _vbtController = new VirtualBoneTransformController(_skinningData);
        }

        public void Play(int idx)
        {
            _fadeOutState = null;
            _fadingLength = Mathf.Infinity;
            _fadingTime = 0f;
            _fadeInState = new GPUAnimationState(_skinningData, idx);
            enabled = true;
        }

        public void CrossFade(int idx, float length)
        {
            _fadeOutState = _fadeInState;
            _fadeInState = new GPUAnimationState(_skinningData, idx);
            _fadingLength = length;
            _fadingTime = 0f;
            enabled = true;
        }

        public void Stop()
        {
            enabled = false;
            _fadeInState = null;
            _fadeOutState = null;
            _fadingLength = Mathf.Infinity;
            _fadingTime = 0f;
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            _fadeOutState.Evaluate(delta);
            _vbtController.Update(_fadeOutState, _fadeInState, _fadingTime / _fadingLength);

            foreach (var renderer in _GPUMeshRenderers)
            {
                renderer.Update();
            }
        }

        public void AddMeshRenderer(GPURendererRes res)
        {
            GPUSkinnedMeshRenderer renderer = new GPUSkinnedMeshRenderer();
            renderer.Init(this, res);
            _GPUMeshRenderers.Add(renderer);
        }

        public void SetJointTransforms(Transform[] trans)
        {
            _jointTrans = trans;
        }

        
    }

}
