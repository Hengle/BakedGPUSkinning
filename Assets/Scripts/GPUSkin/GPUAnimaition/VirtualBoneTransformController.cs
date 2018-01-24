using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class VirtualBoneTransformController
    {
        private SkinningData                _skinningData;
        private VirtualBoneTransform[]      _boneTransforms;
        /// <summary>
        /// 骨骼数据表，由于索引是基于 SkinningData 生成的，因此当前模型下所有 MeshRenderer 都可以使用同一个数据表
        /// </summary>
        private Vector4[]                   _matrixPalette; // _boneIdxMap.Length * 3

        public VirtualBoneTransformController(SkinningData skinningData)
        {
            _skinningData = skinningData;
            _boneTransforms = VirtualBoneTransformCache.Instance.GetOrCreateVBTs(skinningData);
        }

        public void Update(GPUAnimationState fadeOsutState, GPUAnimationState fadeInState, float percent)
        {
            //IRuntimeBoneInfo[] stateValues = _currState.runtimeBoneInfos;
            //Debug.Assert(stateValues.Length == _boneTransforms.Length);
            //for (int i = 0; i < _boneTransforms.Length; i++)
            //{
            //    TRS trs = stateValues[i].trs;
            //    _boneTransforms[i].localToParentMatrix = Matrix4x4.TRS(trs.position * _currWeight, trs.rotation.MultyScalar(_currWeight), trs.scale * _currWeight);
            //}

            //VirtualBoneTransform rootNode = _boneTransforms[0];

            //System.Action<VirtualBoneTransform> updateRecursive = null;
            //updateRecursive = node =>
            //{
            //    node.Update();
            //    foreach (var child in node.children)
            //        updateRecursive(child);

            //    return;
            //};

            //updateRecursive(rootNode);
        }

        
    }

}
