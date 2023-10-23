using UnityEngine;

public class PullablePCDIKController : MonoBehaviour {
    public Transform root;
    public Transform head;
    public Transform neckRoot;
    public Transform defaultFollowTarget;
    public Transform overrideFollowTarget;
    public Transform lookAtTarget;
    public float rotSpeed = 3.0f;
    public float lookAtTriggerDis = 12.0f;
    protected Vector3 posOffsetScale;
    protected Quaternion targetRot;
    protected float lengthScale => root.localScale.y;
    protected Vector3 beforeOverridePos;
    protected Quaternion beforeOverrideRot;
    protected float overrideTransition;
    [SerializeField]
    protected float overrideTransitionCount;
    protected float overrideTransitionprocess;

    void Update() {

        neckRoot.rotation = Quaternion.LookRotation(head.position - neckRoot.position);

        if (lookAtTarget) {

            float toHeadLookAtTargetDisScale = Vector3.Distance(lookAtTarget.position, transform.position) * lengthScale;
            float lookAtTriggerDisScale = lookAtTriggerDis * lengthScale;

            if (toHeadLookAtTargetDisScale < lookAtTriggerDisScale) {

                targetRot = Quaternion.LookRotation((lookAtTarget.position - transform.position).ClearY(),  Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);

            }

        } else {
            posOffsetScale = Vector3.zero;
        }

        overrideTransitionCount += Time.deltaTime;
        overrideTransitionprocess = Mathf.Min(1.0f, overrideTransitionCount / overrideTransition);
        if (overrideFollowTarget) {
            head.position = Vector3.Lerp(beforeOverridePos, overrideFollowTarget.position, overrideTransitionprocess);
            // head.rotation = Quaternion.Lerp(beforeOverrideRot, overrideFollowTarget.rotation, overrideTransitionprocess);
        }
        else if (defaultFollowTarget) {
            head.position = Vector3.Lerp(beforeOverridePos, defaultFollowTarget.position + posOffsetScale, overrideTransitionprocess);
            // head.rotation = Quaternion.Lerp(beforeOverrideRot, defaultFollowTarget.rotation, overrideTransitionprocess);
        }
        
    }

    public void SetLookAtTarget() {

    }

    public void SetFollowTargetOverride(Transform followTarget, float transiton = 0.06f) {
        overrideFollowTarget = followTarget;
        beforeOverridePos = head.position;
        beforeOverrideRot = head.rotation;
        overrideTransition = transiton;
        overrideTransitionCount = 0;
    }

}
