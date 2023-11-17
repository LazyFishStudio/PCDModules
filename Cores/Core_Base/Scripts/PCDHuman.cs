using UnityEngine;
using UnityEngine.Events;

public class PCDHuman : MonoBehaviour {

    public enum BodyState {
        Pose, Stepping, ArchoringAtTarget
    }

    public enum FootState {
        Pose, Idle, Wait, Standing, Stepping
    }

    public enum HandState {
        Pose
    }

    private float scaleDeltaTime => Time.deltaTime / humanBone.rootScale * Mathf.Max(poseInfo.speed / animSetting.oriSpeed, 0.5f);

#region Setting
    public bool isLateUpdate = false;
    public bool autoGenerateHumanBone;

    public PCDHumanBoneSetting humanBone;
    
    public AnimSetting animSetting;

#endregion

    [Space]

    public bool isStepLeft;
    private float stepIntervalCount;

    [Space]

#region CoreVariable

    public PoseInfo poseInfo;
    [SerializeField]
    private Foot lFoot;
    [SerializeField]
    private Foot rFoot;
    [SerializeField]
    private Hand lHand;
    [SerializeField]
    private Hand rHand;
    [SerializeField]
    private StateMachine<BodyState> bodysm = new StateMachine<BodyState>(BodyState.Pose);
    [SerializeField]
    private StateMachine<FootState> footsm = new StateMachine<FootState>(FootState.Idle);
    [SerializeField]
    private StateMachine<HandState> handsm = new StateMachine<HandState>(HandState.Pose);

#endregion

#region PoseLayerVariable

    [SerializeField]
    private int curPoseLayerIndex;
    [SerializeField]
    private int bodyPoseLayerIndexOverride;
    [SerializeField]
    private int lHandPoseLayerIndexOverride;
    [SerializeField]
    private int rHandPoseLayerIndexOverride;
    [SerializeField]
    private int lFootPoseLayerIndexOverride;
    [SerializeField]
    private int rFootPoseLayerIndexOverride;
    [SerializeField]
    private HumanPoseLayer curPoseLayer => GetPoseLayer(curPoseLayerIndex);
    [SerializeField]
    private int curPoseIndex;
    [SerializeField]
    private HumanPose curPose;

#endregion
    
    void Awake() {

        drawGizmos = true;

        humanBone.Init();
        InitPoseInfoAndResetBonePosToBoneSetting();

        InitBodySM();
        InitFootSM();
        InitHandSM();

    }

    void Update() {

        if (!isLateUpdate) {
            UpdateVelocityInfo();

            footsm.UpdateStateAction();
            bodysm.UpdateStateAction();
            handsm.UpdateStateAction();

            UpdateBody();
            UpdateLegs();
            UpdateArms();

            
        }

    }

    void LateUpdate() {
        if (isLateUpdate) {
            UpdateVelocityInfo();

            footsm.UpdateStateAction();
            bodysm.UpdateStateAction();
            handsm.UpdateStateAction();

            UpdateBody();
            UpdateLegs();
            UpdateArms();
        }
    }

    void OnValidate() {
        if (autoGenerateHumanBone) {

            humanBone.root = transform;

            humanBone.body = transform.Find("Body");
            humanBone.head = transform.Find("Body").Find("Head");

            humanBone.lShoulder = transform.Find("Arm_L").GetChild(0);
            humanBone.lHand = transform.Find("Arm_L").GetChild(1);

            humanBone.rShoulder = transform.Find("Arm_R").GetChild(0);
            humanBone.rHand = transform.Find("Arm_R").GetChild(1);

            humanBone.lPelvis = transform.Find("Leg_L").GetChild(0);
            humanBone.lFoot = transform.Find("Leg_L").GetChild(1);

            humanBone.rPelvis = transform.Find("Leg_R").GetChild(0);
            humanBone.rFoot = transform.Find("Leg_R").GetChild(1);

            humanBone.poseLayerParent = transform.Find("HumanPoseLayers");

            autoGenerateHumanBone = false;
        }
    }

    private bool isAnyFootOutRange => lFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale || rFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale;
    private bool isAnyFootNotReset => isAnyFootOutRange || Mathf.Abs(humanBone.lFoot.position.y - humanBone.root.position.y) > 0.01f || Mathf.Abs(humanBone.rFoot.position.y - humanBone.root.position.y) > 0.01f;

#region Private StateMachine Init

    private void InitBodySM() {

        bodysm.GetState(BodyState.Pose).Bind(
            () => {
            },
            () => {
                poseInfo.bodyTargetPosLocal = curPose.body.localPosition;
            },
            () => {}
        );

        bodysm.GetState(BodyState.Stepping).Bind(
            () => {
            },
            () => {

                Foot activeFoot = isStepLeft ? lFoot : rFoot;
                poseInfo.bodyTargetPosLocal = curPose.body.localPosition + Vector3.up * animSetting.bodyHeightCurve.Evaluate(activeFoot.GetProcess());
               
            },
            () => {}
        );

        bodysm.Init();

    }

