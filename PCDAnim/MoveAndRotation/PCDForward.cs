using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PCDForward : MonoBehaviour {
    public Transform center;
    public Transform forward;
    public Transform up;
    
    void Update() {
        transform.rotation = Quaternion.LookRotation(forward.position - center.position, up.position - center.position);
    }
}
