using UnityEngine;

public class PCDHumanHoldAndAttackSM : MonoBehaviour, IPCDActionHandler {

    public enum PickType {
        Pick, Drag
    }

    public enum State {
        Idle, HoldingBox, HoldingSmallBox, HoldingStick, HoldingLongStick,
        DropBox,
        AttackingLongStickHorizon
    }

    public enum AttackState {
        Idle, Charging, Attacking
    }

    public enum FishState {
        Idle, Throwing, Fishing, Catching
    }

    public string playerName = "P1";

    [Header("Holding设置")]
    public PCDWeaponDriver objHoldingDriver;
    public PCDWeaponDriver PCDWeaponAnimtionDriver;
    public Transform boxObjFollowTarget;
    public Transform smallBoxObjFollowTarget;
    public Transform stickObjFollowTarget;
    public Transform longStickObjFollowTarget;
    
    [Space]
    [SerializeField]
    private Transform holdingObject;
    private Transform followTarget;
    [SerializeField]
    private string holdingBoxPoseLayerName = "HoldingBox";
    [SerializeField]
    private string dropBoxPoseLayerName = "HoldingSmallBox";
    [SerializeField]
    private float dropBoxPoseDuration = 0.1f;
    private float dropBoxPoseDurationCount;
    [SerializeField]
    private string holdingSmallBoxPoseLayerName = "HoldingSmallBox";
    [SerializeField]
    private string holdingStickPoseLayerName = "HoldingStick";
    [SerializeField]
    private string holdingLongStickPoseLayerName = "HoldingLongStick";
    [SerializeField]
    private GameObject holdBoxEffect;
    [SerializeField]
    private GameObject holdStickEffect;
    [SerializeField]
    private string actionEventInstancePath;

    [Space]
    [Header("Attacking设置")]
    public Transform testWeapon;
    [SerializeField]
    private Transform attackingWeapon;
    [SerializeField]
    private PCDCircleMovement longStickHorizonCircleMovement;


    [Space]
    [SerializeField]
    private StateMachine<State> sm;
    [SerializeField]
    private SubStateMachine<State, AttackState> attackSM;
    private SubStateMachine<State, FishState> fishSM;
    private PCDHumanMgr PCDHuman;
    private PCDWalkMgr walkMgr => PCDHuman.walkMgr;
    private PCDPoseMgr poseMgr => PCDHuman.poseMgr;
    private PCDArchoringMgr archoringMgr => PCDHuman.arhchoringMgr;
    private PCDAnimator animator => PCDHuman.animator;
    private PCDHumanConfig humanConfig => PCDHuman.humanConfig;

    private FMODUnity.StudioEventEmitter emitter;
    private FMOD.Studio.EventInstance actionEventInstance;

    public bool isholding => holdingObject != null;
    public bool isHoldingBox => sm.curState.Equals(State.HoldingBox);
    public bool isHoldingStick => sm.curState.Equals(State.HoldingStick);
    public bool isHoldingLongStick => sm.curState.Equals(State.HoldingLongStick);
    public bool isAttacking => sm.curState.Equals(State.AttackingLongStickHorizon) || (attackSM.isActiveSM && (attackSM.curState.Equals(AttackState.Charging) || attackSM.curState.Equals(AttackState.Attacking)));

    void Awake() {

        PCDHuman = GetComponentInChildren<PCDHumanMgr>();
        InitHandActionSM();

        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        SafeRun.Run(() => {
            actionEventInstance = FMODUnity.RuntimeManager.CreateInstance(actionEventInstancePath);
        });

        
    }

	private void Start() {
        objHoldingDriver = new PCDWeaponDriver(PCDHuman.skeleton.GetBone("WeaponBone"));
        PCDWeaponAnimtionDriver = new PCDWeaponDriver(PCDHuman.skeleton.GetBone("WeaponBone"));
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        actionManager.RegisterActionHandler(this as IPCDActionHandler);
    }

