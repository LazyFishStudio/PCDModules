using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDBodyHead
{
    private PCDWalkMgr walkMgr;
    private PCDHuman.PoseInfo poseInfo;
    private PCDHuman.PCDHumanBoneSetting humanBone;
    private PCDBoneDriver bodyDriver;
    private PCDBoneDriver headDriver;

    public PCDBodyHead(PCDWalkMgr walkMgr, PCDBone body, PCDBone head) {
        this.walkMgr = walkMgr;
        poseInfo = walkMgr.poseInfo;
        humanBone = walkMgr.humanBone;
        bodyDriver = new PCDBoneDriver(body, true);
        headDriver = new PCDBoneDriver(head, true);
    }

    private Vector3 bodyKFPosLocal;
    private Vector3 bodyTargetPosLocal;
    private Vector3 bodyPosLocalRes;
    private Quaternion bodyFKRotLocal;
    private Quaternion bodyTargetRotLocal;
    private Quaternion bodyRotLocalRes;

    public void UpdateBodyAndHead(PCDKFReader kfReader, PCDFoot activeFoot) {
        /*
         * P1: 从 KeyFrame 读取基本位置和旋转 -> Animation & (Idle / LStep / RStep)
         * P2：根据 LStep 还是 RStep，获取 Foot -> 计算 targetPos
         * P3: 根据走路姿态，计算 Roll 旋转偏移量
         * P4: 根据 targetPos -> 弹簧/Lerp 到这个地方，计算实际位置
         */
        bodyKFPosLocal = kfReader.GetBoneInfo("Body").localPosition;
        bodyFKRotLocal = kfReader.GetBoneInfo("Body").localRotation;

        bodyTargetPosLocal = bodyKFPosLocal;
        if (activeFoot != null) {
            bodyTargetPosLocal = bodyKFPosLocal + Vector3.up * walkMgr.animSetting.bodyHeightCurve.Evaluate(activeFoot.GetProcess());
        }

        bodyTargetRotLocal = bodyFKRotLocal;
        if (true) {//Vector3.Angle(poseInfo.moveDir, humanBone.root.forward) < 180.1f) {
            float bodyRollProcess = Mathf.Clamp((Mathf.Abs(Vector3.SignedAngle(poseInfo.moveDir, humanBone.root.forward, Vector3.up)) * 1.5f) / 120.0f, 0, 1.0f);
            bodyRollProcess = Mathf.Sin(bodyRollProcess * Mathf.PI);
            bodyRollProcess = poseInfo.speed < 0.5f ? 0 : bodyRollProcess;
            float bodyRollSign = Mathf.Sign(Vector3.SignedAngle(poseInfo.moveDir, humanBone.root.forward, Vector3.up));
            Quaternion bodyRoll = Quaternion.AngleAxis(bodyRollSign * bodyRollProcess * walkMgr.animSetting.bodyRollAngle, Vector3.forward);
            bodyTargetRotLocal = bodyRoll * bodyFKRotLocal;
        }
        bodyRotLocalRes = Quaternion.Slerp(humanBone.body.localRotation, bodyTargetRotLocal, Time.deltaTime * walkMgr.animSetting.bodyRotSpeed);

        float toTargetY = bodyTargetPosLocal.y - bodyPosLocalRes.y;
        float dragForceY = toTargetY * walkMgr.animSetting.bodyDragStrength;
        float dampForceY = -poseInfo.bodyVelocity.y * walkMgr.animSetting.bodyVelocityDamp;
        dragForceY += dampForceY;
        // poseInfo.bodyVelocity += Vector3.up * dragForceY * Time.deltaTime;
        poseInfo.bodyVelocity += Vector3.up * dragForceY * Mathf.Min(Time.deltaTime, 0.0125f);
        poseInfo.bodyVelocity = poseInfo.bodyVelocity.normalized * Mathf.Min(30.0f, poseInfo.bodyVelocity.magnitude);
        bodyPosLocalRes += poseInfo.bodyVelocity * Mathf.Min(Time.deltaTime, 0.0125f);

        /* Actual setup value */
        bodyDriver.SetLocalPosition(bodyPosLocalRes);
        bodyDriver.SetLocalRotation(bodyRotLocalRes);
    }
}
