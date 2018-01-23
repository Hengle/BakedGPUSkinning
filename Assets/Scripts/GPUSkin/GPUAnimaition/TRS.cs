using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUSkinning
{
    public class TRS
    {
        public Quaternion   rotation;
        public Vector3      position;
        public Vector3      scale;

        public bool IsEqual(Transform transform)
        {
            Quaternion rhsRot = transform.rotation;
            Vector3 rhsPos = transform.position;
            Vector3 rhsScale = transform.lossyScale;

            return Mathf.Approximately(rotation.x, rhsRot.x)
                && Mathf.Approximately(rotation.y, rhsRot.y)
                && Mathf.Approximately(rotation.z, rhsRot.z)
                && Mathf.Approximately(rotation.w, rhsRot.w)
                && Mathf.Approximately(position.x, rhsPos.x)
                && Mathf.Approximately(position.y, rhsPos.y)
                && Mathf.Approximately(position.z, rhsPos.z)
                && Mathf.Approximately(scale.x, rhsScale.x)
                && Mathf.Approximately(scale.y, rhsScale.y)
                && Mathf.Approximately(scale.z, rhsScale.z);
        }
    }
}
