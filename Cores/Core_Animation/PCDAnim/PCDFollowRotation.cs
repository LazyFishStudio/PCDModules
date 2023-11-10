using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PCDFollowRotation : MonoBehaviour {
    public Transform followTarget;

    void Update() {
        transform.rotation = followTarget.rotation;
    }
}
