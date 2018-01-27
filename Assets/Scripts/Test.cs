using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class Test : MonoBehaviour {
    public float timescale = 1.0f;
    public Animation srcAni;
    // Use this for initialization
    void Start() {
        GameObject prefab = Resources.Load<GameObject>("bundle/unit/pc/academic/academic_ingame");
        SkinningData skinningData = Resources.Load<SkinningData>("GPUSkinning/academic_ingame");
        if (prefab != null && skinningData != null) {
            GameObject go = Instantiate(prefab);
            go.transform.position = new Vector3(-0.5f, 0f, 0f);
            var bakedAni = go.AddComponent<BakedAnimation>();
            bakedAni.skinningData = skinningData;
            //bakedAni.speed = 0.2f;

            StartCoroutine(PlayAni(bakedAni, 0));
        }

        //Application.targetFrameRate = 30;
    }

    void OnValidate() {
        Time.timeScale = Mathf.Max(timescale, 0.1f);
    }

    IEnumerator PlayAni(BakedAnimation bakedAni, int clipIdx) {
        yield return null;

        List<string> names = new List<string>();
        foreach (AnimationState _state in srcAni) {
            names.Add(_state.name);
        }

        bakedAni.PlayByIndex(clipIdx);
        srcAni.Play("airshot");

        float fadeLength = 0.1f;

        while (true) {
            //foreach (var name in names) {
            //    yield return new WaitForSeconds(2);
            //    bakedAni.CrossFade(name, fadeLength);
            //    srcAni.CrossFade(name, fadeLength);
            //}
            yield return new WaitForSeconds(2);
            bakedAni.CrossFade("ride_run", fadeLength);
            srcAni.CrossFade("ride_run", fadeLength);
            yield return new WaitForSeconds(2);
            bakedAni.CrossFade("airshot", fadeLength);
            srcAni.CrossFade("airshot", fadeLength);
        }

    }

    // Update is called once per frame
    void Update() {

    }
}
