using UnityEngine;
[ExecuteInEditMode]
public class PullablePCDIKController : MonoBehaviour {
    public Transform root;
    public Transform head;
    public Transform neckRoot;
    public Transform defaultFollowTarget;
    public Transform overrideFollowTarget;
    public Transform lookAtTarget;
    public float rotSpeed = 3.0f;
    public float lookAtTriggerDis = 12.0f;
    public AnimationCurve oriJointScaleScaleCurve;
    public AnimationCurve pullOutProcessJointScaleScaleCurve;
    protected Vector3 posOffsetScale;
    protected Quaternion targetRot;
    protected float lengthScale => root.localScale.y;
    protected Vector3 beforeOverridePos;
    protected Quaternion beforeOverrideRot;
    protected float overrideTransition;
    [SerializeField]
    protected float overrideTransitionCount;
    protected float overrideTransitionprocess;
    protected PCDIK pcdik;
    protected PullableObject pullable;

    private void Awake() {
        pcdik = GetComponentInChildren<PCDIK>();
        pullable = GetComponent<PullableObject>();
        oriJointScaleScaleCurve = pcdik.jointScaleScaleCurve;
    }

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
        
        if (pcdik && pullable.puller != null) {

            // 创建新的 AnimationCurve，其关键帧将是两个曲线之间的插值结果
            AnimationCurve interpolatedCurve = new AnimationCurve();
            AnimationCurve curve1 = oriJointScaleScaleCurve;
            AnimationCurve curve2 = pullOutProcessJointScaleScaleCurve;
            // 假设 curve1 和 curve2 有相同数量的关键帧，并且对应的关键帧在两条曲线上表示相同的时间点
            // 这通常是一个有用的前提条件，因为不同的时间点可能导致插值非常复杂
            for (int i = 0; i < curve1.length; i++)
            {
                // 插值关键帧的时间
                float time = curve1.keys[i].time; // 取其中一个曲线的时间，因为它们应该是相同的

                // 插值关键帧的值
                float value = Mathf.Lerp(curve1.Evaluate(time), curve2.Evaluate(time), pullable.pullOutProcess);

                // 创建新的关键帧并添加到插值曲线中
                interpolatedCurve.AddKey(new Keyframe(time, value));
            }

            pcdik.jointScaleScaleCurve = interpolatedCurve;

        } else {
            pcdik.jointScaleScaleCurve = oriJointScaleScaleCurve;
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
