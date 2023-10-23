using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShower : MonoBehaviour {
    public float spinDuration = 1.0f;
    public float upDownDuration = 1.0f;
    public AnimationCurve upDownCurve;
    public bool randomStartProcess = false;
    private Vector3 oriPosLocal;
    private float timeCount;

    void Awake() {
        oriPosLocal = transform.localPosition;
        if (randomStartProcess) 
            timeCount += Random.Range(0, 100.0f);
            
    }

    void Update() {
        timeCount += Time.deltaTime;
        transform.localPosition = oriPosLocal + upDownCurve.Evaluate((timeCount % upDownDuration) / upDownDuration) * Vector3.up;
        transform.rotation = Quaternion.Euler(0, 360.0f * ((timeCount % spinDuration) / spinDuration),0);
    }

}
