using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    /// <summary>
    /// 自己实现的动画的入口，会自动协调 BakedAnimation 和 SDAnimation
    /// </summary>
    [RequireComponent(typeof(BakedGPUAnimation), typeof(GPUAnimation))]
    public class GPUAnimationRunner : MonoBehaviour
    {
        private BakedGPUAnimation   _bakedAnimation;
        private GPUAnimation        _sdAnimation;

        private void Awake()
        {
            _bakedAnimation = GetComponent<BakedGPUAnimation>();
            _sdAnimation = GetComponent<GPUAnimation>();
            _sdAnimation.enabled = false;
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }

}
