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
    public float bobberSwingTime = 1.0f;
    public float bobberGatherTime = 0.5f;
    public float bobberJumpHeight = 3.0f;
    public AnimationCurve bobberJumpCurve;
    public float bobberRange = 4f;
    public FishBobber fishBobber;
    public FishShadow curFishShadow; 
    public Transform bobberIdle;
    public Transform bobberStart;
    public Transform bobberTarget;
    public Transform tapeStartPoint;
    public PCDIK lineIK;
    public float rodCurvingTime = 1.0f;
    public float draggingFadeTime = 0.5f;
    public PCDHumanMgr PCDHuman;

    public GameObject smallSplashEffectPrefab;

    private PCDBoneDriver fakeBodyDriver;
    private PCDWeaponDriver weaponAnimationDriver;
    private PCDTransfromLerp rodCurveLerp;

    private float holdTime = 0f;
    private Transform player;
    private bool isDragging;
    private float oriTapeLength;
    private Transform bobber;

    void Awake() {
        bobber = fishBobber.transform;
        fishBobber.fishRod = this;
        rodCurveLerp = GetComponentInChildren<PCDTransfromLerp>();
        oriTapeLength = lineIK.oriLength;
    }

    private void OnFishingStart(InteractComp interactor) {
        isFishing = true;
        isDragging = false;
        holdTime = 0f;


        var locker = interactor.GetComponent<PCDActLocker>();
        locker.movementLocked = true;
        locker.dropLocked = true;

        // Animation
        // 获取 Body 的控制权，什么都不做，让 Animation 来控制 Body
        PCDHuman = interactor.GetComponentInChildren<PCDHumanMgr>();
        // 创建一个新的 Driver 来接管 actor 的 Body 的动画
        fakeBodyDriver = new PCDBoneDriver(PCDHuman.skeleton.GetBone("Body"));
        fakeBodyDriver.TryGetOwnership();
        // 将 WeaponDriver 的 WeaponBoneTarget 设置成 actor 身上的 AnimWeaponBone
        weaponAnimationDriver = new PCDWeaponDriver(PCDHuman.skeleton.GetBone("WeaponBone"), PCDHuman.skeleton.GetBone("WeaponAnimationBone").transform);
        weaponAnimationDriver.TryGetOwnership();
        PCDHuman.uanimator.enabled = true;
        PCDHuman.uanimator.CrossFadeInFixedTime("Fishing-Start", 0.1f);

        bobber.position = bobberStart.position;

        player = interactor.transform;
        // SwingBobber(interactor.transform);

        interactType = "收竿";
    }

    private void OnFishingSucceed(InteractComp interactor) {

        PCDHuman.uanimator.CrossFadeInFixedTime("Fishing-End", 0.16f);

        // SendMessage("PlayEvent", new string[2] {"FishingAction", "BobberOutOfWater"}, SendMessageOptions.DontRequireReceiver);

        // 动作 & 浮标回到竿上
        player = interactor.transform;
        GatherBobber();

        if (targetHoldTime <= 0) {
            if (smallSplashEffectPrefab) {
                GameObject.Instantiate(smallSplashEffectPrefab, bobber.position, Quaternion.identity);
            }
        }

        if (curFishShadow != null) {
            curFishShadow.sm.GotoState(FishShadow.State.Finish);
            curFishShadow = null;
        } else if (fishBobber.probeFish != null) {
            fishBobber.probeFish.sm.GotoState(FishShadow.State.Idle);
            fishBobber.probeFish = null;
        }

        interactType = "甩竿";
    }


    private FuncUpdater tapeCurvingUpdater = null;
    private FuncUpdater rodCurvingUpdater = null;
    /// <summary>
    /// 甩出浮漂
    /// </summary>
    /// <param name="player"></param>
    public void SwingBobber() {
        Rigidbody bobberRb = bobber.GetComponent<Rigidbody>();
        Vector3 targetPos = player.position + player.rotation * bobberTarget.localPosition;
        bobberRb.DOMove(bobberStart.position, 0f);
        bobber.SetParent(null);

        SendMessage("PlayEvent", new string[2] {"FishingAction", "SwingRod"}, SendMessageOptions.DontRequireReceiver);

        
        bobberRb.isKinematic = false;
        bobber.GetComponent<Collider>().enabled = true;
        bobberRb.DOJump(targetPos.CopySetY(fishBobber.ground.position.y), bobberJumpHeight, 1, bobberSwingTime).SetEase(bobberJumpCurve);

        EasyEvent.RegisterOnceCallback("BobberHit", () => {
            isBobberReady = true;
            targetHoldTime = 0f;

            float timeElapse = 0f;
            float startLength = (bobber.position - tapeStartPoint.position).magnitude;
            tapeCurvingUpdater = FuncUpdater.Create(() => {
                timeElapse += Time.deltaTime;
                float progress = Mathf.Clamp01(timeElapse / lineTime);
                float curLength = startLength + lineBias * progress;
                lineIK.oriLength = curLength;

                bool finish = timeElapse >= lineTime;
                if (finish) {
                    tapeCurvingUpdater = null;
                }
                return finish;
            });
        });
    }

    /// <summary>
    /// 收回浮漂
    /// </summary>
    /// <param name="player"></param>
    public void GatherBobber() {
        isBobberReady = false;
        // lineIK.oriLength = 0f;
        lineIK.oriLength = oriTapeLength;
        if (tapeCurvingUpdater != null) {
            tapeCurvingUpdater.DestroySelf();
            tapeCurvingUpdater = null;
		}

        RodCurving(0.0f, 0.16f);

        bobber.DOKill();
        Rigidbody bobberRb = bobber.GetComponent<Rigidbody>();
        bobberRb.isKinematic = false;
        bobberRb.DOJump(bobberIdle.position, bobberJumpHeight, 1, bobberGatherTime).SetEase(bobberJumpCurve).OnComplete(() => {
            bobber.SetParent(transform);
            bobberRb.isKinematic = true;

            isFishing = false;
            var locker = player.GetComponent<PCDActLocker>();
            locker.movementLocked = false;
            locker.dropLocked = false;

            // 归还 Body 的控制权
            fakeBodyDriver.ReturnOwnership();
            weaponAnimationDriver.ReturnOwnership();
            PCDHuman.uanimator.enabled = false;

            bobber.position = bobberIdle.position;

        });
    }

    public override bool CheckInteractCond(InteractComp interactor) {
        return interactor.holdingItem == gameObject && (!isFishing || isBobberReady);
    }

    public override bool OnInteractStay(InteractComp interactor) {
        if (!isFishing) return true;
        
        if (!isDragging) {
            PCDHuman.uanimator.CrossFadeInFixedTime("Fishing-Dragging", draggingFadeTime);
            isDragging = true;
            
            if (targetHoldTime > 0) {
                SendMessage("PlayEvent", new string[2] {"FishingAction", "Dragging"}, SendMessageOptions.DontRequireReceiver);
            }

            RodCurving(1.0f, rodCurvingTime);
        }

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
    
    private void OnFishingPause(InteractComp interactor) {
        isDragging = false;
        // 回到 Fishing-Idle 动画
        PCDHuman.uanimator.CrossFadeInFixedTime("Fishing-Idle", draggingFadeTime * 0.65f);
        RodCurving(0.0f, draggingFadeTime * 0.65f);

        SendMessage("Stop", SendMessageOptions.DontRequireReceiver);
        
	}

    private void RodCurving(float curvingTTarget, float curvingTime) {
        float timeElapse = 0;
        float lastT = rodCurveLerp.t;
        tapeCurvingUpdater?.DestroySelf();
        tapeCurvingUpdater = FuncUpdater.Create(() => {
            timeElapse += Time.deltaTime;
            float progress = Mathf.Clamp01(timeElapse / curvingTime);

            rodCurveLerp.t = Mathf.Lerp(lastT, curvingTTarget, progress);    
            bool finish = timeElapse >= curvingTime;
            if (finish) {
                tapeCurvingUpdater = null;
            }
            return finish;
        });
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.U)) {
            isBobberReady = true;
        }
	}
}
