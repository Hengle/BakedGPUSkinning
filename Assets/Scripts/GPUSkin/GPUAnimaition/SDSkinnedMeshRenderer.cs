using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDSkinnedMeshRenderer
{
    private Mesh        _mesh;
    private Mesh        _skinMesh;
    private int[]       _boneIdxMap;
    private Vector4[]   _matrixPalette; // _boneIdxMap.Length * 3   
    private Material    _material;

    public void Init(SDAnimation sdAnim, SkinnedMeshRenderer smr, int[] boneIdxMap)
    {
        _mesh = smr.sharedMesh;
        _boneIdxMap = boneIdxMap;
        _matrixPalette = new Vector4[_boneIdxMap.Length * 3];

        Material srcMat = smr.sharedMaterial;

        Material newMat = new Material(Shader.Find("GPUSkinning/BakedGPUSkinning"));
        newMat.SetTexture("_MainTex", srcMat.mainTexture);
        _material = newMat;

        
    }

    public void Update()
    {

    }
}
