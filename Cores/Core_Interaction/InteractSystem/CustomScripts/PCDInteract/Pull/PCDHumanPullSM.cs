using System;
using UnityEngine;

public class PCDHumanPullSM : MonoBehaviour {

    public enum State {
        Idle, PullingSmallBox, PullingStick,
        PullingSmallBoxHover, PullingStickHover
    }

    public float animationTransition = 0.12f;
    public Transform boxObjFollowTarget;
    public Transform stickObjFollowTarget;
    public Transform longStickObjFollowTarget;
    public bool isPulling => pullingObject != null;
    public bool isPullingBox => sm.curState.Equals(State.PullingSmallBox);
    public bool isPullingStick => sm.curState.Equals(State.PullingStick);
    [SerializeField]
    private PullableObject pullingObject;
    [SerializeField]
    private PullableObject pullingObjectHover;
    private bool draggingObjIsStretchable = false;
    private Transform rHandFollowTarget;
    private Transform lHandFollowTarget;
    [SerializeField]
    private string pullingSmallBoxPoseLayerName = "PullingSmallBox";
    [SerializeField]
    private string pullingStickPoseLayerName = "PullingStick";
    [SerializeField]
    private GameObject dragBoxEffect;
    [SerializeField]
    private GameObject dragStickEffect;
    [SerializeField]
    private string actionEventInstancePath;
    [SerializeField]
    private StateMachine<State> sm;
    private PCDWalkMgr walkMgr;
    private PCDPoseMgr poseMgr;
    private PCDArchoringMgr archoringMgr;
    private PCDAnimator animator;
    private PCDHumanConfig humanConfig;

    private FMODUnity.StudioEventEmitter emitter;
    private FMOD.Studio.EventInstance actionEventInstance;

    private Transform pullingObjFollowTarget;
    private PCDSkeleton skeleton => humanConfig.skeleton;

    void Awake() {

        walkMgr = GetComponentInChildren<PCDWalkMgr>();
        poseMgr = GetComponentInChildren<PCDPoseMgr>();
        archoringMgr = GetComponentInChildren<PCDArchoringMgr>();
        animator = GetComponentInChildren<PCDAnimator>();
        humanConfig = GetComponentInChildren<PCDHumanConfig>();
        InitHandActionSM();

        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        SafeRun.Run(() => {
            actionEventInstance = FMODUnity.RuntimeManager.CreateInstance(actionEventInstancePath);
        });

        rHandFollowTarget = new GameObject("LHandFollowTarget").transform;
        lHandFollowTarget = new GameObject("RHandFollowTarget").transform;
        pullingObjFollowTarget = new GameObject("HaftCenter").transform;

    }
    
