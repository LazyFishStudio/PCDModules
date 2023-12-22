using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDBone : MonoBehaviour
{
    public string boneName;
    public PCDBoneDriver owner;
    public bool forceOwner;
    private Vector3 oriPosLocal;
    private Quaternion oriRotLocal;

    private void Awake() {
        oriPosLocal = transform.localPosition;
        oriRotLocal = transform.localRotation;
    }

    public void ResetToOriTrans() {
        transform.localPosition = oriPosLocal;
        transform.localRotation = oriRotLocal;
    }

    public void SetOwnership(PCDBoneDriver driver, bool force = false) {
        owner = driver;
        forceOwner = force;
    }
    public void ResetOwnership() {
        owner = null;
        forceOwner = false;
    }
}
