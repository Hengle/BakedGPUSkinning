using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUSkinning;

namespace GPUSkinning
{
    public static class SharedData
    {
        public static bool isSync;
        public static float time;
        public static SkinningData skinningData;
        public static int frameIdx;
        public static int crossFadeIdx;
        public static float crossFadeTime;
    }

    [System.Serializable]
    public class TRData
    {
        public string name;
        public Vector3 srcPos;
        public Vector3 dstPos;
        public Vector3 srcRotate;
        public Vector3 dstRotate;

        [HideInInspector]
        public Matrix4x4 bindPoseInv;

        [HideInInspector]
        public Transform oriT;
    }

    public class SyncAnims : MonoBehaviour
    {

        public Vector3 frameInfo;
        public Transform srcBoneRoot;
        public Transform srcWeaponT;
        public Transform dstWeaponT;


        public TRData[] trDatas;

        private AnimationState _srcAniState;
        private int _pixelPerFrame;
        private Animation _ani;

        // Use this for initialization
        void Start()
        {
            _ani = GetComponent<Animation>();
            //_srcAniState = _ani["airshot"];
        }

        private void OnEnable()
        {
            SharedData.isSync = true;
        }

        private void OnDisable()
        {
            SharedData.isSync = false;
        }

        private float _lastTime = 0;
        void Update()
        {
            if (dstWeaponT == null)
                return;

            //SharedData.time = SharedData.crossFadeTime + 1f / 60;
            //return;

            if (trDatas == null || trDatas.Length == 0)
                InitBones();

            BakedClipInfo clipInfo = SharedData.skinningData.clipInfos[0];

            frameInfo.x = SharedData.crossFadeIdx;
            frameInfo.y = clipInfo.frameCount;
            frameInfo.z = clipInfo.frameRate * (_lastTime % clipInfo.duration);
            _lastTime = SharedData.time;

            int byteOffset = _pixelPerFrame * 8 * SharedData.crossFadeIdx;

            byte[] buff = SharedData.skinningData.boneDatas;
            for (int i = 0; i < trDatas.Length; i++)
            {
                Vector4[] rows = new Vector4[4];

                for (int j = 0; j < 3; j++) // 3 pixel per bone
                {
                    Vector4 row = new Vector4();
                    row.x = ReadFloat(buff, byteOffset);
                    byteOffset += 2;
                    row.y = ReadFloat(buff, byteOffset);
                    byteOffset += 2;
                    row.z = ReadFloat(buff, byteOffset);
                    byteOffset += 2;
                    row.w = ReadFloat(buff, byteOffset);
                    byteOffset += 2;

                    rows[j] = row;
                }
                rows[3] = new Vector4(0, 0, 0, 1);

                Matrix4x4 mat = new Matrix4x4();
                mat.SetRow(0, rows[0]);
                mat.SetRow(1, rows[1]);
                mat.SetRow(2, rows[2]);
                mat.SetRow(3, rows[3]);
                mat *= trDatas[i].bindPoseInv;

                if (i == 0 && SharedData.frameIdx == 2)
                {
                    Vector3 posA, scaleA;
                    Quaternion rotateA;
                    Matrix4x4Helper.Decompose(trDatas[i].bindPoseInv, out scaleA, out rotateA, out posA);
                    posA.x += 1;
                }

                Vector3 pos, scale;
                Quaternion rotate;
                Matrix4x4Helper.Decompose(mat, out scale, out rotate, out pos);

                trDatas[i].dstPos = pos;
                trDatas[i].dstRotate = rotate.eulerAngles;

                trDatas[i].srcPos = trDatas[i].oriT.position - new Vector3(0.5f, 0, 0);
                trDatas[i].srcRotate = trDatas[i].oriT.rotation.eulerAngles;
            }

        }

        private float ReadFloat(byte[] buff, int offset)
        {
            System.Half half = System.Half.ToHalf(buff, offset);
            return half;
        }

        void InitBones()
        {
            BoneInfo[] boneInfos = SharedData.skinningData.boneInfos;
            List<Transform> allTs = new List<Transform>();
            GPUAnimUtils.GetAllChildren(srcBoneRoot, allTs);

            trDatas = new TRData[boneInfos.Length];
            for (int i = 0; i < trDatas.Length; i++)
            {
                TRData trData = new TRData();
                trData.name = boneInfos[i].name;
                trData.bindPoseInv = boneInfos[i].bindPose.inverse;
                foreach (var t in allTs)
                {
                    if (t.name == trData.name)
                    {
                        trData.oriT = t;
                        break;
                    }
                }

                trDatas[i] = trData;
            }

            _pixelPerFrame = boneInfos.Length * 3;
        }

        private void executeEvent(string str)
        {

        }
    }

}