    public void RegisterActionOnUpdate() {
        var locker = GetComponent<PCDActLocker>();
        if (locker != null && locker.attackLocked)
            return;
        
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        var holdingItem = GetComponent<PCDHumanInteractSM>().holdingItem;
        if (holdingItem == null)
            return;
        
        PCDWeapon weapon = holdingItem.GetComponent<PCDWeapon>();
        if (weapon == null)
            return;

        string interactDesc = "攻击";
        if (weapon.interactDesc != null && weapon.interactDesc != "")
            interactDesc = weapon.interactDesc;
        actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", interactDesc, () => {
            UseWeaponAttack(holdingObject, PCDObjectProperties.Shape.LongStick);
        });
    }

	void InitHandActionSM() {

        sm = new StateMachine<State>(State.Idle);
        attackSM = new SubStateMachine<State, AttackState>(sm, AttackState.Idle);
        fishSM = new SubStateMachine<State, FishState>(sm, FishState.Idle);

        sm.GetState(State.Idle).Bind(
            () => {
                
            },
            () => {
            },
            () => {}
        );

        sm.GetState(State.HoldingBox).Bind(
            () => {
                poseMgr.FadeToKeyFrame(animator.GetAnimReader(holdingBoxPoseLayerName).GetKeyFrameReader("Idle"), false, true, true, false, false);
                poseMgr.poseInfo.bodyVelocity = Vector3.down * 5.0f;
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                ResetPoseAndDriveHandToKF();
            }
        );
        
        sm.GetState(State.DropBox).Bind(
            () => {
                poseMgr.FadeToKeyFrame(animator.GetAnimReader(dropBoxPoseLayerName).GetKeyFrameReader("Idle"), false, true, true, false, false);
                dropBoxPoseDurationCount = 0;
            },
            () => {
                dropBoxPoseDurationCount += Time.deltaTime;
                if (dropBoxPoseDurationCount > dropBoxPoseDuration) {
                    sm.GotoState(State.Idle);
                }
            },
            () => {
                ResetPoseAndDriveHandToKF();
            }
        );

        sm.GetState(State.HoldingSmallBox).Bind(
            () => {
                poseMgr.FadeToKeyFrame(animator.GetAnimReader(holdingSmallBoxPoseLayerName).GetKeyFrameReader("Idle"), false, true, true, false, false);
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                ResetPoseAndDriveHandToKF();
            }
        );

        sm.GetState(State.HoldingStick).Bind(
            () => {
                archoringMgr.BoneArchoringToTransform("RHand", followTarget);
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                archoringMgr.ResetBoneFromArchoring("RHand");
                ResetPoseAndDriveHandToKF();
            }
        );

        sm.GetState(State.HoldingLongStick).Bind(
            () => {
                archoringMgr.BoneArchoringToTransform("LHand", holdingObject.Find("LHandTarget"), 0);
                archoringMgr.BoneArchoringToTransform("RHand", holdingObject.Find("RHandTarget"), 0);
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                archoringMgr.ResetBoneFromArchoring("LHand");
                archoringMgr.ResetBoneFromArchoring("RHand");
                ResetPoseAndDriveHandToKF();
            }
        );

        sm.GetState(State.AttackingLongStickHorizon).Bind(
            () => {
                // controller.LockPosAndRot
                // circle Movement Start
                if (attackingWeapon) {
                    archoringMgr.BoneArchoringToTransform("LHand", attackingWeapon.Find("LHandTarget"), 0);
                    archoringMgr.BoneArchoringToTransform("RHand", attackingWeapon.Find("RHandTarget"), 0);
                }
            },
            () => {
                // if (attackingWeapon) {
                //     attackingWeapon.transform.position = followTarget.position;
                //     attackingWeapon.transform.rotation = followTarget.GetChild(0).rotation;
                // }
            },
            () => {
                // controller.UnLockPosAndRot
                archoringMgr.ResetBoneFromArchoring("LHand");
                archoringMgr.ResetBoneFromArchoring("RHand");
                longStickHorizonCircleMovement.StopMovement();
            }
        );

        attackSM.GetState(AttackState.Charging).Bind(
            () => {
               attackSM.GotoState(AttackState.Attacking);
            },
            () => {
                
            },
            () => {
               
            }
        );

        attackSM.GetState(AttackState.Attacking).Bind(
            () => {
                // controller.LockPosAndRot
                // circle Movement Start
                if (attackingWeapon) {
                    archoringMgr.BoneArchoringToTransform("LHand", attackingWeapon.Find("LHandTarget"), 0);
                    archoringMgr.BoneArchoringToTransform("RHand", attackingWeapon.Find("RHandTarget"), 0);
                }
            },
            () => {
                // if (attackingWeapon) {
                //     attackingWeapon.transform.position = followTarget.position;
                //     attackingWeapon.transform.rotation = followTarget.GetChild(0).rotation;
                // }
            },
            () => {
                // archoringMgr.ResetBoneFromArchoring("LHand");
                // archoringMgr.ResetBoneFromArchoring("RHand");
                // Debug.Log("Stop attacking movment");
                longStickHorizonCircleMovement.StopMovement();
            }
        );

        sm.Init();
        attackSM.Init();
        fishSM.Init();

    }

    public void update() {
        sm.UpdateStateAction();
        attackSM.UpdateStateAction();
        fishSM.UpdateStateAction();
    }