    private void InitFootSM() {

        footsm.GetState(FootState.Pose).Bind(
            () => {
                
            },
            () => {

            },
            () => {}
        );

        footsm.GetState(FootState.Idle).Bind(
            () => {
                SetPose(0);
                StartUpdateHandPose(lHand.poseDuration);
                bodysm.GotoState(BodyState.Pose);
            },
            () => {

                if (poseInfo.speed > 0.01f || isAnyFootNotReset) {
                    footsm.GotoState(FootState.Wait);
                }

            },
            () => {}
        );

        footsm.GetState(FootState.Wait).Bind(
            () => {
                
                stepIntervalCount = 0;

                if (poseInfo.speed < animSetting.stepTriggerSpeed) {
                    if (isAnyFootNotReset) {
                        footsm.GotoState(FootState.Standing);
                    } else {
                        footsm.GotoState(FootState.Idle);
                    }
                }

            },
            () => {

                stepIntervalCount += scaleDeltaTime;

                if (poseInfo.turnAngle > 120.0f) {
                    stepIntervalCount = 0;
                }

                if (poseInfo.speed < animSetting.stepTriggerSpeed) {
                    if (isAnyFootOutRange) {
                        footsm.GotoState(FootState.Standing);
                    } else {
                        footsm.GotoState(FootState.Idle);
                    }
                } else {
                    if (stepIntervalCount > animSetting.stepInterval)
                        footsm.GotoState(FootState.Stepping);
                }

            },
            () => {}
        );

        footsm.GetState(FootState.Standing).Bind(
            () => {
                float lFootToTargetDis = Vector3.Distance(poseInfo.lFootTargetPos, lFoot.curPos);
                float rFootToTargetDis = Vector3.Distance(poseInfo.rFootTargetPos, rFoot.curPos);

                isStepLeft = lFootToTargetDis > rFootToTargetDis;

                if (isStepLeft) {
                    SetPose(0);
                    lFoot.Step(() => {
                        footsm.GotoState(FootState.Wait);
                        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
                    });
                    StartUpdateHandPose(lHand.poseDuration);
                    bodysm.GotoState(BodyState.Stepping);
                } else {
                    SetPose(0);
                    rFoot.Step(() => {
                        footsm.GotoState(FootState.Wait);
                        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
                    });
                    StartUpdateHandPose(rHand.poseDuration);
                    bodysm.GotoState(BodyState.Stepping);
                }
            },
            () => {

            },
            () => {}
        );

        footsm.GetState(FootState.Stepping).Bind(
            () => {
                float lFootToTargetDis = Vector3.Distance(poseInfo.lFootTargetPos, lFoot.curPos);
                float rFootToTargetDis = Vector3.Distance(poseInfo.rFootTargetPos, rFoot.curPos);
                
                if (Mathf.Abs(lFootToTargetDis - rFootToTargetDis) < animSetting.stepTargetOffset / 2.0f) {
                    isStepLeft = !animSetting.stepRightFootFirst;
                } else {
                    isStepLeft = lFootToTargetDis > rFootToTargetDis;
                }

                if (isStepLeft) {
                    SetPose(1);
                    lFoot.Step(() => {
                        footsm.GotoState(FootState.Wait);
                        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
                    });
                    StartUpdateHandPose(lHand.poseDuration);
                    bodysm.GotoState(BodyState.Stepping);
                } else {
                    SetPose(2);
                    rFoot.Step(() => {
                        footsm.GotoState(FootState.Wait);
                        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
                    });
                    StartUpdateHandPose(rHand.poseDuration);
                    bodysm.GotoState(BodyState.Stepping);
                }
            },
            () => {

            },
            () => {}
        );

        footsm.Init();
        
    }

    private void InitHandSM() {

        handsm.GetState(HandState.Pose).Bind(
            () => {

                if (humanBone.isLArmActive) LHandDoPose();
                
                if (humanBone.isRArmActive) RHandDoPose();

                void LHandDoPose() {
                    lHand.DoPose();
                }

                void RHandDoPose() {
                    rHand.DoPose();
                }

            },
            () => {

            },
            () => {}
        );

        handsm.Init();

    }

#endregion

#region Private Animation Core

private void UpdateLegs() {

        if (humanBone.isLLegActive) UpdateLLeg();
        if (humanBone.isRLegActive) UpdateRLeg();

        void UpdateLLeg() {

            poseInfo.lPelvisPosLocal = humanBone.body.localPosition + humanBone.body.localRotation * humanBone.lPelvisOriPosLocal;

            Vector3 lPelvisPosScale = humanBone.root.position + humanBone.root.rotation * poseInfo.lPelvisPosLocal * humanBone.rootScale;
            poseInfo.lFootTargetPos = curPose.lFoot.position.CopySetY(humanBone.root.position.y);
            if (footsm.curState.Equals(FootState.Stepping) || footsm.curState.Equals(FootState.Wait)) {
                poseInfo.lFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * humanBone.rootScale;
                // Debug.Log(poseInfo.moveDir == Vector3.zero);
            }

            lFoot.Update(poseInfo.lPelvisPosLocal, poseInfo.lFootTargetPos, humanBone.body.rotation * curPose.lFoot.localRotation);

            humanBone.lPelvis.localPosition = poseInfo.lPelvisPosLocal;
            humanBone.lPelvis.rotation = Quaternion.LookRotation((humanBone.lFoot.position - humanBone.lPelvis.position).normalized, humanBone.lFoot.up);
        
        }

        void UpdateRLeg() {

            poseInfo.rPelvisPosLocal = humanBone.body.localPosition + humanBone.body.localRotation * humanBone.rPelvisOriPosLocal;

            Vector3 rPelvisPosScale = humanBone.root.position + humanBone.root.rotation * poseInfo.rPelvisPosLocal * humanBone.rootScale;
            poseInfo.rFootTargetPos = curPose.rFoot.position.CopySetY(humanBone.root.position.y);
            if (footsm.curState.Equals(FootState.Stepping) || footsm.curState.Equals(FootState.Wait)) {
                poseInfo.rFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * humanBone.rootScale;
            }

            rFoot.Update(poseInfo.rPelvisPosLocal, poseInfo.rFootTargetPos, humanBone.body.rotation * curPose.rFoot.localRotation);

            humanBone.rPelvis.localPosition = poseInfo.rPelvisPosLocal;
            humanBone.rPelvis.rotation = Quaternion.LookRotation((humanBone.rFoot.position - humanBone.rPelvis.position).normalized, humanBone.rFoot.up);

        }

    }

