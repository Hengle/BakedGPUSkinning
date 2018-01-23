using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace GPUSkinning.Editor
{
    /// <summary>
    /// 从 AnimationClip 中提取 AnimationCurveData 的类
    /// </summary>
    public class ClipCurveDataProcessor
    {
        enum CurveType
        {
            None,
            Rotation,
            Position,
            Scale,
            Float,
        }


        public ClipCurveData[] curveDatas { get { return _curveDic.Values.ToArray(); } }

        private AnimationClip _clip;

        /// <summary>
        /// key: bone path, value: curve data
        /// </summary>
        private Dictionary<string, ClipCurveData> _curveDic = new Dictionary<string, ClipCurveData>();

        public ClipCurveDataProcessor(AnimationClip clip)
        {
            _clip = clip;

            ProcessClip();
        }

        private void ProcessClip()
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_clip);
            foreach (var binding in bindings)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(_clip, binding);
                ProcessCurve(curve, binding);
            }

            // 某些 Curve 只有两个关键帧并且值是相同的，去掉此类 Curve 的第二个关键帧以节省空间
            foreach (ClipCurveData data in _curveDic.Values)
            {
                RotationCurve rotCurve = data.rotationCurve;
                if (rotCurve.keys.Length == 2 && rotCurve.keys[0] == rotCurve.keys[1])
                    rotCurve.keys = new RotationKeyFrame[1] { rotCurve.keys[0] };

                Vector3Curve posCurve = data.positionCurve;
                if (posCurve.keys.Length == 2 && posCurve.keys[0] == posCurve.keys[1])
                    posCurve.keys = new Vector3KeyFrame[1] { posCurve.keys[0] };

                Vector3Curve scaleCurve = data.scaleCurve;
                if (scaleCurve.keys.Length == 2 && scaleCurve.keys[0] == scaleCurve.keys[1])
                {
                    if (scaleCurve.keys[0].value == Vector3.one) // 这种情况就不要记录曲线了
                        data.scaleCurve = null;
                    else
                        scaleCurve.keys = new Vector3KeyFrame[1] { scaleCurve.keys[0] };
                }
            }

        }

        private void ProcessCurve(AnimationCurve curve, EditorCurveBinding binding)
        {
            ClipCurveData curveData = GetOrAddCurveData(binding.path);
            CurveType curveType = GetCurveType(binding);
            switch (curveType)
            {
                case CurveType.Rotation:
                    SetRotCurveData(curveData.rotationCurve, curve, binding.propertyName);
                    break;
                case CurveType.Position:
                    SetVector3CurveData(curveData.positionCurve, curve, binding.propertyName);
                    break;
                case CurveType.Scale:
                    SetVector3CurveData(curveData.scaleCurve, curve, binding.propertyName);
                    break;
                default:
                    break;
            }
        }


        private CurveType GetCurveType(EditorCurveBinding binding)
        {
            if (binding.propertyName.StartsWith("m_LocalRotation"))
                return CurveType.Rotation;
            if (binding.propertyName.StartsWith("m_LocalPosition"))
                return CurveType.Position;
            if (binding.propertyName.StartsWith("m_LocalScale"))
                return CurveType.Scale;
            return CurveType.None;
        }

        private void SetRotCurveData(RotationCurve rotCurve, AnimationCurve curve, string propertyName)
        {
            int cnt = curve.keys.Length;
            Keyframe[] srcKeys = curve.keys;

            if (rotCurve.keys == null)
                rotCurve.keys = new RotationKeyFrame[cnt];

            rotCurve.preWrapMode = curve.preWrapMode;
            rotCurve.postWrapMode = curve.postWrapMode;
            //rotCurve.rotationOrder  = RotationOrder.Default;

            if (propertyName.EndsWith(".x"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    rotCurve.keys[i].time = keyFrame.time;
                    rotCurve.keys[i].value.x = keyFrame.value;
                    rotCurve.keys[i].inSlope.x = keyFrame.inTangent;
                    rotCurve.keys[i].outSlope.x = keyFrame.outTangent;
                }
            }
            else if (propertyName.EndsWith(".y"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    rotCurve.keys[i].time = keyFrame.time;
                    rotCurve.keys[i].value.y = keyFrame.value;
                    rotCurve.keys[i].inSlope.y = keyFrame.inTangent;
                    rotCurve.keys[i].outSlope.y = keyFrame.outTangent;
                }
            }
            else if (propertyName.EndsWith(".z"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    rotCurve.keys[i].time = keyFrame.time;
                    rotCurve.keys[i].value.z = keyFrame.value;
                    rotCurve.keys[i].inSlope.z = keyFrame.inTangent;
                    rotCurve.keys[i].outSlope.z = keyFrame.outTangent;
                }
            }
            else if (propertyName.EndsWith(".w"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    rotCurve.keys[i].time = keyFrame.time;
                    rotCurve.keys[i].value.w = keyFrame.value;
                    rotCurve.keys[i].inSlope.w = keyFrame.inTangent;
                    rotCurve.keys[i].outSlope.w = keyFrame.outTangent;
                }
            }
        }

        private void SetVector3CurveData(Vector3Curve vec3urve, AnimationCurve curve, string propertyName)
        {
            int cnt = curve.keys.Length;
            Keyframe[] srcKeys = curve.keys;

            if (vec3urve.keys == null)
                vec3urve.keys = new Vector3KeyFrame[cnt];

            vec3urve.preWrapMode = curve.preWrapMode;
            vec3urve.postWrapMode = curve.postWrapMode;
            //vec3urve.rotationOrder  = RotationOrder.Default;

            if (propertyName.EndsWith(".x"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    vec3urve.keys[i].time = keyFrame.time;
                    vec3urve.keys[i].value.x = keyFrame.value;
                    vec3urve.keys[i].inSlope.x = keyFrame.inTangent;
                    vec3urve.keys[i].outSlope.x = keyFrame.outTangent;
                }
            }
            else if (propertyName.EndsWith(".y"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    vec3urve.keys[i].time = keyFrame.time;
                    vec3urve.keys[i].value.y = keyFrame.value;
                    vec3urve.keys[i].inSlope.y = keyFrame.inTangent;
                    vec3urve.keys[i].outSlope.y = keyFrame.outTangent;
                }
            }
            else if (propertyName.EndsWith(".z"))
            {
                for (int i = 0; i < cnt; i++)
                {
                    Keyframe keyFrame = srcKeys[i];
                    vec3urve.keys[i].time = keyFrame.time;
                    vec3urve.keys[i].value.z = keyFrame.value;
                    vec3urve.keys[i].inSlope.z = keyFrame.inTangent;
                    vec3urve.keys[i].outSlope.z = keyFrame.outTangent;
                }
            }
        }

        private ClipCurveData GetOrAddCurveData(string path)
        {
            ClipCurveData ret;
            if (!_curveDic.TryGetValue(path, out ret))
            {
                ret = new ClipCurveData();
                ret.path = path.Substring(path.LastIndexOf('/') + 1);
                ret.rotationCurve = new RotationCurve();
                ret.positionCurve = new Vector3Curve();
                ret.scaleCurve = new Vector3Curve();
                _curveDic.Add(path, ret);
            }
            return ret;
        }

    }
}
