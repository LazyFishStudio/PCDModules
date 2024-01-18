using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PCDTransfromLerp : MonoBehaviour
{
    public Transform lerpingPoint;
    public Transform startPoint;
    public Transform endPoint;
    [Range(0, 1.0f)]
    public float t;

    void Update() {
        lerpingPoint.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
    }
}
