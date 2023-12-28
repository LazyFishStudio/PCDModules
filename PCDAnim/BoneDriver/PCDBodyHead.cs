using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDBodyHead
{
    public float headLookAtWeight;
    public float bodyLookAtWeight;
    private PCDHumanMgr PCDHuman;
    private PCDWalkMgr walkMgr => PCDHuman.walkMgr;
    private PCDHuman.PoseInfo poseInfo;
    private PCDSkeleton skeleton;
    private PCDBoneDriver bodyDriver;

    public PCDBodyHead(PCDWalkMgr walkMgr, PCDBone body) {
        PCDHuman = walkMgr.GetComponent<PCDHumanMgr>();
        poseInfo = walkMgr.poseInfo;
        skeleton = walkMgr.skeleton;
        bodyDriver = new PCDBoneDriver(body, true);
    }

    private Vector3 bodyKFPosLocal;
    private Vector3 bodyTargetPosLocal;
    private Vector3 bodyPosLocalRes;
    private Quaternion bodyFKRotLocal;
    private Quaternion bodyTargetRotLocal;
    private Quaternion bodyRotLocalRes;

    public void SetBodyPosLocal(Vector3 bodyPosLocal) {
        bodyTargetPosLocal = bodyPosLocal;
    }

    public void UpdateBodyAndHead(PCDKFReader kfReader, PCDFoot activeFoot, Transform lookAtTarget) {
        /*
         * P1: 从 KeyFrame 读取基本位置和旋转 -> Animation & (Idle / LStep / RStep)
         * P2：根据 LStep 还是 RStep，获取 Foot -> 计算 targetPos
         * P3: 根据走路姿态，计算 Roll 旋转偏移量
         * P4: 根据 targetPos -> 弹簧/Lerp 到这个地方，计算实际位置
         */

        PCDBone rootBone = skeleton.GetBone("Root");
        PCDBone bodyBone = skeleton.GetBone("Body");

        bodyKFPosLocal = kfReader.GetBoneInfo("Body").localPosition;
        bodyFKRotLocal = kfReader.GetBoneInfo("Body").localRotation;

        /* BODYPOS: Body UpDown */
        bodyTargetPosLocal = bodyKFPosLocal;
        if (activeFoot != null) {
            bodyTargetPosLocal = bodyKFPosLocal + Vector3.up * walkMgr.animSetting.bodyHeightCurve.Evaluate(activeFoot.GetProcess());
        }

        /* BODYROT: Body Roll  */
        bodyTargetRotLocal = bodyFKRotLocal;
        if (Vector3.Angle(poseInfo.moveDir, rootBone.transform.forward) < 180.1f) {
            float bodyRollProcess = Mathf.Clamp((Mathf.Abs(Vector3.SignedAngle(poseInfo.moveDir, rootBone.transform.forward, Vector3.up)) * 1.5f) / 120.0f, 0, 1.0f);
            bodyRollProcess = Mathf.Sin(bodyRollProcess * Mathf.PI);
            bodyRollProcess = poseInfo.speed < 0.5f ? 0 : bodyRollProcess;
            float bodyRollSign = Mathf.Sign(Vector3.SignedAngle(poseInfo.moveDir, rootBone.transform.forward, Vector3.up));
            Quaternion bodyRoll = Quaternion.AngleAxis(bodyRollSign * bodyRollProcess * walkMgr.animSetting.bodyRollAngle, Vector3.forward);
            bodyTargetRotLocal = bodyRoll * bodyFKRotLocal;
        }

        /* BODYROT: Body Rotate To LookAtTarget At Yaw */
        if (lookAtTarget) {
            Vector3 toLookAtTargetLocal = Quaternion.Inverse(rootBone.transform.rotation) * (lookAtTarget.position - rootBone.transform.position).ClearY();
            bodyTargetRotLocal = Quaternion.Lerp(bodyTargetRotLocal, Quaternion.LookRotation(toLookAtTargetLocal, Vector3.up), walkMgr.animSetting.lookAtWeight_body);
        } 

        /* HEADROT: Head Look At To Target */
        PCDBone headBone = skeleton.GetBone("Head");
        if (headBone) {
            Vector3 toTargetWeight;
            if (lookAtTarget) {
                toTargetWeight = Vector3.Lerp(bodyBone.transform.forward, lookAtTarget.position - headBone.transform.position, walkMgr.animSetting.lookAtWeight_head);
                headBone.transform.rotation = Quaternion.Slerp(headBone.transform.rotation, Quaternion.LookRotation(toTargetWeight, bodyBone.transform.up), Time.deltaTime * walkMgr.animSetting.bodyRotSpeed);
            } else {
                toTargetWeight = Vector3.Lerp(bodyBone.transform.forward, rootBone.transform.forward, PCDHuman.humanConfig.animSetting.lookAtWeight_head);
                headBone.transform.rotation = Quaternion.Slerp(headBone.transform.rotation, Quaternion.LookRotation(toTargetWeight, bodyBone.transform.up), Time.deltaTime * walkMgr.animSetting.bodyRotSpeed * 5.0f);
            }
        }

        /* BODYPOS: Body Offset XZ */
        Vector3 bodyTargetOffsetLocal = bodyKFPosLocal.ClearY();
        poseInfo.bodyOffsetLocal = Vector3.Lerp(poseInfo.bodyOffsetLocal, bodyTargetOffsetLocal, Time.deltaTime * walkMgr.animSetting.bodyOffsetSpeed);

        /* BODYPOS: Body Sprint */
        float toTargetY = bodyTargetPosLocal.y - bodyPosLocalRes.y;
        float dragForceY = toTargetY * walkMgr.animSetting.bodyDragStrength;
        float dampForceY = -poseInfo.bodyVelocity.y * walkMgr.animSetting.bodyVelocityDamp;
        dragForceY += dampForceY;
        poseInfo.bodyVelocity += Vector3.up * dragForceY * Mathf.Min(Time.deltaTime, 0.0125f);
        poseInfo.bodyVelocity = poseInfo.bodyVelocity.normalized * Mathf.Min(30.0f, poseInfo.bodyVelocity.magnitude);

        /* RES: Get Body Rot Res */
        bodyRotLocalRes = Quaternion.Slerp(bodyBone.transform.localRotation, bodyTargetRotLocal, Time.deltaTime * walkMgr.animSetting.bodyRotSpeed);
        /* RES: Get Body Rot Res */
        bodyPosLocalRes += poseInfo.bodyVelocity * Mathf.Min(Time.deltaTime, 0.0125f);

        /* Actual setup value */
        bodyDriver.SetLocalPosition(bodyPosLocalRes + poseInfo.bodyOffsetLocal);
        bodyDriver.SetLocalRotation(bodyRotLocalRes);
    }

}
