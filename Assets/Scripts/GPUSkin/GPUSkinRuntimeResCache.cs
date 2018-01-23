using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDGame;

namespace GPUSkinning
{
    public class GPUSkinRuntimeResCache : Singleton<GPUSkinRuntimeResCache>
    {
        private Dictionary<int, GPURendererRes> _dic = new Dictionary<int, GPURendererRes>();
        public GPURendererRes GetDataByID(int id)
        {
            GPURendererRes data = null;
            _dic.TryGetValue(id, out data);
            return data;
        }

        public void Add(int id, GPURendererRes data)
        {
            if (_dic.ContainsKey(id))
            {
                Debug.LogErrorFormat("AdditionalMeshCache already contains key {0}", id);
                return;
            }
            _dic.Add(id, data);
        }

        public void Remove(int id)
        {
            _dic.Remove(id);
        }

    }

}