    void InitHandActionSM() {

        sm = new StateMachine<State>(State.Idle);

        sm.GetState(State.Idle).Bind(
            () => {},
            () => {},
            () => {}
        );


        sm.GetState(State.PullingSmallBox).Bind(
            () => {

                /* Archoring Hands */
                // walkMgr.SetLHandArchoring(pullingObject.lHaft, animationTransition);
                // walkMgr.SetRHandArchoring(pullingObject.rHaft, animationTransition);
                archoringMgr.BoneArchoringToTransform("LHand", pullingObject.lHaft, animationTransition);
                archoringMgr.BoneArchoringToTransform("RHand", pullingObject.rHaft, animationTransition);

                /* Override Human LookAt & FootAndBodyPoseLayer */
                walkMgr.SetLookAt(pullingObject.transform);
                // poseMgr.SetFootAndBodyPoseLayerOverrideName(pullingSmallBoxPoseLayerName);
                poseMgr.FadeToKeyFrame(animator.GetAnimReader(pullingSmallBoxPoseLayerName).GetKeyFrameReader("Idle"), true, false, false, false, false);
                walkMgr.SetAnim(pullingSmallBoxPoseLayerName);

                /* Override PullablePCDIK Target */
                pullingObject.GetComponentInParent<PullablePCDIKController>()?.SetFollowTargetOverride(pullingObjFollowTarget, animationTransition);

                lHandFollowTarget.position = skeleton.humanBone.lShoulder.transform.position + (pullingObject.transform.position + skeleton.humanBone.root.transform.right * pullingObject.lHaft.localPosition.x - skeleton.humanBone.lShoulder.transform.position).normalized * 1f;
                rHandFollowTarget.position = skeleton.humanBone.rShoulder.transform.position + (pullingObject.transform.position + skeleton.humanBone.root.transform.right * pullingObject.rHaft.localPosition.x - skeleton.humanBone.rShoulder.transform.position).normalized * 1f;
                pullingObjFollowTarget.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;

            },
            () => {
                lHandFollowTarget.position = skeleton.humanBone.lShoulder.transform.position + (pullingObject.transform.position + skeleton.humanBone.root.transform.right * pullingObject.lHaft.localPosition.x - skeleton.humanBone.lShoulder.transform.position).normalized * 1f;
                rHandFollowTarget.position = skeleton.humanBone.rShoulder.transform.position + (pullingObject.transform.position + skeleton.humanBone.root.transform.right * pullingObject.rHaft.localPosition.x - skeleton.humanBone.rShoulder.transform.position).normalized * 1f;
                pullingObjFollowTarget.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;
            },
            () => {
                walkMgr.ResetLookAt();
                // archoringMgr.SetLHandArchoring(null);
                // archoringMgr.SetRHandArchoring(null);
                archoringMgr.ResetBoneFromArchoring("LHand");
                archoringMgr.ResetBoneFromArchoring("RHand");
                // walkMgr.SetFootAndBodyPoseLayerOverrideIndex(-1); 
                walkMgr.ResetAnimToDefault();
                if (pullingObject) {
                    pullingObject.GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
                }
            }
        );

        sm.GetState(State.PullingSmallBoxHover).Bind(
            
            () => {
                walkMgr.SetLookAt(pullingObjectHover.transform);
            },
            () => {
            },
            () => {
                walkMgr.ResetLookAt();
            }
        );
        
        sm.GetState(State.PullingStick).Bind(
            () => {

                // 设置 HandArchoring & HandPoseOverride
                // walkMgr.SetLHandPoseLayerOverrideName(pullingStickPoseLayerName);
                // walkMgr.SetRHandArchoring(pullingObjFollowTarget, animationTransition);
                poseMgr.FadeToKeyFrame(animator.GetAnimReader(pullingStickPoseLayerName).GetKeyFrameReader("Idle"), false, true, false, false, false);
                // archoringMgr.SetRHandArchoring(pullingObjFollowTarget, animationTransition);
                archoringMgr.BoneArchoringToTransform("RHand", pullingObjFollowTarget, animationTransition);
                // human.SetRHandArchoring(rHandFollowTarget, animationTransition);

                // 设置 LookAt & FootAndBodyOverride
                walkMgr.SetLookAt(pullingObject.rHaft);
                // walkMgr.SetFootAndBodyPoseLayerOverrideName(pullingStickPoseLayerName);
                walkMgr.SetAnim(pullingStickPoseLayerName);

                pullingObjFollowTarget.position = skeleton.humanBone.rShoulder.transform.position + (pullingObject.transform.position - skeleton.humanBone.rShoulder.transform.position).normalized * 1f;

            },
            () => {

                // 计算并更新手部位置
                pullingObjFollowTarget.position = skeleton.humanBone.rShoulder.transform.position + (pullingObject.transform.position - skeleton.humanBone.rShoulder.transform.position).normalized * 1f;

            },
            () => {

                // 解除 HandArchoring & HandPoseOverride
                // walkMgr.SetLHandPoseLayerOverrideIndex(-1);
                poseMgr.ResetPose();
                // walkMgr.SetRHandArchoring(null);
                archoringMgr.ResetBoneFromArchoring("RHand");

                // 解除 LookAt & FootAndBodyOverride
                walkMgr.ResetLookAt();
                walkMgr.ResetAnimToDefault();

                
            }
        );

        sm.GetState(State.PullingStickHover).Bind(
            
            () => {
                walkMgr.SetLookAt(pullingObjectHover.rHaft);
            },
            () => {
            },
            () => {
                walkMgr.ResetLookAt();
            }
        );

        sm.Init();

    }

