using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDGame;

namespace GPUSkinning
{
    public class GPUSkinRuntimeData
    {
        /// <summary>
        /// 原始 Prefab Load 后的 SharedMesh.GetInstanceID()
        /// </summary>
        public int      id;
        /// <summary>
        /// 运行时生成的用来提供 blendIndex, blendWeight 通道的 Mesh
        /// </summary>
        public Mesh     additionalMesh;
        /// <summary>
        /// 与此 Mesh 关联的 Material
        /// </summary>
        public Material material;
        /// <summary>
        /// 与上个字段相同，但是开启了 CROSS_FADING
        /// </summary>
        public Material crossFadeMaterial;
    }

    public class GPUSkinRuntimeDataCache : Singleton<GPUSkinRuntimeDataCache>
    {
        private Dictionary<int, GPUSkinRuntimeData> _dic = new Dictionary<int, GPUSkinRuntimeData>();
        public GPUSkinRuntimeData GetDataByID(int id)
        {
            GPUSkinRuntimeData data = null;
            _dic.TryGetValue(id, out data);
            return data;
        }

        public void Add(int id, GPUSkinRuntimeData data)
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