    private void UpdateArms() {

        if (humanBone.isLArmActive) {
            poseInfo.lShoulderPosLocal = humanBone.body.localPosition + humanBone.body.localRotation * humanBone.lShoulderOriPosLocal;
            humanBone.lShoulder.localPosition = poseInfo.lShoulderPosLocal;
        }
        if (humanBone.isRArmActive) {
            poseInfo.rShoulderPosLocal = humanBone.body.localPosition + humanBone.body.localRotation * humanBone.rShoulderOriPosLocal;
            humanBone.rShoulder.localPosition = poseInfo.rShoulderPosLocal;
        }

        if (humanBone.isLArmActive) {
            poseInfo.lHandTargetPosLocalRefToBody = curPose.body.localRotation * curPose.lHand.localPosition - curPose.body.localPosition;
            poseInfo.lHandTargetRotLocalRefToBody = curPose.lHand.localRotation;
            lHand.Update(poseInfo.lShoulderPosLocal, poseInfo.lHandTargetPosLocalRefToBody, poseInfo.lHandTargetRotLocalRefToBody);
            humanBone.lShoulder.rotation = Quaternion.LookRotation((humanBone.lHand.position - humanBone.lShoulder.position).normalized, humanBone.lHand.up);
        }

        if (humanBone.isRArmActive) {
            poseInfo.rHandTargetPosLocalRefToBody = curPose.body.localRotation * curPose.rHand.localPosition - curPose.body.localPosition;
            poseInfo.rHandTargetRotLocalRefToBody = curPose.rHand.localRotation;
            rHand.Update(poseInfo.rShoulderPosLocal, poseInfo.rHandTargetPosLocalRefToBody, poseInfo.rHandTargetRotLocalRefToBody);
            humanBone.rShoulder.rotation = Quaternion.LookRotation((humanBone.rHand.position - humanBone.rShoulder.position).normalized, humanBone.rHand.up);
        }
        
    }

    [SerializeField]
    private Transform lookAtTarget_body;
    [SerializeField]
    private Transform lookAtTarget_head;
    private void UpdateBody() {
        
        UpdateBodyOffset();
        UpdateBodyRotToPose();
        UpdateBodyRoll();
        UpdateBodySprint();

        if (lookAtTarget_body == null && lookAtTarget_head == null) {
            if (humanBone.head) {
                Vector3 toTargetWeight = Vector3.Lerp(humanBone.root.forward, humanBone.body.forward, animSetting.lookAtWeight_head);
                humanBone.head.transform.rotation = Quaternion.Slerp(humanBone.head.transform.rotation, Quaternion.LookRotation(toTargetWeight, humanBone.body.up), Time.deltaTime * animSetting.bodyRotSpeed);
            }
        } else {

            if (lookAtTarget_body) {
                Vector3 toLookAtTargetLocal = Quaternion.Inverse(humanBone.root.rotation) * (lookAtTarget_body.position - humanBone.root.position).ClearY();
                poseInfo.bodyTargetRotLocal = Quaternion.Lerp(poseInfo.bodyTargetRotLocal, Quaternion.LookRotation(toLookAtTargetLocal, Vector3.up), animSetting.lookAtWeight_body);
            } 

            if (lookAtTarget_head) {
                if (humanBone.head) {
                    Vector3 toTargetWeight = Vector3.Lerp(humanBone.body.forward, lookAtTarget_head.position - humanBone.head.position, animSetting.lookAtWeight_head);
                    humanBone.head.transform.rotation = Quaternion.Slerp(humanBone.head.transform.rotation, Quaternion.LookRotation(toTargetWeight, humanBone.body.up), Time.deltaTime * animSetting.bodyRotSpeed);
                }
            }
        }

        humanBone.body.localPosition = poseInfo.bodyPosLocal + poseInfo.bodyOffsetLocal;
        humanBone.body.localRotation = Quaternion.Slerp(humanBone.body.localRotation, poseInfo.bodyTargetRotLocal, Time.deltaTime * animSetting.bodyRotSpeed);


        void UpdateBodySprint() {
            float toTargetY = poseInfo.bodyTargetPosLocal.y - poseInfo.bodyPosLocal.y;
            float dragForceY = toTargetY * animSetting.bodyDragStrength;
            float dampForceY = -poseInfo.bodyVelocity.y * animSetting.bodyVelocityDamp;
            dragForceY += dampForceY;
            // poseInfo.bodyVelocity += Vector3.up * dragForceY * Time.deltaTime;
            poseInfo.bodyVelocity += Vector3.up * dragForceY * Mathf.Min(Time.deltaTime, 0.0125f);
            poseInfo.bodyVelocity = poseInfo.bodyVelocity.normalized * Mathf.Min(30.0f, poseInfo.bodyVelocity.magnitude);
            poseInfo.bodyPosLocal += poseInfo.bodyVelocity * Mathf.Min(Time.deltaTime, 0.0125f);
        }

        void UpdateBodyOffset() {
            Vector3 bodyTargetOffsetLocal = curPose.body.localPosition.ClearY();
            poseInfo.bodyOffsetLocal = Vector3.Lerp(poseInfo.bodyOffsetLocal, bodyTargetOffsetLocal, Time.deltaTime * animSetting.bodyOffsetSpeed);
        }

        void UpdateBodyRotToPose() {
            poseInfo.bodyTargetRotLocal = curPose.body.localRotation;
        }

        void UpdateBodyRoll() {
            // 待解决问题：检测出RootForward比MoveDir先到达目标角度的情况（会导致Roll角度相反），目前可以通过控制MoveDir的旋转速度大于RootForward来解决
            if (bodysm.curState.Equals(BodyState.Stepping) && Vector3.Angle(poseInfo.moveDir, humanBone.root.forward) < 180.1f) {
                float bodyRollProcess = Mathf.Clamp((Mathf.Abs(Vector3.SignedAngle(poseInfo.moveDir, humanBone.root.forward, Vector3.up)) * 1.5f) / 120.0f, 0, 1.0f);
                bodyRollProcess = Mathf.Sin(bodyRollProcess * Mathf.PI);
                bodyRollProcess = poseInfo.speed < 0.5f ? 0 : bodyRollProcess;
                float bodyRollSign = Mathf.Sign(Vector3.SignedAngle(poseInfo.moveDir, humanBone.root.forward, Vector3.up));
                Quaternion bodyRoll = Quaternion.AngleAxis(bodyRollSign * bodyRollProcess * animSetting.bodyRollAngle, Vector3.forward);
                poseInfo.bodyTargetRotLocal = bodyRoll * poseInfo.bodyTargetRotLocal;
            }
            
        }

    }

#endregion

#region Private Implement 