#region PRIVATE IMPLEMENT

    private void HoldBox(Transform box) {
        // if (box == null) {
        if ((isholding && isHoldingBox) || box == null) {
            DropBox();
            OnHoldExit();
            return;
        }
        OnHoldEnter(box, boxObjFollowTarget);
        if (!muteHoldSound) {
            if (holdBoxEffect) {
                GameObject.Instantiate(holdBoxEffect, humanConfig.skeleton.GetBone("LHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("LHand").transform);
                GameObject.Instantiate(holdBoxEffect, humanConfig.skeleton.GetBone("RHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("RHand").transform);
            }
            emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");
        }
        sm.GotoState(State.HoldingBox);
    }

    private void DropBox() {
        if (holdingObject == null) {
            return;
        }

        holdingObject.transform.position = transform.position + transform.forward * 1.5f;
        holdingObject.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        if (holdingObject.GetComponent<Rigidbody>()) {
            holdingObject.GetComponent<Rigidbody>().velocity = Vector3.down * 4.0f;
            holdingObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        
        sm.GotoState(State.DropBox);

        emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
    }

    private void HoldSmallBox(Transform box) {
        // if (box == null) {
        if ((isholding && isHoldingBox) || box == null) {
            DropSmallBox();
            OnHoldExit();
            return;
        }
        OnHoldEnter(box, smallBoxObjFollowTarget);
        if (!muteHoldSound) {
            if (holdBoxEffect) {
                GameObject.Instantiate(holdBoxEffect, humanConfig.skeleton.GetBone("LHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("LHand").transform);
                GameObject.Instantiate(holdBoxEffect, humanConfig.skeleton.GetBone("RHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("RHand").transform);
            }
            emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");
        }
        sm.GotoState(State.HoldingSmallBox);
    }

    private void DropSmallBox() {
        if (holdingObject == null) {
            return;
        }

        holdingObject.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        if (holdingObject.GetComponent<Rigidbody>()) {
            holdingObject.GetComponent<Rigidbody>().velocity = Vector3.down * 4.0f;
            holdingObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        
        sm.GotoState(State.Idle);

        emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
    }
    
    private void HoldStick(Transform stick) {
        if ((isholding && isHoldingStick) || stick == null) {
            DropStick();
            OnHoldExit();
            sm.GotoState(State.Idle);
            return;
        }
        if (!muteHoldSound) {
            if (holdStickEffect) {
                GameObject.Instantiate(holdStickEffect, humanConfig.skeleton.GetBone("RHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("RHand").transform);
            }
            emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");
        }
        OnHoldEnter(stick, stickObjFollowTarget);
        sm.GotoState(State.HoldingStick);
    }

    private void DropStick() {
        if (holdingObject == null) {
            return;
        }
        if (holdingObject.GetComponent<Rigidbody>()) {
            holdingObject.GetComponent<Rigidbody>().velocity = Vector3.down * 4.0f;
            holdingObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
    }

    private void HoldLongStick(Transform longStick) {
        if ((isholding && isHoldingLongStick) || longStick == null) {
            DropLongStick();
            OnHoldExit();
            sm.GotoState(State.Idle);
            return;
        }
        if (!muteHoldSound) {
            if (holdStickEffect) {
                GameObject.Instantiate(holdStickEffect, humanConfig.skeleton.GetBone("LHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("LHand").transform);
                GameObject.Instantiate(holdStickEffect, humanConfig.skeleton.GetBone("RHand").transform.position, Quaternion.identity, humanConfig.skeleton.GetBone("RHand").transform);
            }
            emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");
        }
        OnHoldEnter(longStick, longStickObjFollowTarget);
        sm.GotoState(State.HoldingLongStick);
    }

    private void DropLongStick() {
        if (holdingObject == null) {
            return;
        }
        if (holdingObject.GetComponent<Rigidbody>()) {
            holdingObject.GetComponent<Rigidbody>().velocity = Vector3.down * 4.0f;
            holdingObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
    }
    
    private void UseLongStickHorizonAttack(Transform weapon) {

        PCDWeapon PCDWeapon = weapon.GetComponent<PCDWeapon>();

        if (!PCDWeapon || !PCDWeapon.attackAnimPrefab) {
            return;
        }

        longStickHorizonCircleMovement = GameObject.Instantiate(PCDWeapon.attackAnimPrefab, humanConfig.skeleton.GetBone("Body").transform.position, humanConfig.skeleton.GetBone("Body").transform.rotation, humanConfig.skeleton.GetBone("Body").transform).GetComponentInChildren<PCDCircleMovement>();

        if (!longStickHorizonCircleMovement) {
            return;
        }

        attackingWeapon = weapon;
        Transform lastFollowTarget = followTarget; 
        followTarget = longStickHorizonCircleMovement.movingObj.GetChild(0);

        PCDWeaponAnimtionDriver.weaponBoneTarget = followTarget;
        PCDWeaponAnimtionDriver.TryGetOwnership();

        longStickHorizonCircleMovement.StartMovement(transform, transform, () => {
            // sm.GotoState(State.Idle);
            attackSM.ReturnToParentSM();
            followTarget = lastFollowTarget;
            PCDWeaponAnimtionDriver.ReturnOwnership();
            // HoldObj(attackingWeapon, PCDObjectProperties.Shape.LongStick, true);
        });

        sm.GotoSubSM<AttackState>(attackSM, AttackState.Charging);
        // sm.GotoState(State.AttackingLongStickHorizon);

    }

#endregion

#region PUBLIC INTERFACE
    private bool muteHoldSound;
    public void HoldObj(Transform obj, PCDObjectProperties.Shape shape = PCDObjectProperties.Shape.Box, bool muteHoldSound = false) {
        if (isAttacking) {
            return;
        }
        this.muteHoldSound = muteHoldSound;
        if (shape == PCDObjectProperties.Shape.Box) {
            HoldBox(obj);
        } else if (shape == PCDObjectProperties.Shape.Stick) {
            HoldStick(obj);
        } else if (shape == PCDObjectProperties.Shape.LongStick) {
            HoldLongStick(obj);
        } else if (shape == PCDObjectProperties.Shape.SmallBox) {
            HoldSmallBox(obj);
        }
    }

    /// <summary>
    /// 使用武器进行攻击
    /// </summary>
    /// <param name="weapon">武器物体</param>
    /// <param name="shape">武器形状</param>
    public void UseWeaponAttack(Transform weapon, PCDObjectProperties.Shape shape) {
        if (isAttacking) {
            return;
        }
        if (shape == PCDObjectProperties.Shape.LongStick && isHoldingLongStick) {
            UseLongStickHorizonAttack(weapon);
        }
    }

    public void TriggerAttackingWeaponStartEvent() {
        attackingWeapon.GetComponent<PCDWeapon>().OnAttackStart(transform);
    }

    public void TriggerAttackingWeaponEndEvent() {
        attackingWeapon.GetComponent<PCDWeapon>().OnAttackEnd(transform);
    }

#endregion

#region EVENT

    private void OnHoldEnter(Transform obj, Transform objFollowTarget) {
        holdingObject = obj;
        followTarget = objFollowTarget;

        // 用Driver来接管物体的跟随
        holdingObject.SetParent(objHoldingDriver.attachedBone.transform);
        holdingObject.localPosition = Vector3.zero;
        holdingObject.localRotation = Quaternion.identity;
        objHoldingDriver.weaponBoneTarget = followTarget;
        objHoldingDriver.TryGetOwnership();
    }

    private void OnHoldUpdate() {
        
        // UpdateHoldingObjFollow();

        void UpdateHoldingObjFollow() {
            holdingObject.transform.position = followTarget.transform.position;
            holdingObject.transform.rotation = followTarget.transform.rotation;
        }
        
    }

    private void OnHoldExit() {
        objHoldingDriver.ReturnOwnership();
        holdingObject.SetParent(null);

        holdingObject = null;
        followTarget = null;
    }

#endregion

    private void ResetPoseAndDriveHandToKF() {
        poseMgr.ResetPose();
        walkMgr.DriveHandToKF();
        // human.SetBodyPoseLayerOverrideIndex(-1);
        // human.SetLHandPoseLayerOverrideIndex(-1);
        // human.SetRHandPoseLayerOverrideIndex(-1);
        // human.DoHandPose();
    }

}
