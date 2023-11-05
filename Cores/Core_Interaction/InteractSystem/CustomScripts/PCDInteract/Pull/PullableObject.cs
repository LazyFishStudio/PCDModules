using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using UnityEngine;
using InteractSystem;

public class PullableObject : PickableObject {
    public bool destroyOnPullOut = true;
    public Transform root;
    public Transform lHaft;
    public Transform rHaft;
    public PullInfo pullInfo;
    public GameObject pickableObjectPrefab;
    public GameObject pulledOutEffect;

    public Action pulloutCallback;

    public float rootScale => root.localScale.y;
    public virtual void OnPulledOut() {
        if (pulledOutEffect)
            GameObject.Instantiate(pulledOutEffect, transform.position, Quaternion.identity);
        if (destroyOnPullOut) {
            if (attachedPlace != null) {
                RemovedFrom(null);
            }
            GameObject.Destroy(gameObject);
        }
        GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);

        pulloutCallback?.Invoke();
    }

}