    private void UpdateVelocityInfo() {

        Vector3 lastMoveDir = poseInfo.moveDir;
        poseInfo.speed = poseInfo.velocity.magnitude;

        if (poseInfo.speed > 0.01f) {
            poseInfo.moveDir = poseInfo.velocity.ClearY().normalized;
        }

        if (poseInfo.moveDir == Vector3.zero || lastMoveDir == Vector3.zero) {
            poseInfo.turnAngle = 0;
        } else {
            poseInfo.turnAngle = Vector3.Angle(lastMoveDir, poseInfo.moveDir);
        }

    }

    public void InitPoseInfoAndResetBonePosToBoneSetting() {

        Debug.Log("init bone");
        
        InitPoseInfo();
        InitFootHand();
        ResetBodyPosToBoneSetting();
        ResetFootHandPoseInfoToBoneSetting();
        ResetFootHandPosToPoseInfo();
        
    }

    public void InitPoseInfo() {
        poseInfo = new();
        SetPoseLayerIndex(0);
        SetPose(0);
    }

    // 要先initBody 因为 Foot Hand 的位置依赖于 Body 的相对位置
    private void ResetBodyPosToBoneSetting() {
        // 将Body的初始位置作为原始位置
        // 初始化PoseInfo中的数值
        poseInfo.bodyPosLocal = humanBone.bodyOriPosLocal;
        poseInfo.bodyTargetPosLocal = poseInfo.bodyPosLocal;

        // 初始化Body的旋转到Pose
        humanBone.body.localPosition = poseInfo.bodyPosLocal;
        humanBone.body.localRotation = curPose.body.localRotation;

        humanBone.lPelvis.localPosition = humanBone.lPelvisOriPosLocal + humanBone.body.localPosition;
        humanBone.rPelvis.localPosition = humanBone.rPelvisOriPosLocal + humanBone.body.localPosition;
        humanBone.lShoulder.localPosition = humanBone.lShoulderOriPosLocal + humanBone.body.localPosition;
        humanBone.rShoulder.localPosition = humanBone.rShoulderOriPosLocal + humanBone.body.localPosition;

    }

    private void ResetFootHandPoseInfoToBoneSetting() {

        if (humanBone.isLLegActive) {
            poseInfo.lFootTargetPos = humanBone.body.position + humanBone.body.rotation * (curPose.lFoot.localPosition - curPose.body.localPosition);
        }

        if (humanBone.isRLegActive) {
            poseInfo.rFootTargetPos = humanBone.body.position + humanBone.body.rotation * (curPose.rFoot.localPosition - curPose.body.localPosition);
        }
        
        if (humanBone.isLArmActive) {
            poseInfo.lHandTargetPosLocalRefToBody = curPose.lHand.localPosition - humanBone.body.localPosition;
            poseInfo.lHandTargetRotLocalRefToBody = curPose.lHand.localRotation;
        }

        if (humanBone.isRArmActive) {
            poseInfo.rHandTargetPosLocalRefToBody = curPose.rHand.localPosition - humanBone.body.localPosition;
            poseInfo.rHandTargetRotLocalRefToBody = curPose.rHand.localRotation;
        }

    }

    private void ResetFootHandPosToPoseInfo() {

        if (humanBone.isLLegActive) {
            lFoot.SetFootPos(poseInfo.lFootTargetPos);
        }

        if (humanBone.isRLegActive) {
            rFoot.SetFootPos(poseInfo.rFootTargetPos);
            // humanBone.rFoot.transform.rotation = rFoot.archoringRot;
        }
        
        if (humanBone.isLArmActive) {
            lHand.SetHandPos(poseInfo.lHandTargetPosLocalRefToBody);
        }

        if (humanBone.isRArmActive) {
            rHand.SetHandPos(poseInfo.rHandTargetPosLocalRefToBody);
        }
    }

