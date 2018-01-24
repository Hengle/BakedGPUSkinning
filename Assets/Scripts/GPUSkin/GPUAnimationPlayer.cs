using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public enum GPUAnimationType
    {
        GPUAnimation,
        BakedGPUAnimation,
    }

    /// <summary>
    /// 动画播放器入口，会自动协调 BakedGPUAnimation 和 GPUAnimation
    /// </summary>
    [RequireComponent(typeof(BakedGPUAnimation), typeof(GPUAnimation))]
    public class GPUAnimationPlayer : MonoBehaviour
    {
        public SkinningData             skinningData;

        private IGPUAnimation           _currAnimation;
        private IGPUAnimation           _bakedGPUAnimation;
        private IGPUAnimation           _GPUAnimation;

        private Transform               _rootMotionNode;
        private Transform[]             _jointTrans;

        void Awake()
        {
            _bakedGPUAnimation = GetComponent<BakedGPUAnimation>();
            _GPUAnimation = GetComponent<GPUAnimation>();
            _GPUAnimation.enabled = false;
            _currAnimation = _bakedGPUAnimation;
        }

        void Start()
        {
            ProcessNode();
        }

        void Update()
        {

        }

        #region private
        /// <summary>
        /// 将除 mesh 节点，挂点(将其提到最顶级)，rootMotion 之外的所有节点移除
        /// </summary>
        private void ProcessNode()
        {
            _rootMotionNode = transform.Find(Consts.ROOT_MOTION_NAME);

            List<Transform> allChildren = new List<Transform>();
            GPUAnimUtils.GetAllChildren(transform, allChildren);

            List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
            foreach (var node in allChildren)
            {
                if (node == transform) continue;

                SkinnedMeshRenderer smr = node.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    /*
                        调试代码，发布时把 if 去掉
                        (挂载的这个武器并不能使用模型的 Animation, 并且其 Bone 数量也为0，因此应该是用 MeshRenderer 而不是 SkinnedMeshRenderer)
                     */
                    if (node.name != "right_weapon")
                    {
                        // 某些 smr 不位于模型直接子节点，将其提升以避免被移除掉(显示不会受到影响)
                        if (node.parent != transform)
                            node.parent = transform;
                        smrs.Add(smr);
                        continue;
                    }
                }

                if (node == _rootMotionNode)
                    continue;

                bool isJoint = false;
                foreach (var name in skinningData.jointNames)
                {
                    if (name == node.name)
                    {
                        node.parent = transform;
                        isJoint = true;
                        break;
                    }
                }

                if (!isJoint)
                {
                    // 不要使用 DestroyImmediate, 否则循环中的其它元素可能无法访问并且下面的 bsmr.Init 无法获取到 bones
                    Destroy(node.gameObject);
                }
            }

            // 原始的 SkinnedMeshRenderer 都创建对应的两种 Renderer
            foreach (var smr in smrs)
            {
                GPURendererRes res = GPUSkinRuntimeResMgr.Instance.GetOrCreateRes(smr, skinningData);
                _bakedGPUAnimation.AddMeshRenderer(res);
                _GPUAnimation.AddMeshRenderer(res);
                DestroyImmediate(smr);
            }

            Animation oriAnimation = gameObject.GetComponent<Animation>();
            if (oriAnimation != null)
                DestroyImmediate(oriAnimation);

            // 初始化绑点
            _jointTrans = new Transform[skinningData.jointNames.Length];
            for (int i = 0; i < _jointTrans.Length; i++)
            {
                Transform t = transform.Find(skinningData.jointNames[i]);
                if (t == null)
                {
                    Debug.LogErrorFormat("can not find join {0}", skinningData.jointNames[i]);
                    return;
                }

                _jointTrans[i] = t;
            }

            _bakedGPUAnimation.SetJointTransforms(_jointTrans);
            _GPUAnimation.SetJointTransforms(_jointTrans);
        }

        #endregion
    }

}
