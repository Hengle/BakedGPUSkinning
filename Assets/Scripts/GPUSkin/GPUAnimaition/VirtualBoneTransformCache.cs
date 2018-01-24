using SDGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    /// <summary>
    /// 缓存虚拟骨骼树，同一个 SkinningData 使用的虚拟树是相同的
    /// </summary>
    public class VirtualBoneTransformCache : Singleton<VirtualBoneTransformCache>
    {
        private Dictionary<SkinningData, VirtualBoneTransform[]> _dic = new Dictionary<SkinningData, VirtualBoneTransform[]>();

        public VirtualBoneTransform[] GetOrCreateVBTs(SkinningData skinningData)
        {
            VirtualBoneTransform[] ret;
            if(_dic.TryGetValue(skinningData, out ret))
                return ret;

            ret = BuildBoneHierarchy(skinningData);
            return ret;
        }

        private VirtualBoneTransform[] BuildBoneHierarchy(SkinningData skinningData)
        {
            BoneInfo[] boneInfos = skinningData.boneInfos;
            VirtualBoneTransform[] ret = new VirtualBoneTransform[skinningData.boneInfos.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new VirtualBoneTransform();
                ret[i].name = boneInfos[i].name;
            }
            for (int i = 0; i < ret.Length; i++)
            {
                int parentIdx = boneInfos[i].parentIdx;
                if (parentIdx != -1)
                {
                    ret[i].parent = ret[parentIdx];
                    ret[parentIdx].children.Add(ret[i]);
                }
            }

            //Debug.Assert(_jointTrans != null);
            return ret;
        }
    }

}
