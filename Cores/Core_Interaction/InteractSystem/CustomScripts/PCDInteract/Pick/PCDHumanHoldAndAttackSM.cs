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

    public string playerName = "P1";

    [Header("Holding设置")]
    public Transform boxObjFollowTarget;
    public Transform smallBoxObjFollowTarget;
    public Transform stickObjFollowTarget;
    public Transform longStickObjFollowTarget;
    
    [Space]
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
    private Transform attackingWeapon;
    [SerializeField]
    private PCDCircleMovement longStickHorizonCircleMovement;


    [Space]
    [SerializeField]
    private StateMachine<State> sm;
    private PCDHuman human;

    private FMODUnity.StudioEventEmitter emitter;
    private FMOD.Studio.EventInstance actionEventInstance;

    public bool isholding => holdingObject != null;
    public bool isHoldingBox => sm.curState.Equals(State.HoldingBox);
    public bool isHoldingStick => sm.curState.Equals(State.HoldingStick);
    public bool isHoldingLongStick => sm.curState.Equals(State.HoldingLongStick);
    public bool isAttacking => sm.curState.Equals(State.AttackingLongStickHorizon);

    void Awake() {

        human = GetComponentInChildren<PCDHuman>();
        InitHandActionSM();

        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        SafeRun.Run(() => {
            actionEventInstance = FMODUnity.RuntimeManager.CreateInstance(actionEventInstancePath);
        });

        
    }

	private void Start() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        actionManager.RegisterActionHandler(this as IPCDActionHandler);
    }

    public void RegisterActionOnUpdate() {
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

        sm.GetState(State.Idle).Bind(
            () => {
                
            },
            () => {
            },
            () => {}
        );

        sm.GetState(State.HoldingBox).Bind(
            () => {
                human.SetBodyPoseLayerOverrideName(holdingBoxPoseLayerName);
                human.SetLHandPoseLayerOverrideName(holdingBoxPoseLayerName);
                human.SetRHandPoseLayerOverrideName(holdingBoxPoseLayerName);
                human.DoHandPose(0);
                human.poseInfo.bodyVelocity = Vector3.down * 5.0f;
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                ClearPoseLayerOverride();
            }
        );
        
        sm.GetState(State.DropBox).Bind(
            () => {
                human.SetBodyPoseLayerOverrideName(dropBoxPoseLayerName);
                human.SetLHandPoseLayerOverrideName(dropBoxPoseLayerName);
                human.SetRHandPoseLayerOverrideName(dropBoxPoseLayerName);
                human.DoHandPose(0);
                dropBoxPoseDurationCount = 0;
            },
            () => {
                dropBoxPoseDurationCount += Time.deltaTime;
                if (dropBoxPoseDurationCount > dropBoxPoseDuration) {
                    sm.GotoState(State.Idle);
                }
            },
            () => {
                ClearPoseLayerOverride();
            }
        );

        sm.GetState(State.HoldingSmallBox).Bind(
            () => {
                human.SetBodyPoseLayerOverrideName(holdingSmallBoxPoseLayerName);
                human.SetLHandPoseLayerOverrideName(holdingSmallBoxPoseLayerName);
                human.SetRHandPoseLayerOverrideName(holdingSmallBoxPoseLayerName);
                human.DoHandPose(0);
                // human.poseInfo.bodyVelocity = Vector3.down * 5.0f;
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                ClearPoseLayerOverride();
            }
        );

        sm.GetState(State.HoldingStick).Bind(
            () => {
                human.SetRHandArchoring(followTarget);
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                human.SetRHandArchoring(null);
                ClearPoseLayerOverride();
            }
        );

        sm.GetState(State.HoldingLongStick).Bind(
            () => {
                human.SetLHandArchoring(holdingObject.Find("LHandTarget"));
                human.SetRHandArchoring(holdingObject.Find("RHandTarget"));
                // human.StartUpdateHandPose(0.05f);
            },
            () => {
                OnHoldUpdate();
            },
            () => {
                ClearPoseLayerOverride();
                human.SetLHandArchoring(null);
                human.SetRHandArchoring(null);
            }
        );

        sm.GetState(State.AttackingLongStickHorizon).Bind(
            () => {
                // controller.LockPosAndRot
                // circle Movement Start
                if (attackingWeapon) {
                    human.SetLHandArchoring(attackingWeapon.Find("LHandTarget"));
                    human.SetRHandArchoring(attackingWeapon.Find("RHandTarget"));
                    // human.SetLookAt(attackingWeapon, attackingWeapon);
                    // longStickHorizonCircleMovement.AddProcessEvent(0.45f, () => {attackingWeapon.GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(true);});
                    // longStickHorizonCircleMovement.AddProcessEvent(0.55f, () => {attackingWeapon.GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(false);});
                }
            },
            () => {
                if (attackingWeapon) {
                    attackingWeapon.transform.position = followTarget.position;
                    attackingWeapon.transform.rotation = followTarget.GetChild(0).rotation;
                }
            },
            () => {
                // controller.UnLockPosAndRot
                human.SetLHandArchoring(null);
                human.SetRHandArchoring(null);
                // human.SetLookAt(null, null);
                // longStickHorizonCircleMovement.ClearAllProcessEvent();
                longStickHorizonCircleMovement.StopMovement();
            }
        );

        sm.Init();

    }

    public void update() {
        sm.UpdateStateAction();
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
                GameObject.Instantiate(holdBoxEffect, human.humanBone.lHand.position, Quaternion.identity, human.humanBone.lHand);
                GameObject.Instantiate(holdBoxEffect, human.humanBone.rHand.position, Quaternion.identity, human.humanBone.rHand);
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
                GameObject.Instantiate(holdBoxEffect, human.humanBone.lHand.position, Quaternion.identity, human.humanBone.lHand);
                GameObject.Instantiate(holdBoxEffect, human.humanBone.rHand.position, Quaternion.identity, human.humanBone.rHand);
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
                GameObject.Instantiate(holdStickEffect, human.humanBone.rHand.position, Quaternion.identity, human.humanBone.rHand);
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
                GameObject.Instantiate(holdStickEffect, human.humanBone.lHand.position, Quaternion.identity, human.humanBone.lHand);
                GameObject.Instantiate(holdStickEffect, human.humanBone.rHand.position, Quaternion.identity, human.humanBone.rHand);
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

        longStickHorizonCircleMovement = GameObject.Instantiate(PCDWeapon.attackAnimPrefab, human.humanBone.body.position, human.humanBone.body.rotation, human.humanBone.body).GetComponentInChildren<PCDCircleMovement>();

        if (!longStickHorizonCircleMovement) {
            return;
        }

        attackingWeapon = weapon;
        followTarget = longStickHorizonCircleMovement.movingObj;
        longStickHorizonCircleMovement.StartMovement(transform, transform, () => {
            sm.GotoState(State.Idle);
            HoldObj(attackingWeapon, PCDObjectProperties.Shape.LongStick, true);
        });

        sm.GotoState(State.AttackingLongStickHorizon);

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
    }

    private void OnHoldUpdate() {
        
        UpdateHoldingObjFollow();

        void UpdateHoldingObjFollow() {
            holdingObject.transform.position = followTarget.transform.position;
            holdingObject.transform.rotation = followTarget.transform.rotation;
        }
        
    }

    private void OnHoldExit() {
        holdingObject = null;
        followTarget = null;
    }

#endregion

    private void ClearPoseLayerOverride() {
        human.SetBodyPoseLayerOverrideIndex(-1);
        human.SetLHandPoseLayerOverrideIndex(-1);
        human.SetRHandPoseLayerOverrideIndex(-1);
        human.DoHandPose();
    }

}
