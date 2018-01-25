using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using SDGame.Util;

#if UNITY_EDITOR
public class GPUSkinSampler : MonoBehaviour
{
    public const string SKINNING_DATA_SAVE_DIR = "Assets/Resources/GPUSkinning/";
    public const int SAMPLER_FRAME_RATE = 60;

    private Animation               _animation;
    private AnimationClip[]         _clips;
    private Transform               _boneRoot;
    private Transform               _rootMotionNode;

    private List<BoneSampleData>    _allBoneDatas;
    private List<JointSampleData>   _allJointDatas;
    private RootMotionData          _rootMotionData;

    private bool                    _isSampling;
    private List<SampleParam>       _sampleParams;
    private SampleParam             _sampleParam;

    private List<string>            _exposedJoints;

    private int                     _allFrameCount;
    private int                     _elaspedFrameCount;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        Init();
    }
    void Start()
    {
        StartSampleClip(0);
    }

    [ContextMenu("sample clip")]
    private void BeginSample() {
        Application.targetFrameRate = -1;
        Init();
        StartSampleClip(0);

        for (int i = 1, imax = _clips.Length; i < imax; i++) {
            _sampleParam.aniState.enabled = false;
            _sampleParam.clipIdx++;
            StartSampleClip((short)i);

            _sampleParam.aniState.time = _sampleParam.currFrameIdx / _sampleParam.frameRate;
            _animation.Sample();
            GetSampleFrameData();
            _sampleParam.currFrameIdx++;
            _elaspedFrameCount++;
            EditorUtility.DisplayProgressBar(string.Format("正在采样动画 {0} ({1}/{2})", transform.name, _sampleParam.clipIdx + 1, _clips.Length)
                , string.Format("frame: {0:000} / {1:000},   clip: {2}", _sampleParam.currFrameIdx, _sampleParam.frameCount, _sampleParam.clip.name)
                , _elaspedFrameCount * 1f / _allFrameCount);
        }

        WriteToFile();
        EditorUtility.ClearProgressBar();
        EditorApplication.isPlaying = false;
        EditorUtility.DisplayDialog("提示", "动画采样完毕", "确定");
    }


    void StartSampleClip(short clipIdx)
    {
        _isSampling = true;

        if(_sampleParam != null)
        {
            _sampleParam.aniState.enabled = false;
        }

        AnimationClip clip = _clips[clipIdx];

        _sampleParam = new SampleParam();
        _sampleParam.clipIdx = clipIdx;
        _sampleParam.clip = clip;

        AnimationState aniState = _animation[clip.name];
        aniState.enabled        = true;
        aniState.weight         = 1;
        aniState.normalizedTime = 0f;
        aniState.speed          = 1f;
        _sampleParam.aniState = aniState;

        _sampleParam.frameRate       = SAMPLER_FRAME_RATE;
        _sampleParam.frameCount      = (short)(clip.length * SAMPLER_FRAME_RATE);
        _sampleParam.currFrameIdx    = 0;

        transform.parent        = null;
        transform.position      = Vector3.zero;
        transform.rotation      = Quaternion.identity;
        transform.localScale    = Vector3.one;

        foreach(var boneData in _allBoneDatas)
        {
            boneData.matrixes[_sampleParam.clipIdx]       = new Matrix4x4[_sampleParam.frameCount];
        }

        foreach(var jointData in _allJointDatas)
        {
            jointData.matrixes[_sampleParam.clipIdx] = new PosRot[_sampleParam.frameCount];
        }

        _rootMotionData.matrixes[_sampleParam.clipIdx] = new PosRot[_sampleParam.frameCount];

        _sampleParams.Add(_sampleParam);

        Debug.LogFormat("<color=yellow>Start sample clip <{0}>[{1}],length:{2},frameRate:{3},frameCount:{4}</color>", clip.name, clipIdx, clip.length, _sampleParam.frameRate, _sampleParam.frameCount);
    }

    void Update()
    {
        if (_isSampling == false)
            return;

        if (_sampleParam.currFrameIdx >= _sampleParam.frameCount)
        {
            _isSampling = false;
            _sampleParam.aniState.enabled = false;

            if (_sampleParam.clipIdx < _clips.Length - 1)
                //if (_sampleParam.clipIdx < 2)
            {
                _sampleParam.clipIdx++;
                StartSampleClip(_sampleParam.clipIdx);
            }else
            {
                WriteToFile();

                EditorUtility.ClearProgressBar();
                EditorApplication.isPlaying = false;
                EditorUtility.DisplayDialog("提示", "动画采样完毕", "确定");
                return;
            }
        }

        _sampleParam.aniState.time = _sampleParam.currFrameIdx / _sampleParam.frameRate;
        _animation.Sample();
        GetSampleFrameData();
        _sampleParam.currFrameIdx++;

        _elaspedFrameCount++;
        EditorUtility.DisplayProgressBar(string.Format("正在采样动画 {0} ({1}/{2})", transform.name, _sampleParam.clipIdx + 1, _clips.Length)
            , string.Format("frame: {0:000} / {1:000},   clip: {2}", _sampleParam.currFrameIdx, _sampleParam.frameCount, _sampleParam.clip.name)
            , _elaspedFrameCount * 1f / _allFrameCount);
    }

    private void GetSampleFrameData()
    {
        if (_isSampling == false)
            return;

        Matrix4x4 matrix = Matrix4x4.identity;
        for (int i = 0, imax = _allBoneDatas.Count; i < imax; i++) {
            var boneData = _allBoneDatas[i];
            // 要求物体必须位于根节点并且无旋转无偏移, 否则就要自己逐级乘了
            var mat = boneData.transform.localToWorldMatrix * boneData.bindPose;
            boneData.matrixes[_sampleParam.clipIdx][_sampleParam.currFrameIdx] = mat;
        }

        foreach(var jointData in _allJointDatas)
        {
            Transform jointT = jointData.transform;
            jointData.matrixes[_sampleParam.clipIdx][_sampleParam.currFrameIdx] = new PosRot() { position = jointT.position, rotation = jointT.rotation };
        }

        _rootMotionData.matrixes[_sampleParam.clipIdx][_sampleParam.currFrameIdx] = new PosRot() { position = _rootMotionNode.position, rotation = _rootMotionNode.rotation };

        
    }

    private void WriteToFile()
    {
        string savePath = string.Format("{0}{1}.asset", SKINNING_DATA_SAVE_DIR, transform.name);

        SkinningData skinningData = AssetDatabase.LoadAssetAtPath<SkinningData>(savePath);
        if(skinningData == null)
        {
            skinningData = ScriptableObject.CreateInstance<SkinningData>();
            skinningData.name = transform.name;
            AssetDatabase.CreateAsset(skinningData, savePath);
        }

        skinningData.frameRate = SAMPLER_FRAME_RATE;
        skinningData.boneNames = _allBoneDatas.Select(boneData => boneData.transform.name).ToArray();
        skinningData.clipInfos = new BakedClipInfo[_sampleParams.Count];

        skinningData.bindPoses = new Matrix4x4[skinningData.boneNames.Length];
        for(int i = 0; i < skinningData.bindPoses.Length;i++)
        {
            skinningData.bindPoses[i] = _allBoneDatas[i].bindPose;
        }

        Vector2 texSize = CalcTextureSize();
        Texture2D tex = new Texture2D((int)texSize.x, (int)texSize.y, TextureFormat.RGBAHalf, false, true);
        Color[] pixels = tex.GetPixels();

        int pixelIdx = 0;

        /*
            layout: Clip -> FrameIdx -> Bone，存储完某一帧内所有骨头再存下一帧，原因是同一帧内数据相互靠近可以提高显存 cache 命中率
            简化存储可以使用 Dual Quaternion 或者只存储位移和旋转，然后在 shader 里还原(可能会耗费一定性能) 
         */
        for (int i = 0; i < _sampleParams.Count; i++)
        {
            SampleParam sampleParam = _sampleParams[i];
            BakedClipInfo clipData = new BakedClipInfo();

            int frameCnt = sampleParam.frameCount;

            clipData.name               = sampleParam.clip.name;
            clipData.frameCount         = sampleParam.frameCount;
            clipData.duration           = sampleParam.clip.length;
            clipData.wrapMode           = sampleParam.clip.wrapMode;
            clipData.localBounds        = sampleParam.clip.localBounds;
            clipData.clipPixelOffset    = pixelIdx;
            clipData.frameRate          = skinningData.frameRate; // TODO 先临时复制一下全局帧率
            skinningData.clipInfos[i]   = clipData;
            
            for(int j = 0; j < frameCnt; j++)
            {
                foreach (var boneData in _allBoneDatas)
                {
                    Matrix4x4 matrix = boneData.matrixes[i][j];
                    // 去掉第四行(0,0,0,1)
                    //matrix.GetRow(0);
                    pixels[pixelIdx++] = new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03);
                    pixels[pixelIdx++] = new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13);
                    pixels[pixelIdx++] = new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        skinningData.width      = (short)texSize.x;
        skinningData.height     = (short)texSize.y;
        skinningData.boneDatas  = tex.GetRawTextureData();

        /*
            save joints and rootMotion
        */
        skinningData.jointNames     = _allJointDatas.Select(jointData => jointData.transform.name).ToArray();
        skinningData.jointDatas     = new List<JointClipData>(); //new PosRot[_clips.Length][][];
        skinningData.rootMotions = new List<RootMotionClipData>(); //new PosRot[_clips.Length][];

        int jointCount = skinningData.jointNames.Length;
        for (int i = 0; i < _sampleParams.Count; i++)
        {
            SampleParam sampleParam = _sampleParams[i];
            int frameCnt = sampleParam.frameCount;
            skinningData.jointDatas.Add(new JointClipData());
            skinningData.jointDatas[i].clipName = sampleParam.clip.name;
            skinningData.rootMotions.Add(new RootMotionClipData());

            for (int j = 0; j < frameCnt; j++)
            {
                skinningData.jointDatas[i].frameData.Add(new JointFrameData());
                for(int k = 0; k < jointCount; k++)
                {
                    skinningData.jointDatas[i].frameData[j].jointData.Add(_allJointDatas[k].matrixes[i][j]);
                }

                skinningData.rootMotions[i].data.Add(_rootMotionData.matrixes[i][j]);
            }
        }

        /*
            save animation events
         */
        for (int i = 0; i < _sampleParams.Count; i++)
        {
            SampleParam sampleParam = _sampleParams[i];
            AnimationEvent[] aniEvents = sampleParam.clip.events;
            EventInfo[] events = new EventInfo[aniEvents.Length];
            for (int j = 0; j < aniEvents.Length; j++)
            {
                events[j] = new EventInfo(aniEvents[j]);
            }

            skinningData.clipInfos[i].events = events;
        }

        EditorUtility.SetDirty(skinningData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 计算 bake 的骨骼数据需要多大尺寸的纹理来保存
    /// </summary>
    /// <returns></returns>
    private Vector2 CalcTextureSize()
    {
        int pixelCount = 0;
        foreach(var sampleParam in _sampleParams)
        {
            pixelCount += sampleParam.frameCount * _allBoneDatas.Count * 3;
        }

        Vector2 result = new Vector2(1, 1);
        bool odd = true;
        while (true)
        {
            if (result.x * result.y > pixelCount)
                break;
            if (odd)
                result.x *= 2;
            else
                result.y *= 2;

            odd = !odd;
        }

        return result;
    }


    private void OnApplicationQuit()
    {
        EditorUtility.ClearProgressBar();
    }

    #region Init
    private void Init()
    {
        _isSampling = false;

        _exposedJoints = new List<string>() { "bip001 pelvis", "right_weapon", "~boxbone01" };
        _sampleParams = new List<SampleParam>();

        _animation = GetComponent<Animation>();
        _clips = BoneSampleUtil.GetClips(_animation);

        _boneRoot = transform.Find(Consts.BONE_ROOT_NAME);
        _rootMotionNode = transform.Find(Consts.ROOT_MOTION_NAME);
        InitBoneDatas();
        InitBindPose();
        InitJointDatas();
        InitRootMotion();
        GetAllFrameCount();
    }

    /// <summary>
    /// 从所有的子节点中查找 SkinnedMeshRenderer, 并在其中找到相符的骨骼，获取其 BindPose
    /// </summary>
    /// <remarks>经验证多个 SkinnedMeshRenderer 中相同的骨骼的 BindPose 是相同的</remarks>
    private void InitBindPose()
    {
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform[][] smrBones = new Transform[smrs.Length][];
        Matrix4x4[][] bindPoses = new Matrix4x4[smrs.Length][];
        for(int i = 0; i < smrs.Length; i++)
        {
            smrBones[i] = smrs[i].bones;
            bindPoses[i] = smrs[i].sharedMesh.bindposes;
        }

        foreach (var boneData in _allBoneDatas)
        {
            bool found = false;
            for(int i = 0; i < smrs.Length; i++)
            {
                Transform[] bones = smrBones[i];
                for(int j = 0; j < bones.Length; j++)
                {
                    if(bones[j] == boneData.transform)
                    {
                        boneData.bindPose = bindPoses[i][j];
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }
    }

    private void InitBoneDatas()
    {
        _allBoneDatas = new List<BoneSampleData>();
        BoneSampleUtil.GetBoneSampleDataRecursive(gameObject, _boneRoot, string.Empty, _allBoneDatas);

        foreach (var boneData in _allBoneDatas)
        {
            boneData.matrixes = new Matrix4x4[_clips.Length][];
        }
    }

    private void InitJointDatas()
    {
        _allJointDatas = new List<JointSampleData>();

        // Find Joints need to Exposed
        {
            List<Transform> allChildren = new List<Transform>();
            Utils.GetAllChildren(transform, allChildren);

            var joints = allChildren.Where(
                node => _exposedJoints.Where(
                    jointName => jointName == node.name
                    ).Count() > 0
                );

            int jointIdx = 0;
            foreach (var joint in joints)
            {
                JointSampleData jointData = new JointSampleData();
                jointData.transform = joint;
                jointData.jointIdx = jointIdx++;
                jointData.matrixes = new PosRot[_clips.Length][];

                _allJointDatas.Add(jointData);
            }
        }

        foreach (var jointData in _allJointDatas)
        {
            jointData.matrixes = new PosRot[_clips.Length][];
        }
    }

    private void InitRootMotion()
    {
        _rootMotionData             = new RootMotionData();
        _rootMotionData.transform   = _rootMotionNode;
        _rootMotionData.matrixes    = new PosRot[_clips.Length][];
    }

    private void GetAllFrameCount()
    {
        foreach(var clip in _clips)
        {
            _allFrameCount += (int)(clip.length * SAMPLER_FRAME_RATE);
        }
    }
#endregion

    private void executeEvent(string str)
    {

    }
}

#endif
