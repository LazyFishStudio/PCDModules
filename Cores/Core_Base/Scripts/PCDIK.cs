using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class PCDIK : MonoBehaviour {

    public Transform root;
    public Transform startBone;
    public Transform endBone;
    public Transform start;
    public Transform end;
    public Transform[] joints;
    public AnimationCurve jointScaleCurve;
    public AnimationCurve jointScaleScaleCurve;
    public int jointNum => joints.Length;
    public float rootScale => root.localScale.x;

    [Range(0.1f, 0.9f)]
    public float midPointProp = 0.5f;
    [SerializeField]
    private float oriLength;
    [SerializeField]
    private float oriLengthScale;
    [SerializeField]
    private float oriStartLength => oriLengthScale * midPointProp;
    [SerializeField]
    private float oriEndLength => oriLengthScale * (1.0f - midPointProp);

    public Vector2 bezierPointOffset = new Vector2(0, 0);

    public Vector3 jointTargetDir => Vector3.Dot(startToEnd, endBone.forward) > 0 ? Vector3.Cross(startToEnd, endBone.right).normalized : Vector3.Cross(endBone.right, -startToEnd).normalized;
    public Vector3 startPoint => startBone.position;
    public Vector3 endPoint => endBone.position;
    public Vector3 midPoint => Vector3.Lerp(startBone.position, endBone.position, midPointProp);
    public Vector3 startToEnd => endBone.position - startBone.position;
    public float startLength => startToEnd.magnitude * midPointProp;
    public float endLength => startToEnd.magnitude * midPointProp;

    void Awake() {
        if (oriLength == 0)
            oriLength = startToEnd.magnitude;

    }

    void LateUpdate() {

        oriLengthScale = rootScale * oriLength;

        if (jointNum == 0) {
            OneBoneIK();
        } else if (jointNum == 1) {
            TwoBoneIK();
        } else {
            BezierCurveIK();
        }
        
    }

    private void OneBoneIK() {
        start.position = startPoint;
        start.rotation = startBone.rotation;


        end.position = endPoint;
        end.rotation = startBone.rotation;

        start.localScale = Vector3.one * jointScaleCurve.Evaluate(0) * jointScaleScaleCurve.Evaluate(0);
        end.localScale = Vector3.one * jointScaleCurve.Evaluate(1) * jointScaleScaleCurve.Evaluate(1);

    }

    private void TwoBoneIK() {

        Transform joint = joints[0];

        if (startToEnd.magnitude >= oriLengthScale) {
            
            joint.position = midPoint;
            joint.rotation = startBone.rotation;

            start.position = startPoint;
            start.rotation = startBone.rotation;

            end.position = endPoint;
            end.rotation = startBone.rotation;

            joint.localScale = Vector3.one * jointScaleCurve.Evaluate(midPointProp) * jointScaleScaleCurve.Evaluate(midPointProp);

            return;

        }

        joint.position = GetTwoBoneIKJointPos();
        joint.rotation = Quaternion.LookRotation(startToEnd, jointTargetDir);

        start.position = startPoint;
        start.rotation = Quaternion.LookRotation(joint.position - start.position, Vector3.Cross(joint.position - start.position, joint.right));

        end.position = endPoint;
        end.rotation = Quaternion.LookRotation(end.position - joint.position, Vector3.Cross(end.position - joint.position, joint.right));

        start.localScale = Vector3.one * jointScaleCurve.Evaluate(0) * jointScaleScaleCurve.Evaluate(0);
        end.localScale = Vector3.one * jointScaleCurve.Evaluate(1) * jointScaleScaleCurve.Evaluate(1);

    }

    private void BezierCurveIK() {

        if (startToEnd.magnitude >= oriLengthScale) {
            
            float jointSpace = startToEnd.magnitude / (joints.Length + 1);
            for (int i = 0; i < joints.Length; i++) {
                joints[i].position = startPoint + startToEnd.normalized * (i + 1) * jointSpace;
                joints[i].rotation = Quaternion.LookRotation(startToEnd, Vector3.up);
                joints[i].localScale = Vector3.one * jointScaleCurve.Evaluate((i + 1.0f) / (joints.Length + 2.0f)) * jointScaleScaleCurve.Evaluate((i + 1.0f) / (joints.Length + 2.0f));
            }

            start.position = startPoint;
            start.rotation = Quaternion.LookRotation(joints[0].position - start.position, Vector3.Cross(joints[0].position - start.position, joints[0].right));

            end.position = endPoint;
            end.rotation = Quaternion.LookRotation(end.position - joints[joints.Length - 1].position, Vector3.Cross(end.position - joints[joints.Length - 1].position, joints[joints.Length - 1].right));

            start.localScale = Vector3.one * jointScaleCurve.Evaluate(0) * jointScaleScaleCurve.Evaluate(0);
            end.localScale = Vector3.one * jointScaleCurve.Evaluate(1) * jointScaleScaleCurve.Evaluate(1);

            return;

        }

        Vector3 jointPos = GetTwoBoneIKJointPos();
        Vector3[] bezierPoints = new Vector3[] {
            startPoint,
            jointPos - startToEnd.normalized * bezierPointOffset.x * rootScale,
            jointPos + startToEnd.normalized * bezierPointOffset.y * rootScale,
            endPoint
        };

        Debug.DrawLine(bezierPoints[0], bezierPoints[1]);
        Debug.DrawLine(bezierPoints[1], bezierPoints[2]);
        Debug.DrawLine(bezierPoints[2], bezierPoints[3]);
    
        float dt = 1.0f / (joints.Length + 1);

        for (int i = 0; i < joints.Length; i++) {
            Vector3 curJointPos = GetBezierPoint(bezierPoints, dt * (i + 1));
            Vector3 lastJointPos = GetBezierPoint(bezierPoints, dt * i);
            Vector3 nextJointPos = GetBezierPoint(bezierPoints, dt * (i + 2));
            Vector3 lastToCur = curJointPos - lastJointPos;
            Vector3 curToNext = nextJointPos - curJointPos;
            joints[i].position = curJointPos;
            joints[i].rotation = Quaternion.LookRotation(lastToCur + curToNext, jointTargetDir);
            joints[i].localScale = Vector3.one * jointScaleCurve.Evaluate((i + 1.0f) / (joints.Length + 2.0f)) * jointScaleScaleCurve.Evaluate((i + 1.0f) / (joints.Length + 2.0f));
        }

        start.position = startPoint;
        start.rotation = Quaternion.LookRotation(joints[0].position - start.position, Vector3.Cross(joints[0].position - start.position, joints[0].right));

        end.position = endPoint;
        end.rotation = Quaternion.LookRotation(end.position - joints[joints.Length - 1].position, Vector3.Cross(end.position - joints[joints.Length - 1].position, joints[joints.Length - 1].right));

        start.localScale = Vector3.one * jointScaleCurve.Evaluate(0) * jointScaleScaleCurve.Evaluate(0);
        end.localScale = Vector3.one * jointScaleCurve.Evaluate(1) * jointScaleScaleCurve.Evaluate(1);

    }

    public Vector3 GetTwoBoneIKJointPos() {
        float cosA = startLength / oriStartLength;
        return midPoint + jointTargetDir * oriStartLength * Mathf.Sin(Mathf.Acos(cosA));
    }

    private Vector3 GetBezierPoint(Vector3[] bezierPoints, float t) {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 point = uuu * bezierPoints[0] + 3 * uu * t * bezierPoints[1] + 3 * u * tt * bezierPoints[2] + ttt * bezierPoints[3];
        return point;
    }

}
