using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class GPURendererRes
    {
        /// <summary>
        /// 原始 Prefab Load 后的 SharedMesh.GetInstanceID()
        /// </summary>
        public int      id;
        /// <summary>
        /// 加载的原始 Mesh
        /// </summary>
        public Mesh     mesh;
        /// <summary>
        /// 运行时生成的用来提供 blendIndex, blendWeight 通道的额外 Mesh
        /// </summary>
        public Mesh     additionalMesh;
        /// <summary>
        /// 运行 Baked Animation 所需的 Material
        /// </summary>
        public Material bakedGPUMaterial;
        /// <summary>
        /// 正常的 GPUSkinning 的 Material(主要用于 CrossFade)
        /// </summary>
        public Material GPUMaterial;
    }
}
