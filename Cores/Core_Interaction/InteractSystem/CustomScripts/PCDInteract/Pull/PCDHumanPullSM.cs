using System;
using UnityEngine;

public class PCDHumanPullSM : MonoBehaviour {

    public enum State {
        Idle, PullingBoxHover, PullingStick,
        PullingSmallBoxHover, PullingStickHover
    }

    public float archoringTransition_pull = 0.18f;
    public Transform boxObjFollowTarget;
    public Transform stickObjFollowTarget;
    public Transform longStickObjFollowTarget;
    public bool isPulling => pullingObject != null;
    public bool isPullingBox => sm.curState.Equals(State.PullingBoxHover);
    public bool isPullingStick => sm.curState.Equals(State.PullingStick);
    [SerializeField]
    private PullableObject pullingObject;
    [SerializeField]
    private PullableObject pullingObjectHover;
    private bool draggingObjIsStretchable = false;
    private Transform rHandFollowTarget;
    private Transform lHandFollowTarget;
    [SerializeField]
    private int pullBoxPoseLayerIndex;
    [SerializeField]
    private int pullStickPoseLayerIndex;
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

    void Awake() {

        human = GetComponentInChildren<PCDHuman>();
        InitHandActionSM();

        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        SafeRun.Run(() => {
            actionEventInstance = FMODUnity.RuntimeManager.CreateInstance(actionEventInstancePath);
        });

        rHandFollowTarget = new GameObject("LHandFollowTarget").transform;
        lHandFollowTarget = new GameObject("RHandFollowTarget").transform;

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

        Transform haftCenter = new GameObject("HaftCenter").transform;

        sm.GetState(State.PullingBoxHover).Bind(
            () => {

                /* Archoring Hands */
                human.SetLHandArchoring(lHandFollowTarget, archoringTransition_pull);
                human.SetRHandArchoring(rHandFollowTarget, archoringTransition_pull);

                /* Override Human LookAt & FootAndBodyPoseLayer */
                human.SetLookAt(null, haftCenter);
                human.SetFootAndBodyPoseLayerOverride(pullBoxPoseLayerIndex);

                /* Override PullablePCDIK Target */
                pullingObject.GetComponentInParent<PullablePCDIKController>()?.SetFollowTargetOverride(haftCenter, archoringTransition_pull);

            },
            () => {
                Debug.DrawLine(pullingObject.transform.position, lHandFollowTarget.position);
                Debug.DrawLine(pullingObject.transform.position, rHandFollowTarget.position);
                lHandFollowTarget.position = human.humanBone.lShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.lHaft.localPosition.x - human.humanBone.lShoulder.position).normalized * 1f;
                rHandFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position + human.humanBone.root.right * pullingObject.rHaft.localPosition.x - human.humanBone.rShoulder.position).normalized * 1f;
                haftCenter.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;
                HandlePullOut();
            },
            () => {
                human.SetLookAt(null, null);
                human.SetLHandArchoring(null);
                human.SetRHandArchoring(null);
                human.SetFootAndBodyPoseLayerOverride(-1); 
                if (pullingObject) {
                    pullingObject.GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
                }
            }
        );

        sm.GetState(State.PullingSmallBoxHover).Bind(
            
            () => {

                /* Archoring Hand */
                // human.SetLHandArchoring(lHandFollowTarget, dragTransition);
                // human.SetRHandArchoring(rHandFollowTarget, dragTransition);

                human.SetLookAt(pullingObjectHover.transform, haftCenter);
                
            },
            () => {

                lHandFollowTarget.position = human.humanBone.lShoulder.position + (pullingObjectHover.transform.position + human.humanBone.root.right * pullingObjectHover.lHaft.localPosition.x - human.humanBone.lShoulder.position).normalized * 1f;
                rHandFollowTarget.position = human.humanBone.rShoulder.position + (pullingObjectHover.transform.position + human.humanBone.root.right * pullingObjectHover.rHaft.localPosition.x - human.humanBone.rShoulder.position).normalized * 1f;
                haftCenter.position = (lHandFollowTarget.transform.position + rHandFollowTarget.transform.position) / 2.0f;

            },
            () => {

                // human.SetLHandArchoring(null);
                // human.SetRHandArchoring(null);

                human.SetLookAt(null, null);

            }
        );
        
        sm.GetState(State.PullingStick).Bind(
            () => {

                // 设置 HandArchoring & HandPoseOverride
                human.SetLHandPoseLayerOverride(pullStickPoseLayerIndex);
                human.SetRHandArchoring(rHandFollowTarget, archoringTransition_pull);

                // 设置 LookAt & FootAndBodyOverride
                human.SetLookAt(null, pullingObject.rHaft);
                human.SetFootAndBodyPoseLayerOverride(pullStickPoseLayerIndex);

                // 设置 PullablePCDIK override
                pullingObject.GetComponentInParent<PullablePCDIKController>()?.SetFollowTargetOverride(rHandFollowTarget, archoringTransition_pull);
            },
            () => {

                // 计算并更新手部位置
                rHandFollowTarget.position = human.humanBone.rShoulder.position + (pullingObject.transform.position - human.humanBone.rShoulder.position).normalized * 1f;

                // 更新PullOut检测
                HandlePullOut();
            },
            () => {

                // 解除 HandArchoring & HandPoseOverride
                human.SetLHandPoseLayerOverride(-1);
                human.SetRHandArchoring(null);

                // 解除 LookAt & FootAndBodyOverride
                human.SetLookAt(null, null);
                human.SetFootAndBodyPoseLayerOverride(-1);

                // 解除 PullablePCDIK override
                if (pullingObject) {
                    pullingObject.GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
                }
            }
        );

        sm.GetState(State.PullingStickHover).Bind(
            
            () => {

                // Archoring Hand
                // human.SetRHandArchoring(rHandFollowTarget, archoringTransition_pull);
                // Override Human LookAt
                human.SetLookAt(pullingObjectHover.transform, pullingObjectHover.rHaft);
                
            },
            () => {

                    // if (!pullingObjectHover) {
                    //     sm.GotoState(State.Idle);
                    // }
                    // if (Vector3.Distance(human.humanBone.rShoulder.position, pullingObjectHover.rHaft.position) < 1f) {
                    //     rHandFollowTarget.transform.position = pullingObjectHover.rHaft.position;
                    // } else {
                    //     rHandFollowTarget.transform.position = human.humanBone.rShoulder.position + (pullingObjectHover.rHaft.position - human.humanBone.rShoulder.position).normalized * 1f;
                    // }

            },
            () => {

                /* Disoverride Hand */
                // human.SetRHandArchoring(null);
                /* Disoverride Human LookAt */
                human.SetLookAt(null, null);
                /* Clear Human PoseOverride */
                // ClearPoseLayerOverride();

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
        sm.GotoState(State.PullingBoxHover);
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


        emitter.SafePlaySetParameterByNameWithLabel("Action", "Pick");

        InitPullOut();
    }

    [SerializeField]
    private PullInfo pullingInfo;
    private float pullOutTimeCount;
    
    private void HandlePullOut() {
        if (pullingInfo == null) {
            return;
        }

        Transform drager = human.humanBone.body;

        float stretchDis = Vector3.Distance(pullingObject.transform.position.ClearY(), drager.position.ClearY());

        float maxStretchLengthScale = pullingInfo.maxStretchLength * pullingObject.rootScale;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce((pullingObject.transform.position - drager.position) * pullingInfo.contractStrength * Time.deltaTime, ForceMode.Acceleration);
        if (stretchDis > maxStretchLengthScale) {
            pullOutTimeCount += Time.deltaTime;
            Debug.DrawLine(pullingObject.transform.position, drager.position, Color.red);
        } else {
            pullOutTimeCount = Mathf.Max(0, pullOutTimeCount - Time.deltaTime);
            Debug.DrawLine(pullingObject.transform.position, drager.position, Color.green);
        }

        if (pullOutTimeCount >= pullingInfo.pullOutTime) {
            pullingObject.OnPulledOut();
            pulledOutCallback?.Invoke();
            RestPulling();
            emitter.SafePlaySetParameterByNameWithLabel("Action", "Drop");
            return;
        }

    }

    private void OnPullExit() {
        // human.SetLookAt(null);
        sm.GotoState(State.Idle, false);
        pullingObject = null;
    }

    private void InitPullOut() {
        pullOutTimeCount = 0;
    }

#endregion

}

[System.Serializable]
public class PullInfo {
    public bool isStretchable = false;
    public float maxStretchLength = 2.0f;
    public float contractStrength = 2.0f;
    public float pullOutTime = 1.0f;
}
