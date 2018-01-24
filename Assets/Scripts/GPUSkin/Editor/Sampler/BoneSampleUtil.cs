using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GPUSkinning.Editor
{
    public static class BoneSampleUtil
    {
        /// <summary>
        /// 按 Animation 组件内显示的顺序获取 AnimationClip
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        public static AnimationClip[] GetClips(Animation animation)
        {
            AnimationClip[] tmpClips = new AnimationClip[animation.GetClipCount()]; // 可能有空的，因此这个可能不是最终数量
            int idx = 0;
            foreach (AnimationState state in animation)
            {
                tmpClips[idx++] = animation.GetClip(state.name);
            }

            AnimationClip[] clips = new AnimationClip[idx];
            System.Array.Copy(tmpClips, clips, idx);
            return clips;
        }

        /// <summary>
        /// 获取节点及其所有子节点的骨骼数据并设置其parent关系(深度优先), 同时记录绑点信息
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="boneRoot"></param>
        /// <param name="basePath"></param>
        /// <param name="result"></param>
        public static void GetBoneSampleDataRecursive(GameObject obj, Transform boneRoot, string basePath, Transform[] jointNodes, List<BoneSampleData> result)
        {
            List<Transform> smrBones = GetAllSmrBones(obj);
            GetBoneSampleDataRecursive(boneRoot, smrBones, basePath, result, 0);
            foreach(var joint in jointNodes)
            {
                bool found = false;
                foreach(var bone in result)
                {
                    if(bone.transform == joint)
                    {
                        bone.isJoint = true;
                        bone.exposed = true;
                        found = true;
                        break;
                    }
                }

                if (!found) // 纯绑点，不属于 bone
                {
                    List<Transform> parents = new List<Transform>();
                    Transform currNode = joint;
                    while (currNode != obj.transform)
                    {
                        parents.Insert(0, currNode);
                        currNode = currNode.parent;
                    }

                    string path = string.Empty;
                    for(int i = 0; i < parents.Count; i++)
                    {
                        Transform jointNode = parents[i];
                        path += jointNode.name + "/";
                        var boneFinded = from boneNode in result where boneNode.transform == jointNode select boneNode.transform;
                        if (boneFinded != null) // 这个路径节点是骨骼
                            continue;

                        BoneSampleData data = new BoneSampleData();
                        data.transform = jointNode;
                        data.parent = i == 0 ? null : parents[i - 1];
                        data.path = jointNode.name;
                        data.boneIdx = -1;
                        data.isJoint = true;
                        data.isPureJoint = true;
                        data.exposed = i == parents.Count - 1;
                        data.bindPose = Matrix4x4.TRS(jointNode.localPosition, jointNode.localRotation, jointNode.localScale);
                    }
                }
            }
        }

        #region private
        private static List<Transform> GetAllSmrBones(GameObject obj)
        {
            SkinnedMeshRenderer[] smrs = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

            List<Transform> result = new List<Transform>();
            foreach (var smr in smrs)
            {
                result.AddRange(smr.bones);
            }

            return result;
        }


        /// <summary>
        /// 某节点是否是骨骼(当前节点以及递归子节点中有一个是骨骼则返回true)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="smrBones"></param>
        /// <returns></returns>
        private static bool IsNodeIsBone(Transform node, List<Transform> smrBones)
        {
            if (smrBones.Where(bone => bone.name == node.name).Count() > 0)
                return true;

            int childCount = node.childCount;
            if (childCount == 0)
                return false;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = node.GetChild(i);
                if (IsNodeIsBone(child, smrBones))
                    return true;
            }

            return false;
        }

        private static BoneSampleData GetBoneSampleDataRecursive(Transform node, List<Transform> smrBones, string basePath, List<BoneSampleData> result, short boneIdx)
        {
            // 如果此节点及其所有子节点都不在 SkinnedMeshRenderer 内则忽略(此类节点可能是挂点，由 Joint 控制)
            if (!IsNodeIsBone(node, smrBones))
            {
                return null;
            }

            BoneSampleData boneInfo = new BoneSampleData();
            boneInfo.transform = node;
            boneInfo.path = (basePath == string.Empty) ? node.name : string.Format("{0}/{1}", basePath, node.name);
            boneInfo.boneIdx = boneIdx;

            result.Add(boneInfo);

            int childrenCount = node.childCount;
            for (int i = 0; i < childrenCount; i++)
            {
                Transform child = node.GetChild(i);
                BoneSampleData childBone = GetBoneSampleDataRecursive(child, smrBones, boneInfo.path, result, boneIdx++);
                if (childBone != null)
                    childBone.parent = node;
            }

            return boneInfo;
        }

    }
    #endregion
}
