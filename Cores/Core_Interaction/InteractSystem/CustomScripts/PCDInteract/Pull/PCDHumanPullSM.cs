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
    private PCDHuman human;

    private FMODUnity.StudioEventEmitter emitter;
    private FMOD.Studio.EventInstance actionEventInstance;

    private Transform pullingObjFollowTarget;

    void Awake() {

        human = GetComponentInChildren<PCDHuman>();
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
                human.SetLHandArchoring(pullingObject.lHaft, animationTransition);
                human.SetRHandArchoring(pullingObject.rHaft, animationTransition);
                // human.SetLHandArchoring(lHandFollowTarget, animationTransition);
                // human.SetRHandArchoring(rHandFollowTarget, animationTransition);

                /* Override Human LookAt & FootAndBodyPoseLayer */
                human.SetLookAt(null, pullingObject.transform);
                human.SetFootAndBodyPoseLayerOverrideName(pullingSmallBoxPoseLayerName);

                /* Override PullablePCDIK Target */
                pullingObject.GetComponentInParent<PullablePCDIKController>()?.SetFollowTargetOverride(pullingObjFollowTarget, animationTransition);

                lHandFollowTarget.position = human.humanBone.lShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.lHaft.localPosition.x - human.humanBone.lShoulder.position).normalized * 1f;
                rHandFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.rHaft.localPosition.x - human.humanBone.rShoulder.position).normalized * 1f;
                pullingObjFollowTarget.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;

            },
            () => {
                lHandFollowTarget.position = human.humanBone.lShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.lHaft.localPosition.x - human.humanBone.lShoulder.position).normalized * 1f;
                rHandFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.rHaft.localPosition.x - human.humanBone.rShoulder.position).normalized * 1f;
                pullingObjFollowTarget.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;
            },
            () => {
                human.SetLookAt(null, null);
                human.SetLHandArchoring(null);
                human.SetRHandArchoring(null);
                human.SetFootAndBodyPoseLayerOverrideIndex(-1); 
                if (pullingObject) {
                    pullingObject.GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
                }
            }
        );

        sm.GetState(State.PullingSmallBoxHover).Bind(
            
            () => {
                human.SetLookAt(pullingObjectHover.transform, pullingObjectHover.transform);
            },
            () => {
            },
            () => {
                human.SetLookAt(null, null);
            }
        );
        
        sm.GetState(State.PullingStick).Bind(
            () => {

                // 设置 HandArchoring & HandPoseOverride
                human.SetLHandPoseLayerOverrideName(pullingStickPoseLayerName);
                human.SetRHandArchoring(pullingObjFollowTarget, animationTransition);
                // human.SetRHandArchoring(rHandFollowTarget, animationTransition);

                // 设置 LookAt & FootAndBodyOverride
                human.SetLookAt(null, pullingObject.rHaft);
                human.SetFootAndBodyPoseLayerOverrideName(pullingStickPoseLayerName);

                pullingObjFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position - human.humanBone.rShoulder.position).normalized * 1f;

            },
            () => {

                // 计算并更新手部位置
                pullingObjFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position - human.humanBone.rShoulder.position).normalized * 1f;

            },
            () => {

                // 解除 HandArchoring & HandPoseOverride
                human.SetLHandPoseLayerOverrideIndex(-1);
                human.SetRHandArchoring(null);

                // 解除 LookAt & FootAndBodyOverride
                human.SetLookAt(null, null);
                human.SetFootAndBodyPoseLayerOverrideIndex(-1);

                
            }
        );

        sm.GetState(State.PullingStickHover).Bind(
            
            () => {
                human.SetLookAt(pullingObjectHover.transform, pullingObjectHover.rHaft);
            },
            () => {
            },
            () => {
                human.SetLookAt(null, null);
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
            GameObject.Instantiate(dragStickEffect, human.humanBone.lHand.position, Quaternion.identity, human.humanBone.lHand);
            GameObject.Instantiate(dragStickEffect, human.humanBone.rHand.position, Quaternion.identity, human.humanBone.rHand);
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


