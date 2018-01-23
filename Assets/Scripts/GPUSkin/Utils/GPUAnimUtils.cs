using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public static class GPUAnimUtils
    {
        public static void GetAllChildren(Transform node, List<Transform> result)
        {
            result.Add(node);
            int childCount = node.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = node.GetChild(i);
                GetAllChildren(child, result);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();

            return component;
        }

        public static bool IsQuaternionEqual(Quaternion lhs, Quaternion rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x)
                && Mathf.Approximately(lhs.y, rhs.y)
                && Mathf.Approximately(lhs.z, rhs.z)
                && Mathf.Approximately(lhs.w, rhs.w);
        }

        public static bool IsVector3Equal(Vector3 lhs, Vector3 rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x)
                && Mathf.Approximately(lhs.y, rhs.y)
                && Mathf.Approximately(lhs.z, rhs.z);
        }

        /// <summary>
        /// 计算 smr 中 bone 的索引顺序与 Bake 后的 SkinningData 中记录的 bone 的索引之间的映射关系，
        /// 因为 smr 中并不包含所有的 bone，并且顺序也不一定与 SkinningData 一致
        /// 返回值的结构：index: original bone index, value: baked bone index
        /// </summary>
        /// <param name="smr"></param>
        /// <param name="skinningData"></param>
        /// <returns></returns>
        public static int[] CalcBoneIdxMap(SkinnedMeshRenderer smr, SkinningData skinningData)
        {
            Transform[] bones = smr.bones;
            int boneCount = bones.Length;
            int skinnedBoneCount = skinningData.boneInfos.Length;

            int[] boneIdxMap = new int[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
#if UNITY_EDITOR
                bool found = false;
#endif
                string boneName = bones[i].name;
                for (int j = 0; j < skinnedBoneCount; j++)
                {
                    if (skinningData.boneInfos[j].name == boneName)
                    {
                        boneIdxMap[i] = j;
#if UNITY_EDITOR
                        found = true;
#endif
                        break;
                    }
                }

#if UNITY_EDITOR
                if (found == false)
                {
                    Debug.LogErrorFormat("can not find bone {0}", boneName);
                }
#endif
            }

            return boneIdxMap;
        }
    }

}