    private void InitFootHand() {

        footsm.Init();
        handsm.Init();

        if (humanBone.isLLegActive) {
            lFoot = new(this, humanBone.lFoot);
        }
        if (humanBone.isRLegActive) {
            rFoot = new(this, humanBone.rFoot);
        }
        
        if (humanBone.isLArmActive) {
            lHand = new(this, humanBone.lHand);
        }
        if (humanBone.isRArmActive) {
            rHand = new(this, humanBone.rHand);
        }
    }

    private HumanPoseLayer GetPoseLayer(int layerIndex) {
        if (layerIndex > humanBone.poseLayers.Length - 1) {
            return null;
        }
        return humanBone.poseLayers[layerIndex];
    }

    private void SetPose(int poseIndex) {
        if (poseIndex > curPoseLayer.poses.Length - 1) {
            return;
        }

        curPoseIndex = poseIndex;

        FreshCurPose();

    }

    /// <summary>
    /// 根据当前的OverridePose刷新CurPose，当Override的Pose的Target的LocalPos为0时代表没有动画，此时使用默认的PoseTarget
    /// </summary>
    private void FreshCurPose() {

        Transform poseTarget;

        poseTarget = bodyPoseLayerIndexOverride > 0 ? GetPoseLayer(bodyPoseLayerIndexOverride).poses[curPoseIndex].body : curPoseLayer.poses[curPoseIndex].body;
        curPose.body = poseTarget.localPosition == Vector3.zero ? curPose.body : poseTarget;

        poseTarget = lHandPoseLayerIndexOverride > 0 ? GetPoseLayer(lHandPoseLayerIndexOverride).poses[curPoseIndex].lHand : curPoseLayer.poses[curPoseIndex].lHand;
        curPose.lHand = poseTarget.localPosition == Vector3.zero ? curPose.lHand : poseTarget;

        poseTarget = rHandPoseLayerIndexOverride > 0 ? GetPoseLayer(rHandPoseLayerIndexOverride).poses[curPoseIndex].rHand : curPoseLayer.poses[curPoseIndex].rHand;
        curPose.rHand = poseTarget.localPosition == Vector3.zero ? curPose.rHand : poseTarget;

        poseTarget = lFootPoseLayerIndexOverride > 0 ? GetPoseLayer(lFootPoseLayerIndexOverride).poses[curPoseIndex].lFoot : curPoseLayer.poses[curPoseIndex].lFoot;
        curPose.lFoot = poseTarget.localPosition == Vector3.zero ? curPose.lFoot : poseTarget;

        poseTarget = rFootPoseLayerIndexOverride > 0 ? GetPoseLayer(rFootPoseLayerIndexOverride).poses[curPoseIndex].rFoot : curPoseLayer.poses[curPoseIndex].rFoot;
        curPose.rFoot = poseTarget.localPosition == Vector3.zero ? curPose.rFoot : poseTarget;

    }

#endregion

#region Public Interface 

    public void StartUpdateHandPose(float poseChangeDuration = -1.0f) {
        lHand.poseDuration = poseChangeDuration < 0 ? animSetting.handPoseDuration : poseChangeDuration;
        rHand.poseDuration = poseChangeDuration < 0 ? animSetting.handPoseDuration : poseChangeDuration;
        handsm.GotoState(HandState.Pose);
    }


    public void SetLookAt(Transform bodyTarget, Transform headTarget) {
        lookAtTarget_body = bodyTarget;
        lookAtTarget_head = headTarget;
    }

    public void SetLHandArchoring(Transform target, float transition = 0) {
        lHand.ArchoringAt(target, transition);
    }

    public void SetRHandArchoring(Transform target, float transition = 0) {
        rHand.ArchoringAt(target, transition);
    }

    public void SetLFootArchoring(Transform target, float transition = 0) {
        lFoot.ArchoringAt(target, transition);
        footsm.GotoState(target ? FootState.Pose : FootState.Idle);
    }

    public void SetRFootArchoring(Transform target, float transition = 0) {
        rFoot.ArchoringAt(target, transition);
        footsm.GotoState(target ? FootState.Pose : FootState.Idle);
    }

    public void SetPoseLayerIndex(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        curPoseLayerIndex = poseLayerIndex;
        FreshCurPose();
    }

    public void SetAllPoseLayerOverride(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        bodyPoseLayerIndexOverride = poseLayerIndex;
        lHandPoseLayerIndexOverride = poseLayerIndex;
        rHandPoseLayerIndexOverride = poseLayerIndex;
        lFootPoseLayerIndexOverride = poseLayerIndex;
        rFootPoseLayerIndexOverride = poseLayerIndex;

        FreshCurPose();
    }

    public void SetFootAndBodyPoseLayerOverride(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        bodyPoseLayerIndexOverride = poseLayerIndex;
        lFootPoseLayerIndexOverride = poseLayerIndex;
        rFootPoseLayerIndexOverride = poseLayerIndex;

        FreshCurPose();
    }

    public void SetBodyPoseLayerOverride(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        bodyPoseLayerIndexOverride = poseLayerIndex;

        FreshCurPose();
    }

    public void SetLHandPoseLayerOverride(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        lHandPoseLayerIndexOverride = poseLayerIndex;

        FreshCurPose();
    }

    public void SetRHandPoseLayerOverride(int poseLayerIndex) {
        if (poseLayerIndex > humanBone.poseLayers.Length - 1) {
            return;
        }
        rHandPoseLayerIndexOverride = poseLayerIndex;

        FreshCurPose();
    }

#endregion

#region Private Core Class 

