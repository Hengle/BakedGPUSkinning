using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class GPUSkinnedMeshRenderer
    {
        private GPURendererRes _res;
        private int[]       _boneIdxMap;
        private Vector4[]   _matrixPalette; // _boneIdxMap.Length * 3   

        public void Init(GPUAnimation sdAnim, GPURendererRes res)
        {
            _res = res;
            _matrixPalette = new Vector4[_boneIdxMap.Length * 3];

        }

        public void Update()
        {

        }
    }

}
