using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class BakedGPUAnimation : MonoBehaviour, IGPUAnimation
    {
        public SkinningData     skinningData { get; private set; }
        public float            speed = 1.0f;

        public bool             isPlaying { get; private set; }
        public bool             isPaused { get; private set; }
        public string           playingClip { get { return _currClipInfo.clipIdx == -1 ? string.Empty : _currClipInfo.bakedClipInfo.name; } }

        public PosRot           posRot { get; private set; }
        public Texture2D        animTexture { get; private set; }

        
        
        private List<BakedGPUSkinnedMeshRenderer>   _bakedRenderers;
        private Transform[]                         _jointTrans;

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
            public bool     isCrossFading;
            public float    fadeLength;
            public float    elapsed;

#if DEBUG
            public bool     flag;
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
            skinningData = GetComponent<GPUAnimationPlayer>().skinningData;
            _bakedRenderers = new List<BakedGPUSkinnedMeshRenderer>();
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
        }

        public bool Play(string animation)
        {
            int clipIdx = FindClipIdx(animation);
            if (clipIdx == -1)
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

            _currClipInfo.clipIdx = index;
            _currClipInfo.bakedClipInfo = skinningData.clipInfos[index];
            _currClipInfo.time = 0;
            _currClipInfo.lastExecutedEventIdx = -1;

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
            if (_currClipInfo.clipIdx != -1)
            {
                isPaused = false;
                isPlaying = true;
            }
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


        void Update()
        {
            if (isPlaying == false || isPaused)
                return;

            _deltaTime = Time.deltaTime * speed;

            UpdateCurrAnimClip();
            UpdateBakedSmr();
            UpdateJoints();
            UpdateCulling();
            DoRootMotion();
            ExecuteEvents();
        }

        public void AddMeshRenderer(GPURendererRes res)
        {
            BakedGPUSkinnedMeshRenderer bsmr = new BakedGPUSkinnedMeshRenderer();
            bsmr.Init(this, res);

            _bakedRenderers.Add(bsmr);
        }

        public void SetJointTransforms(Transform[] trans)
        {
            _jointTrans = trans;
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
                PosRot posRot = skinningData.bakedJointDatas[_currClipInfo.clipIdx].frameData[_currClipInfo.frameIdx].jointData[i];
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


        

        private int FindClipIdx(string clipName)
        {
            var clipInfos = skinningData.clipInfos;
            int clipCount = clipInfos.Length;
            for (int i = 0; i < clipCount; i++)
            {
                if (clipInfos[i].name == clipName)
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
}
