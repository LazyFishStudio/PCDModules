using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using InteractSystem;

public class PullableObject : PickableObject {
    public Transform puller;
    public bool destroyOnPullOut = true;
    public Transform root;
    public Transform lHaft;
    public Transform rHaft;
    public PullInfo pullInfo;
    public GameObject pickableObjectPrefab;
    public GameObject pulledOutEffect;
    public Action pulloutCallback;
    public float pullOutProcessDeathZone = 1.0f;
    [Range(0, 1.0f)]
    public float pullOutProcess;
    public float deltaPullOutProcess; // 可以检测PullableObject被向后拖动和被向前拉回，触发一些事件
    [SerializeField]
    private float lastPullOutProcess;
    private float pullOutTimeCount;

    protected virtual void Update() {
        if (puller) {
            UpdatePulling();
            deltaPullOutProcess = pullOutProcess - lastPullOutProcess;
            lastPullOutProcess = pullOutProcess;
        } else {
            pullOutProcess = deltaPullOutProcess = lastPullOutProcess = 0;
        }
    }

    void FixedUpdate() {
        if (puller) {
            Rigidbody rb = puller.GetComponent<Rigidbody>();
            rb.AddForce((transform.position - puller.position) * pullInfo.contractStrength * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void UpdatePulling() {

        float dis = Vector3.Distance(puller.position, transform.position);
        if (dis < pullOutProcessDeathZone) {
            pullOutProcess = 0;
        } else {
            pullOutProcess = Mathf.Clamp01((dis - pullOutProcessDeathZone) / (pullInfo.maxStretchLength - pullOutProcessDeathZone));
        }

        // Rigidbody rb = puller.GetComponent<Rigidbody>();
        // rb.AddForce((transform.position - puller.position) * pullInfo.contractStrength * Time.deltaTime, ForceMode.Acceleration);

        float stretchDis = Vector3.Distance(transform.position.ClearY(), puller.position.ClearY());
        float maxStretchLengthScale = pullInfo.maxStretchLength * rootScale;

        if (stretchDis > maxStretchLengthScale) {
            pullOutTimeCount += Time.deltaTime;
        } else {
            pullOutTimeCount = Mathf.Max(0, pullOutTimeCount - Time.deltaTime);
        }

        if (pullOutTimeCount >= pullInfo.pullOutTime) {
            OnPulledOut();
            PCDHumanPullSM pullComp = puller.GetComponent<PCDHumanPullSM>();
            if (pullComp) {
                pullComp.OnPullOutObj();
                pullComp.RestPulling();
                // 要呼叫IKController恢复
            }

            RestPull();

            // emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
            return;
        }

    }

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

    public virtual void OnRestPull() {

    }

    public virtual void StartPullBy(Transform puller, Transform followTarget) {
        this.puller = puller;
        // 设置 PullablePCDIK override
        GetComponentInParent<PullablePCDIKController>()?.SetFollowTargetOverride(followTarget, pullInfo.archoringTransition);
        GetComponentInParent<DraggableObject>()?.StartDragBy(puller);
    }

    public virtual void RestPull() {
        if (!puller) {
            return;
        }
        puller = null;
        OnRestPull();
        // 解除 PullablePCDIK override
        GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
        GetComponentInParent<DraggableObject>()?.RestDrag();
    }
    

}

[System.Serializable]
public class PullInfo {
    public float maxStretchLength = 2.5f;
    public float contractStrength = 250.0f;
    public float pullOutTime = 0.25f;
    public float archoringTransition = 0.1f;

}