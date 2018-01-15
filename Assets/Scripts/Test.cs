using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class Test : MonoBehaviour {

    public Animation srcAni;
	// Use this for initialization
	void Start () {
        GameObject prefab = Resources.Load<GameObject>("bundle/unit/pc/academic/academic_ingame");
        SkinningData skinningData = Resources.Load<SkinningData>("GPUSkinning/academic_ingame");
        if (prefab != null && skinningData != null)
        {
            GameObject go = Instantiate(prefab);
            go.transform.position = new Vector3(-0.5f, 0f, 0f);
            var bakedAni = go.AddComponent<BakedAnimation>();
            bakedAni.skinningData = skinningData;
            //bakedAni.speed = 0.2f;

            StartCoroutine(PlayAni(bakedAni, 0));
        }

        //Application.targetFrameRate = 30;
    }

    IEnumerator PlayAni(BakedAnimation bakedAni, int clipIdx)
    {
        yield return null;
        //bakedAni.Play("idle01");
        bakedAni.PlayByIndex(clipIdx);
        srcAni.Play("airshot");

        float fadeLength = 0.3f;

        while (true)
        {
            yield return new WaitForSeconds(2);
            bakedAni.CrossFade("airshot", fadeLength);
            srcAni.CrossFade("airshot", fadeLength);
            //yield break;
            yield return new WaitForSeconds(2);
            bakedAni.CrossFade("airshot", fadeLength);
            srcAni.CrossFade("airshot", fadeLength);
        }

    }

    // Update is called once per frame
    void Update () {
		
	}
}
