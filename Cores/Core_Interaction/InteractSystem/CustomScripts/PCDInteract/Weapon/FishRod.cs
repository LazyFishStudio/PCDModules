using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using DG.Tweening;
using Bros.Utils;

public class FishRod : PCDHoldProp
{
    public bool isFishing = false;
    public bool isBobberReady = false;
    public float targetHoldTime = 2f;
    public float progress = 0f;
    public float lineBias = 0f;
    public float lineTime = 0.2f;
    public Transform bobber;
    public Transform bobberIdle;
    public Transform bobberStart;
    public Transform bobberTarget;
    public PCDIK lineIK;

    private float holdTime = 0f;

    private void OnFishingStart(InteractComp interactor) {
        isFishing = true;
        holdTime = 0f;

        var locker = interactor.GetComponent<PCDActLocker>();
        locker.movementLocked = true;
        locker.dropLocked = true;

        // Animation
        SwingBobber(interactor.transform);

        interactType = "收竿";
    }

    private void OnFishingSucceed(InteractComp interactor) {
        // 动作 & 浮标回到竿上
        GatherBobber(interactor.transform);

        interactType = "甩竿";
    }

    private void OnFishingPause(InteractComp interactor) {

	}

    private FuncUpdater updater = null;
    private void SwingBobber(Transform player) {
        Rigidbody bobberRb = bobber.GetComponent<Rigidbody>();
        Vector3 targetPos = player.position + player.rotation * bobberTarget.localPosition;
        bobberRb.DOMove(bobberStart.position, 0f);
        bobber.SetParent(null);
        
        bobberRb.isKinematic = false;
        bobber.GetComponent<Collider>().enabled = true;
        bobberRb.DOJump(targetPos.CopySetY(0f), 3f, 1, 1f);

        EasyEvent.RegisterOnceCallback("BobberHit", () => {
            isBobberReady = true;
            targetHoldTime = 0f;

            float timeElapse = 0f;
            float startLength = (bobber.position - bobberStart.position).magnitude;
            updater = FuncUpdater.Create(() => {
                timeElapse += Time.deltaTime;
                float progress = Mathf.Clamp01(timeElapse / lineTime);
                float curLength = startLength + lineBias * progress;
                lineIK.oriLength = curLength;

                bool finish = timeElapse >= lineTime;
                if (finish) {
                    updater = null;
                }
                return finish;
            });
        });
    }

    private void GatherBobber(Transform player) {
        isBobberReady = false;
        lineIK.oriLength = 0f;
        if (updater != null) {
            updater.DestroySelf();
            updater = null;
		}

        Rigidbody bobberRb = bobber.GetComponent<Rigidbody>();
        bobberRb.isKinematic = false;
        bobberRb.DOJump(bobberIdle.position, 3f, 1, 1f).OnComplete(() => {
            bobber.SetParent(transform);
            bobberRb.isKinematic = true;

            isFishing = false;
            var locker = player.GetComponent<PCDActLocker>();
            locker.movementLocked = false;
            locker.dropLocked = false;
        });
    }

    public override bool CheckInteractCond(InteractComp interactor) {
        return interactor.holdingItem == gameObject && (!isFishing || isBobberReady);
    }

    public override bool OnInteractStay(InteractComp interactor) {
        if (!isFishing) return true;
        holdTime += Time.deltaTime;
        progress = Mathf.Clamp01(holdTime / (targetHoldTime + 0.0001f));
        if (holdTime >= targetHoldTime) {
            return true;
		}
        return false;
    }

    public override void OnInteractFinish(InteractComp interactor) {
        base.OnInteractFinish(interactor);
        if (!isFishing) {
            OnFishingStart(interactor);
		} else {
            OnFishingSucceed(interactor);
		}
    }

    public override void OnInteractTerminate(InteractComp interactor) {
        base.OnInteractTerminate(interactor);
        OnFishingPause(interactor);
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.U)) {
            isBobberReady = true;
        }
	}
}