    public void update() {
        sm.UpdateStateAction();
        
    }

#region Private Pull Function

    /// <summary>
    /// pull a small box
    /// </summary>
    /// <param name="box">pullable smallBox</param>
    private void PullSmallBox(PullableObject box) {
        // if (box == null) {
        if ((isPulling && isPullingBox) || box == null) {
            OnPullExit();
            return;
        }
        OnPullEnter(box);
        if (dragStickEffect) {
            GameObject.Instantiate(dragStickEffect, skeleton.humanBone.lHand.transform.position, Quaternion.identity, skeleton.humanBone.lHand.transform);
            GameObject.Instantiate(dragStickEffect, skeleton.humanBone.rHand.transform.position, Quaternion.identity, skeleton.humanBone.rHand.transform);
        }
        // GameObject.Instantiate(holdBoxEffect, curFollowTarget.position + Vector3.up * 0.5f, Quaternion.identity);
        sm.GotoState(State.PullingSmallBox);
    }

    /// <summary>
    /// pull a stick
    /// </summary>
    /// <param name="stick">pullable stick</param>
    private void PullStick(PullableObject stick) {
        if ((isPulling && isPullingStick) || stick == null) {
            OnPullExit();
            return;
        }
        OnPullEnter(stick);
        if (dragStickEffect)
            GameObject.Instantiate(dragStickEffect, pullingObject.rHaft.position, Quaternion.identity, pullingObject.rHaft);
        sm.GotoState(State.PullingStick);
    }

#endregion

#region Public Pull Function

    private Action pulledOutCallback;

    public void PullObj(PullableObject obj, PullInfo pullInfo = null, Action pulledOutCallback = null, PCDObjectProperties.Shape shape = PCDObjectProperties.Shape.Stick) {
        this.pulledOutCallback = pulledOutCallback;
        pullingInfo = pullInfo;
        if (shape == PCDObjectProperties.Shape.SmallBox) {
            PullSmallBox(obj);
        } else if (shape == PCDObjectProperties.Shape.Stick) {
            PullStick(obj);
        }
    }

    public void RestPulling() {
        if (pullingObject == null) {
            return;
        }
        PullObj(null, null, null, pullingObject.shape);
        PullObjHover(null);
        OnPullExit();
    }

    public void PullObjHover(PullableObject obj, PCDObjectProperties.Shape shape = PCDObjectProperties.Shape.Stick) {
        if (obj == null) {
            pullingObjectHover = null;
            sm.GotoState(State.Idle);
            return;
        }

        if (shape == PCDObjectProperties.Shape.SmallBox) {
            if (pullingObjectHover != obj) {
                pullingObjectHover = obj;
                sm.GotoState(State.PullingSmallBoxHover);
            }
            if (sm.curState != State.PullingSmallBoxHover) {
                sm.GotoState(State.PullingSmallBoxHover);
            }
        } else if (shape == PCDObjectProperties.Shape.Stick) {
            if (pullingObjectHover != obj) {
                pullingObjectHover = obj;
                sm.GotoState(State.PullingStickHover);
            }
            if (sm.curState != State.PullingStickHover) {
                sm.GotoState(State.PullingStickHover);
            }
        }
    }

    public void OnPullOutObj() {
        pulledOutCallback?.Invoke();
        emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
    }

#endregion 

#region Private LifeCycle Function

    private void OnPullEnter(PullableObject pullableObj) {
        pullingObject = pullableObj;
        foreach (Collider collider in pullingObject.GetComponents<Collider>()) {
            collider.enabled = false;
        }
        if (pullingObject.GetComponent<Rigidbody>()) {
            pullingObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        pullableObj.StartPullBy(transform, pullingObjFollowTarget);
        emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");

        InitPullOut();
    }

    [SerializeField]
    private PullInfo pullingInfo;
    private float pullOutTimeCount;
    
    private void OnPullExit() {
        sm.GotoState(State.Idle, false);
        pullingObject?.RestPull();
        pullingObject = null;
    }

    private void InitPullOut() {
        pullOutTimeCount = 0;
    }

#endregion

}


