using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class PCDGardenEel : PullablePCDIKController {
    public Transform hole;
    public Vector2 runAwayTriggerDisRange = new Vector2(6.0f, 3.0f);
    public Vector3 runAwayOffset = new Vector3(0, 0.75f, 1.0f);

    void Awake() {
        hole.SetParent(null);
    }

    void Update() {

        neckRoot.rotation = Quaternion.LookRotation((head.position - neckRoot.position).ClearY(), Vector3.up);

        if (lookAtTarget) {

            float toHeadLookAtTargetDisScale = Vector3.Distance(lookAtTarget.position, transform.position) * lengthScale;
            float lookAtTriggerDisScale = lookAtTriggerDis * lengthScale;
            Vector2 runAwayTriggerDisScale = runAwayTriggerDisRange * lengthScale;

            if (toHeadLookAtTargetDisScale < runAwayTriggerDisScale.x) {

                float process = Mathf.Min(1.0f, (runAwayTriggerDisScale.x - toHeadLookAtTargetDisScale) / (runAwayTriggerDisScale.x - runAwayTriggerDisScale.y));
                posOffsetScale = (transform.position - lookAtTarget.position).ClearY().normalized * runAwayOffset.z + Vector3.up * runAwayOffset.y;
                posOffsetScale *= process * lengthScale;

            } else {
                posOffsetScale = Vector3.zero;
            }


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

}
