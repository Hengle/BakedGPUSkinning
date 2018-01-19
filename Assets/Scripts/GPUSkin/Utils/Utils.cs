using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Utils
{
    public static void GetAllChildren(Transform node, List<Transform> result)
    {
        result.Add(node);
        int childCount = node.childCount;
        for(int i = 0; i < childCount; i++)
        {
            Transform child = node.GetChild(i);
            GetAllChildren(child, result);
        }
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T: Component
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
}