    [System.Serializable]
    public class Foot {
        public enum State {
            Stand, Step, ArchoringAtTarget
        }

        private Transform footBone;
        private PCDHuman human;
        private float stepDurationCount;
        private float stepProcess;

        public Transform archoringTarget;
        public float archoringTransition = 0;
        public float archoringTransitionCount;
        public float archoringTransitionProcess;
        public Vector3 lastPos;
        public Quaternion lastRot;
        public Vector3 lastPosLocal;

        public Vector3 archoringPos;
        public Quaternion archoringRot;
        public Vector3 pelvisPosLocal;
        public Vector3 targetPos;
        public Vector3 curPos;
        public Quaternion targetRot;

        private UnityAction stepFinishCallBack;
        public StateMachine<State> sm = new StateMachine<State>(State.Stand);

        public Foot(PCDHuman human, Transform footBone) {
            this.human = human;
            this.footBone = footBone;
            InitSM();
        }

        public void Step(UnityAction stepFinishCallBack) {
            this.stepFinishCallBack = stepFinishCallBack;
            sm.GotoState(State.Step);
        }

        public void Update(Vector3 pelvisPosLocal,Vector3 targetPos, Quaternion targetRot) {
            this.pelvisPosLocal = pelvisPosLocal;
            this.targetPos = targetPos;
            this.targetRot = targetRot;
            sm.UpdateStateAction();
        }

        public float GetDisToTargetPos() {
            return Vector3.Distance(archoringPos, targetPos);
        }

        public float GetProcess() {
            return stepProcess;
        }

        public void ArchoringAt(Transform archoringTarget, float transition = 0) {
            if (archoringTarget == null) {
                sm.GotoState(State.Stand);
                this.archoringTarget = null;
                return;
            }
            this.archoringTarget = archoringTarget;
            archoringTransition = transition;

            sm.GotoState(State.ArchoringAtTarget);
        }

        public void SetFootPos(Vector3 curFootPos) {
            curPos = lastPos = targetPos = archoringPos = curFootPos;
            footBone.position = curPos;
        }

        private void InitSM() {

            sm.GetState(State.Stand).Bind(
                () => {
                },
                () => {
                    footBone.position = archoringPos;
                    footBone.rotation = targetRot;
                },
                () => {}
            );

            sm.GetState(State.Step).Bind(
                () => {
                    stepDurationCount = 0;
                },
                () => {

                    stepDurationCount += human.scaleDeltaTime;
                    stepProcess = Mathf.Min(1.0f, stepDurationCount / human.animSetting.stepDuration);

                    curPos = Vector3.Lerp(archoringPos, targetPos, human.animSetting.footPosCurve.Evaluate(stepProcess));
                    curPos += Vector3.up * human.animSetting.footHeightCurve.Evaluate(stepProcess) * human.humanBone.rootScale;

                    footBone.position = curPos;

                    footBone.rotation = targetRot;

                    // if step finish
                    if (stepProcess >= 1.0f) {
                        sm.GotoState(State.Stand);
                        stepFinishCallBack.Invoke();
                        archoringPos = curPos.CopySetY(human.humanBone.root.position.y);                      
                    }


                },
                () => {

                }
            );

            sm.GetState(State.ArchoringAtTarget).Bind(
                () => {
                    archoringTransitionCount = 0;
                    lastPos = footBone.position;
                    lastRot = footBone.rotation;
                },
                () => {

                    if (archoringTarget == null) {
                        lastPosLocal = Quaternion.Inverse(human.humanBone.body.localRotation) * (footBone.localPosition - human.humanBone.body.localPosition);
                        sm.GotoState(State.Stand);
                        return;
                    }

                    if (archoringTransition == 0) {
                        footBone.position = archoringTarget.position;
                        footBone.rotation = archoringTarget.rotation;
                    } else {
                        archoringTransitionCount += Time.deltaTime;
                        archoringTransitionProcess = Mathf.Min(1.0f, archoringTransitionCount / archoringTransition);
                        footBone.position = Vector3.Lerp(lastPos, archoringTarget.position, archoringTransitionProcess);
                        footBone.rotation = Quaternion.Lerp(lastRot, archoringTarget.rotation, archoringTransitionProcess);
                    }

                    

                },
                () => {
                }
            );

            sm.Init();
        }

    }

    [System.Serializable]
    public class Hand {

        public enum State {
            Pose, ArchoringAtTarget
        }

        private PCDHuman human;
        private Transform handBone;
        public float poseDuration;
        public float poseDurationCount;
        public float poseProcess;
        public float archoringTransition = 0;
        public float archoringTransitionCount;
        public float archoringTransitionProcess;

        public Vector3 archoringPos;
        public Vector3 shoulderPosLocalRefToBody;
        public Vector3 targetPosLocalRefToBody;
        public Quaternion targetRotLocal;
        public Vector3 lastPosLocalRefToBody;
        public Vector3 curPosLocalRefToBody;
        public Transform archoringTarget;
        public Vector3 curPos => human.humanBone.body.position + human.humanBone.body.rotation * curPosLocalRefToBody;
        public Vector3 targetPos => human.humanBone.body.position + human.humanBone.body.rotation * targetPosLocalRefToBody;
        public Vector3 lastPos;
        public Quaternion lastRot;

        public StateMachine<State> sm = new StateMachine<State>(State.Pose);

