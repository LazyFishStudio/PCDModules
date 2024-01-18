using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PCDFollow : MonoBehaviour {
    public Transform followTarget;
    public Vector3 followOffset;

    void Update() {
        transform.position = followTarget.position + followOffset;
    }
}
