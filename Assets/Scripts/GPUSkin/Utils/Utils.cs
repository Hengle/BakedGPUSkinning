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
}
