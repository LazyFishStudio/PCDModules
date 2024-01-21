using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PCDFollow : MonoBehaviour {
    public bool isLateUpdate;
    public Transform followTarget;
    public Vector3 followOffset;

    void Update() {
        if (isLateUpdate) {
            return;
        }
        transform.position = followTarget.position + followOffset;
    }

    void LateUpdate() {
        if (!isLateUpdate) {
            return;
        }
        transform.position = followTarget.position + followOffset;
    }
}
