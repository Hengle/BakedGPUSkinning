using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDGame;

namespace GPUSkinning
{
    public class GPUSkinRuntimeResMgr : Singleton<GPUSkinRuntimeResMgr>
    {
        private Dictionary<int, GPURendererRes> _dic = new Dictionary<int, GPURendererRes>();

        /// <summary>
        /// 通过 id  获取 GPURendererRes
        /// </summary>
        /// <param name="id">SkinnedMeshRenderer.sharedMesh.GetInstanceID()</param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取或者创建 Res
        /// </summary>
        /// <param name="smr"></param>
        /// <param name="skinningData"></param>
        /// <returns></returns>
        public GPURendererRes GetOrCreateRes(SkinnedMeshRenderer smr, SkinningData skinningData)
        {
            int id = smr.sharedMesh.GetInstanceID();
            GPURendererRes ret = null;
            _dic.TryGetValue(id, out ret);
            if (ret != null)
                return ret;

            ret = new GPURendererRes();
            ret.skinningData = skinningData;
            ret.mesh = smr.sharedMesh;
            CreateSkinMesh(smr, ret);
            CreateBakedTexture2D(ret);
            CreateMaterial(smr, ret);

            _dic.Add(id, ret);

            return ret;
        }

        #region private
        /// <summary>
        /// 创建 GPUSkin 所需的 Mesh, 强制每个顶点只有两根骨骼
        /// 
        /// 由于 Unity 没有开放 BLENDINDICES 和 BLENDWEIGHT 语义，我们又不想修改资源内的原始mesh，只能自己创建一个 mesh 来存储,
        /// 缺点就是每个顶点多出了 4 * 4 个字节的体积, 假设每个模型 4000 个顶点，共缓存了 30 套模型，那么将多出
        /// 16 * 4000 * 30 = 1920000 = 1.83MB, 可以接受
        /// </summary>
        /// <param name="smr"></param>
        /// <returns></returns>
        private void CreateSkinMesh(SkinnedMeshRenderer smr, GPURendererRes res)
        {
            int[] boneIdxMap = GPUAnimUtils.CalcBoneIdxMap(smr, res.skinningData);

            Mesh smrMesh = smr.sharedMesh;

            Mesh addMesh = new Mesh();
            BoneWeight[] oriBoneWeights = smrMesh.boneWeights;
            int weightCount = oriBoneWeights.Length;
            List<Vector4> blendIndices = new List<Vector4>(weightCount);
            List<Vector4> blendWeights = new List<Vector4>(weightCount);

            for (int i = 0; i < weightCount; i++)
            {
                BoneWeight weight = oriBoneWeights[i];
                Vector4 indices = new Vector4();
                indices.x = boneIdxMap[weight.boneIndex0]; // 骨骼索引重新映射下
                indices.y = boneIdxMap[weight.boneIndex1];
                indices.z = boneIdxMap[weight.boneIndex2];
                indices.w = boneIdxMap[weight.boneIndex3];
                blendIndices.Add(indices);

                Vector4 weights = new Vector4();
                weights.x = weight.weight0;
                weights.y = weight.weight1;
                weights.z = weight.weight2;
                weights.w = weight.weight3;
                blendWeights.Add(weights);

                //float sum = weight.weight0 + weight.weight1;
                //blendWeights[i].x = weight.weight0 / sum;
                //blendWeights[i].y = weight.weight1 /sum;
            }

            addMesh.vertices = smrMesh.vertices; // 由于 Unity 有判断要求其它 channel 长度必须与 vertices 相等，这个内存只能浪费掉了
            addMesh.SetUVs(2, blendIndices);
            addMesh.SetUVs(3, blendWeights);
            //addMesh.uv3      = blendIndices;
            //addMesh.uv4      = blendWeights;
            addMesh.UploadMeshData(true); // warning!, DeviceLost 时可能无法恢复数据

            res.additionalMesh = addMesh;
        }

        private void CreateMaterial(SkinnedMeshRenderer smr, GPURendererRes res)
        {
            Texture2D animTex = res.bakedAnimTex;

            Material srcMat = smr.sharedMaterial;

            Material bakedGPUMaterial = new Material(Shader.Find("GPUSkinning/BakedGPUSkinning"));
            bakedGPUMaterial.SetTexture("_MainTex", srcMat.mainTexture);
            bakedGPUMaterial.SetTexture("_BakedAnimTex", animTex);
            bakedGPUMaterial.SetVector("_BakedAnimTexWH", new Vector4(res.skinningData.width, res.skinningData.height, 0, 0));
            bakedGPUMaterial.enableInstancing = true;

            Material GPUMaterial = new Material(Shader.Find("GPUSkinning/GPUSkinning"));
            GPUMaterial.SetTexture("_MainTex", srcMat.mainTexture);
            GPUMaterial.enableInstancing = true;

            res.bakedGPUMaterial = bakedGPUMaterial;
            res.GPUMaterial = GPUMaterial;
        }

        private void CreateBakedTexture2D(GPURendererRes res)
        {
            Texture2D tex = new Texture2D(res.skinningData.width, res.skinningData.height, TextureFormat.RGBAHalf, false, true);
            tex.name = string.Format("BakedAnimTexture_{0}", res.skinningData.name);
            tex.filterMode = FilterMode.Point;
            tex.LoadRawTextureData(res.skinningData.boneDatas);
            tex.Apply(false, true);
            tex.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

            res.bakedAnimTex = tex;
        }
        #endregion

    }

}
