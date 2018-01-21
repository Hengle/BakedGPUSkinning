using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 虚拟骨骼节点，用来计算矩阵
/// </summary>
public class VirtualBoneTransform
{
    public string name;
    public bool dirty
    {
        get { return _dirty; }
        set
        {
            _dirty = true;
            SetChildrenDirty();
        }
    }

    public Matrix4x4    localToParentMatrix
    {
        get { return _localToParentMatrix; }
        private set
        {
            _localToParentMatrix = value;
            dirty = true;
            SetChildrenDirty();
        }
    }

    public Matrix4x4                    localToWorldMatrix { get { return _localToWorldMatrix; } } // localToModel

    public VirtualBoneTransform         parent;
    public List<VirtualBoneTransform>   children;

    private bool                        _dirty;
    private Matrix4x4                   _localToParentMatrix;
    private Matrix4x4                   _localToWorldMatrix;

    public void Update()
    {
        if (!dirty)
            return;

        if (parent != null)
        {
            Debug.Assert(parent.dirty == false, "parent must update before children");
            _localToWorldMatrix = parent.localToWorldMatrix * _localToParentMatrix;
        }
        else
            _localToWorldMatrix = _localToParentMatrix;

        _dirty = false;
    }

    public void SetChildrenDirty()
    {
        foreach (var child in children)
            child.dirty = true;
    }
}
