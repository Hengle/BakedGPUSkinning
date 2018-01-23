using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public interface IGPUAnimation
    {
        bool enabled { get; set; }
        void AddMeshRenderer(GPURendererRes res);
        void SetJointTransforms(Transform[] trans);
    }

}
