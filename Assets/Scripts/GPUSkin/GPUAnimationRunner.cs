using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自己实现的动画的入口，会自动协调 BakedAnimation 和 SDAnimation
/// </summary>
[RequireComponent(typeof(BakedGPUAnimation), typeof(SDAnimation))]
public class GPUAnimRunner : MonoBehaviour
{
    private BakedGPUAnimation _bakedAnimation;
    private SDAnimation _sdAnimation;

    private void Awake()
    {
        _bakedAnimation = GetComponent<BakedGPUAnimation>();
        _sdAnimation = GetComponent<SDAnimation>();
        _sdAnimation.enabled = false;
    }

    void Start ()
    {
        
	}
	
	void Update () {
		
	}
}
