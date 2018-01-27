using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakedAnimation : MonoBehaviour
{
    public SkinningData skinningData;
    public bool isPlaying { get; private set; }
    public bool isPaused { get; private set; }
    public string playingClip { get { return _currClipInfo.clipIdx == -1 ? string.Empty : _currClipInfo.bakedClipInfo.name; } }
    public float speed = 1.0f;

    public PosRot PosRot { get; private set; }

    private Transform                           _rootMotionNode;
    private Transform[]                         _jointTrans;
    private List<BakedSkinningMeshRenderer>     _bakedRenderers;

    private ClipInfo                            _fadeOutClipInfo;
    private ClipInfo                            _currClipInfo;

    private CrossFadeInfo                       _crossFadeInfo;
    private float                               _deltaTime;


    struct ClipInfo
    {
        public int              clipIdx;
        public BakedClipInfo    bakedClipInfo;
        public float            time;
        public int              frameIdx;
        /// <summary>
        /// 上一次执行的事件的索引
        /// </summary>
        public int              lastExecutedEventIdx;
    }

    struct CrossFadeInfo
    {
        public bool isCrossFading;
        public float fadeLength;
        public float elapsed;

#if DEBUG
        public bool flag;
#endif

        public void Set(float fadeLength)
        {
            isCrossFading = true;
            this.fadeLength = fadeLength;
            elapsed = 0;
#if DEBUG
            flag = true;
#endif
        }

        public void Reset()
        {
            isCrossFading = false;
            fadeLength = 0;
            elapsed = 0;
#if DEBUG
            flag = false;
#endif
        }
    }

    void Awake()
    {
        _bakedRenderers = new List<BakedSkinningMeshRenderer>();
        _fadeOutClipInfo = new ClipInfo();
        _fadeOutClipInfo.clipIdx = -1;
        _currClipInfo = new ClipInfo();
        _currClipInfo.clipIdx = -1;

        _crossFadeInfo = new CrossFadeInfo();

        isPlaying = false;
        isPaused = false;
    }

    void Start()
    {
        ProcessNode();
    }

    public bool Play(string animation)
    {
        int clipIdx = FindClipIdx(animation);
        if(clipIdx == -1)
        {
            Debug.LogFormat("Invalid clip name[{0}]", animation);
            return false;
        }

        return PlayByIndex(clipIdx, true);
    }

    /// <summary>
    /// 播放指定索引的 clip(必须在 Start 之后调用，以后再修正此问题)
    /// </summary>
    /// <param name="index">clip 索引</param>
    /// <param name="uncheck">是否不检查有效性</param>
    /// <returns></returns>
    public bool PlayByIndex(int index, bool uncheck = false)
    {
        if (!uncheck && !IsValidClipIdx(index))
            return false;

        _currClipInfo.clipIdx               = index;
        _currClipInfo.bakedClipInfo         = skinningData.clipInfos[index];
        _currClipInfo.time                  = 0;
        _currClipInfo.lastExecutedEventIdx  = -1;

#if DEBUG
        _currClipInfo.bakedClipInfo.wrapMode = WrapMode.Loop;
        SharedData.skinningData = skinningData;
#endif

        foreach (var bsmr in _bakedRenderers)
        {
            bsmr.StartPlay(index);
        }

        isPaused = false;
        isPlaying = true;

        return true;
    }

    public void Play()
    {
        if(_currClipInfo.clipIdx != -1)
        {
            isPaused = false;
            isPlaying = true;
        }
    }

    public void CrossFade(string animation, float fadeLength)
    {
        _fadeOutClipInfo = _currClipInfo;
        _crossFadeInfo.Set(fadeLength);

        foreach (var bsmr in _bakedRenderers)
        {
            bsmr.BeginCrossFade();
        }

        Play(animation);
        Debug.Log("<color=yellow>CrossFade Begin</color>");
        Time.timeScale = 0.1f;
        //UnityEditor.EditorApplication.isPaused = true;
    }

    public void Stop()
    {
        _currClipInfo.time = 0;
        _currClipInfo.frameIdx = 0;
        _currClipInfo.lastExecutedEventIdx = -1;
        _crossFadeInfo.Reset();
        isPlaying = false;
        isPaused = false;
    }

    public void Pause()
    {
        isPlaying = false;
        isPaused = true;
    }


    void Update ()
    {
        if (isPlaying == false || isPaused)
            return;

        _deltaTime  = Time.deltaTime * speed;

        UpdateCurrAnimClip();
        UpdateCrossFadeAnimClip();

        UpdateBakedSmr();
        UpdateJoints();
        UpdateCulling();
        DoRootMotion();
        ExecuteEvents();
    }

#region private
    private void UpdateCurrAnimClip()
    {
        float time = _currClipInfo.time;
        time += _deltaTime;

#if DEBUG
        if (SharedData.isSync)
            time = SharedData.time;
#endif

        float duration = _currClipInfo.bakedClipInfo.duration;
        if (_currClipInfo.bakedClipInfo.wrapMode != WrapMode.Loop && _currClipInfo.time > duration)
        {
            Stop();
            return;
        }

        _currClipInfo.time = time % duration;
        int frameIdx = Mathf.RoundToInt(_currClipInfo.time * _currClipInfo.bakedClipInfo.frameRate);
        if (frameIdx == _currClipInfo.bakedClipInfo.frameCount)
            frameIdx = 0;

        if (frameIdx == _currClipInfo.frameIdx)
            return;
        _currClipInfo.frameIdx = frameIdx;

#if DEBUG
        if (SharedData.isSync)
            SharedData.frameIdx = frameIdx;
#endif
    }

    private void UpdateCrossFadeAnimClip()
    {
        if (!_crossFadeInfo.isCrossFading)
            return;

        float time = _fadeOutClipInfo.time;
        time += _deltaTime;
        _crossFadeInfo.elapsed += _deltaTime;

        float duration = _fadeOutClipInfo.bakedClipInfo.duration;
        if (_crossFadeInfo.elapsed >= _crossFadeInfo.fadeLength
            || _fadeOutClipInfo.time > duration)
        {
            StopCrossFade();
            return;
        }

        _fadeOutClipInfo.time = time % duration;
        int frameIdx = Mathf.RoundToInt(_fadeOutClipInfo.time * _fadeOutClipInfo.bakedClipInfo.frameRate);
        if (frameIdx == _fadeOutClipInfo.bakedClipInfo.frameCount)
            frameIdx = 0;

        if (frameIdx == _fadeOutClipInfo.frameIdx)
            return;
        _fadeOutClipInfo.frameIdx = frameIdx;

        //if(_crossFadeInfo.flag && _crossFadeInfo.elapsed / _crossFadeInfo.fadeLength > 0.5f)
        //{
        //    UnityEditor.EditorApplication.isPaused = true;
        //    _crossFadeInfo.flag = false;
        //}
    }

    private void StopCrossFade()
    {
        _crossFadeInfo.Reset();
        foreach(var bsmr in _bakedRenderers)
        {
            bsmr.EndCrossFade();
        }
        Debug.Log("<color=yellow>CrossFade End</color>");
        Time.timeScale = 1f;
        //UnityEditor.EditorApplication.isPaused = true;
        //Time.timeScale = 1f;
    }


    private void UpdateBakedSmr()
    {
        bool isFading = _crossFadeInfo.isCrossFading;

        int frameIndex = _currClipInfo.frameIdx;
        int fadeOutFrameIdx = _fadeOutClipInfo.frameIdx;
        //if (_crossFadeInfo.isCrossFading)
        //    fadeOutFrameIdx = frameIndex;

        float fadeOutPercent = isFading ? _crossFadeInfo.elapsed / _crossFadeInfo.fadeLength : 1;
        foreach (var bsmr in _bakedRenderers)
        {
            bsmr.UpdateFrameIndex(frameIndex, fadeOutFrameIdx, fadeOutPercent);
        }
    }

    private void UpdateJoints()
    {
        for (int i = 0; i < _jointTrans.Length; i++)
        {
            Transform t = _jointTrans[i];
            PosRot posRot = skinningData.jointDatas[_currClipInfo.clipIdx].frameData[_currClipInfo.frameIdx].jointData[i];
            t.localPosition = posRot.position;
            t.localRotation = posRot.rotation;
        }
    }

    private void DoRootMotion()
    {

    }

    private void UpdateCulling()
    {

    }

    private void ExecuteEvents()
    {
        EventInfo[] events = _currClipInfo.bakedClipInfo.events;
        if (events.Length == 0)
            return;

        int eventsCnt = events.Length;
        int lastEvtIdx = _currClipInfo.lastExecutedEventIdx;
        int beginIdx = System.Math.Min(lastEvtIdx + 1, eventsCnt - 1);
        int endIdx = beginIdx;

        // 还没到下一个 event 的时间
        if (_currClipInfo.time < events[beginIdx].time)
            return;

        // 已执行完最后一个 event
        if (lastEvtIdx == -1 && _currClipInfo.time > events[eventsCnt - 1].time)
            return;

        for (int i = beginIdx; i < eventsCnt; i++)
        {
            if (events[i].time > _currClipInfo.time)
                break;
            endIdx = i;
        }

        for (int i = beginIdx; i <= endIdx; i++)
        {
            DoEvent(_currClipInfo.bakedClipInfo.events[i], i);
        }

        _currClipInfo.lastExecutedEventIdx = endIdx == events.Length - 1 ? -1 : endIdx;
    }

    private void DoEvent(EventInfo evtInfo, int idx)
    {
        //Debug.LogFormat("Execute <color=yellow>Event[{0}] {1}</color> at {2} with param:<color=yellow>{3}</color>", idx, evtInfo.functionName, _currClipInfo.time, evtInfo.stringParameter);
    }


    /// <summary>
    /// 将除 mesh 节点，挂点(将其提到最顶级)，rootMotion 之外的所有节点移除
    /// </summary>
    private void ProcessNode()
    {
        _rootMotionNode = transform.Find(Consts.ROOT_MOTION_NAME);

        List<Transform> allChildren = new List<Transform>();
        Utils.GetAllChildren(transform, allChildren);

        List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
        foreach(var node in allChildren)
        {
            if (node == transform) continue;

            SkinnedMeshRenderer smr = node.GetComponent<SkinnedMeshRenderer>();
            if(smr != null)
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
            foreach(var name in skinningData.jointNames)
            {
                if(name == node.name)
                {
                    node.parent = transform;
                    isJoint = true;
                    break;
                }
            }

            if(!isJoint)
            {
                // 不要使用 DestroyImmediate, 否则循环中的其它元素可能无法访问并且下面的 bsmr.Init 无法获取到 bones
                Destroy(node.gameObject);
            }
        }

        foreach(var smr in smrs)
        {
#if UNITY_EDITOR
            BakedSkinningMeshRenderer bsmr = smr.gameObject.AddComponent<BakedSkinningMeshRenderer>();
#else
            BakedSkinningMeshRenderer bsmr = new BakedSkinningMeshRenderer();
#endif
            bsmr.Init(this, smr);
            DestroyImmediate(smr);
            _bakedRenderers.Add(bsmr);
        }

        Animation oriAnimation = gameObject.GetComponent<Animation>();
        if (oriAnimation != null)
            DestroyImmediate(oriAnimation);

        // 初始化绑点
        _jointTrans = new Transform[skinningData.jointNames.Length];
        for(int i = 0; i < _jointTrans.Length; i++)
        {
            Transform t = transform.Find(skinningData.jointNames[i]);
            if(t == null)
            {
                Debug.LogErrorFormat("can not find join {0}", skinningData.jointNames[i]);
                return;
            }

            _jointTrans[i] = t;
        }
    }

    private int FindClipIdx(string clipName)
    {
        var clipInfos = skinningData.clipInfos;
        int clipCount = clipInfos.Length;
        for(int i = 0; i < clipCount; i++)
        {
            if(clipInfos[i].name == clipName)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsValidClipIdx(int index)
    {
        if (index < 0 || index >= skinningData.clipInfos.Length)
            return false;

        return true;
    }
#endregion
}