        public Hand(PCDHuman human, Transform handBone) {
            this.human = human;
            this.handBone = handBone;
            this.poseDuration = human.animSetting.handPoseDuration;
            InitSM();
        }

        public void DoPose() {
            if (sm.curState.Equals(State.ArchoringAtTarget)) {
                return;
            }
            sm.GotoState(State.Pose);
        }

        public void ArchoringAt(Transform archoringTarget, float transition = 0) {
            if (archoringTarget == null) {
                sm.GotoState(State.Pose);
                this.archoringTarget = null;
                return;
            }
            this.archoringTarget = archoringTarget;
            // handBone.position = archoringTarget.position;
            archoringTransition = transition;

            sm.GotoState(State.ArchoringAtTarget);
        }

        public void Update(Vector3 shoulderPosLocal, Vector3 targetPosLocal, Quaternion targetRotLocal) {
            this.shoulderPosLocalRefToBody = shoulderPosLocal;
            this.targetPosLocalRefToBody = targetPosLocal;
            this.targetRotLocal = targetRotLocal;
            sm.UpdateStateAction();
        }

        public void SetHandPos(Vector3 curHandPosLocalRefToBody) {
            curPosLocalRefToBody = lastPosLocalRefToBody = targetPosLocalRefToBody = curHandPosLocalRefToBody;
            handBone.localPosition = human.humanBone.body.localPosition + human.humanBone.body.localRotation * curPosLocalRefToBody;
        }

        private void InitSM() {

            sm.GetState(State.Pose).Bind(
                () => {
                    poseDurationCount = 0;
                    // lastPosLocal = curPosLocal;
                    lastPosLocalRefToBody = Quaternion.Inverse(human.humanBone.body.localRotation) * (handBone.localPosition - human.humanBone.body.localPosition);
                },
                () => {

                    poseDurationCount += human.scaleDeltaTime;
                    poseProcess = Mathf.Min(1.0f, (float)(poseDurationCount / poseDuration));

                    curPosLocalRefToBody = Vector3.Lerp(lastPosLocalRefToBody, targetPosLocalRefToBody, human.animSetting.handPosCurve.Evaluate(poseProcess));
                    handBone.localPosition = human.humanBone.body.localPosition + human.humanBone.body.localRotation * curPosLocalRefToBody;
                    Vector3 shoulderToHand = handBone.position - human.humanBone.root.position + shoulderPosLocalRefToBody;
                    handBone.localRotation = Quaternion.Slerp(handBone.localRotation, targetRotLocal, 2.0f * Time.deltaTime);
                    // handBone.localRotation = targetRotLocal;

                },
                () => {}
            );

            sm.GetState(State.ArchoringAtTarget).Bind(
                () => {
                    archoringTransitionCount = 0;
                    lastPos = handBone.position;
                    lastRot = handBone.rotation;
                },
                () => {

                    if (archoringTarget == null) {
                        lastPosLocalRefToBody = Quaternion.Inverse(human.humanBone.body.localRotation) * (handBone.localPosition - human.humanBone.body.localPosition);
                        sm.GotoState(State.Pose);
                        return;
                    }

                    if (archoringTransition == 0) {
                        handBone.position = archoringTarget.position;
                        handBone.rotation = archoringTarget.rotation;
                    } else {
                        archoringTransitionCount += Time.deltaTime;
                        archoringTransitionProcess = Mathf.Min(1.0f, archoringTransitionCount / archoringTransition);
                        handBone.position = Vector3.Lerp(lastPos, archoringTarget.position, archoringTransitionProcess);
                        handBone.rotation = Quaternion.Lerp(lastRot, archoringTarget.rotation, archoringTransitionProcess);
                    }

                    

                },
                () => {
                }
            );

            sm.Init();

        }
        
    }

#endregion

#region Private InfoClass

     [System.Serializable]
    public class PCDHumanBoneSetting {

        public Transform root;

        public Transform head;

        public Transform body;

        public Transform lPelvis;
        public Transform rPelvis;

        public Transform lFoot;
        public Transform rFoot;

        public Transform lShoulder;
        public Transform rShoulder;

        public Transform lHand;
        public Transform rHand;

        public Transform poseLayerParent;
        public HumanPoseLayer[] poseLayers;

        public Vector3 bodyOriPosLocal;
        public Vector3 lPelvisOriPosLocal;
        public Vector3 rPelvisOriPosLocal;
        public Vector3 lFootOriPosLocal;
        public Vector3 rFootOriPosLocal;
        public Vector3 lShoulderOriPosLocal;
        public Vector3 rShoulderOriPosLocal;
        public Vector3 lHandOriPosLocal;
        public Vector3 rHandOriPosLocal;


        public bool isLLegActive;
        public bool isRLegActive;
        public bool isLArmActive;
        public bool isRArmActive;

        public Quaternion toLocalRot => Quaternion.Inverse(root.rotation);
        public float rootScale => root.localScale.x;

        public void Init() {

            isLLegActive = lPelvis != null && lFoot != null;
            isRLegActive = rPelvis != null && rFoot != null;
            isLArmActive = lShoulder != null && lHand != null;
            isRArmActive = rShoulder != null && rHand != null;

            bodyOriPosLocal = body.localPosition;

            if (isLLegActive) {
                lPelvisOriPosLocal = lPelvis.localPosition - body.localPosition;
                lFootOriPosLocal = lFoot.localPosition - body.localPosition;
            }

            if (isRLegActive) {
                rPelvisOriPosLocal = rPelvis.localPosition - body.localPosition;
                rFootOriPosLocal = rFoot.localPosition - body.localPosition;
            }

            if (isLArmActive) {
                lShoulderOriPosLocal = lShoulder.localPosition - body.localPosition;
                lHandOriPosLocal = lHand.localPosition - body.localPosition;
            }

            if (isRArmActive) {
                rShoulderOriPosLocal = rShoulder.localPosition - body.localPosition;
                rHandOriPosLocal = rHand.localPosition - body.localPosition;
            }

            if (poseLayerParent && poseLayerParent.childCount > 0) {
                poseLayers = new HumanPoseLayer[poseLayerParent.childCount];
                for (int i = 0; i < poseLayerParent.childCount; i++) {
                    poseLayers[i] = new HumanPoseLayer(poseLayerParent.GetChild(i));
                }
            }

        }

    }

    [System.Serializable]
    public class HumanPose {
        public Transform parent;
        public HumanPose(Transform parent) {
            if (parent == null) {
                return;
            }
            this.parent = parent;
            body = parent.GetChild(0);
            lHand = parent.GetChild(1);
            rHand = parent.GetChild(2);
            lFoot = parent.GetChild(3);
            rFoot = parent.GetChild(4);
        }
        public Transform body;
        public Transform lHand;
        public Transform rHand;
        public Transform lFoot;
        public Transform rFoot;

    }

    [System.Serializable]
    public class HumanPoseLayer {
        public Transform parent;
        public HumanPose[] poses;
        public HumanPoseLayer(Transform parent) {
            this.parent = parent;
            poses = new HumanPose[parent.childCount];
            for (int i = 0; i < parent.childCount; i++) {
                poses[i] = new HumanPose(parent.GetChild(i));
            }
        }
    }
    
    [System.Serializable]
    public class PoseInfo {
        public Vector3 velocity;
        public Vector3 moveDir;
        // 待添加功能：用目标运动方向代替当前运动方向来判断Roll动画
        public Vector3 targetMoveDir;
        public float turnAngle;
        public float speed;
        public Vector3 lPelvisPosLocal;
        public Vector3 rPelvisPosLocal;
        public Vector3 lFootTargetPos;
        public Vector3 rFootTargetPos;
        public Vector3 lShoulderPosLocal;
        public Vector3 rShoulderPosLocal;
        public Vector3 lHandTargetPosLocalRefToBody;
        public Vector3 rHandTargetPosLocalRefToBody;
        public Quaternion lHandTargetRotLocalRefToBody;
        public Quaternion rHandTargetRotLocalRefToBody;
        public Vector3 bodyPosLocal;
        public Quaternion bodyTargetRotLocal;
        public Vector3 bodyTargetPosLocal;
        public Vector3 bodyVelocity;
        public Vector3 bodyOffsetLocal;
    }

    [System.Serializable]
    public class AnimSetting {
        public float oriSpeed = 5.0f;
        public bool stepRightFootFirst = false;
        [Tooltip("迈步时脚部位置的差值曲线")]
        public AnimationCurve footPosCurve;
        [Tooltip("迈步时的脚部高度曲线")]
        public AnimationCurve footHeightCurve;
        [Tooltip("迈步迈多远")]
        public float stepTargetOffset = 0.45f;
        [Tooltip("一步迈多久")]
        public float stepDuration = 0.18f;
        [Tooltip("迈一步之后停顿多久")]
        public float stepInterval = 0.05f;
        [Tooltip("距离目标多远时迈步")]
        public float stepTriggerDis = 1.2f;
        [Tooltip("速度多大时迈步")]
        public float stepTriggerSpeed = 1f;

        [Tooltip("迈步时手部位置的差值曲线")]
        public AnimationCurve handPosCurve;
        [Tooltip("手部动多久")]
        public float handPoseDuration = 0.2f;

        [Tooltip("迈步时的身体高度曲线")]
        public AnimationCurve bodyHeightCurve;
        [Tooltip("身体弹簧运动的拉扯强度，越高弹簧加速越快")]
        public float bodyDragStrength = 500f;
        [Tooltip("身体弹簧运动的速度衰减，越高弹簧复位越快")]
        public float bodyVelocityDamp = 10f;
        [Tooltip("身体在运动时的俯仰角")]
        public float bodyRollAngle = 10f;
        [Tooltip("身体在运动时的翻滚角")]
        public float bodyRotSpeed = 8f;
        public float bodyOffsetSpeed = 5.0f;

        [Range(0, 1.0f)]
        public float lookAtWeight_head = 0.25f;
        [Range(0, 1.0f)]
        public float lookAtWeight_body = 0.75f;
    }


#endregion

    private bool drawGizmos = false;

    private void OnDrawGizmos() {

        if (!drawGizmos) {
            return;
        }

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(poseInfo.lFootTargetPos, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lHand.targetPos, 0.05f);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(poseInfo.rFootTargetPos, 0.05f);
        Gizmos.DrawSphere(rHand.targetPos, 0.05f);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(lFoot.archoringPos, 0.05f);
        // Gizmos.DrawSphere(rFoot.archoringPos, 0.05f);
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(lFoot.targetPos, 0.05f);
        // Gizmos.DrawSphere(rFoot.targetPos, 0.05f);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(lHand.archoringPos, 0.05f);
        // Gizmos.DrawSphere(rHand.archoringPos, 0.05f);
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(lHand.targetPos, 0.05f);
        // Gizmos.DrawSphere(rHand.targetPos, 0.05f);
    }

}
