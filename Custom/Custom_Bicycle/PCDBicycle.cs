using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDBicycle : MonoBehaviour {
    public PCDHuman PCDHuman;
    public Transform frontWheel;
    public float wheelLerpSpeed = 5.0f;
    public float wheelYawMax = 75.0f;

    public Transform pedalAxle;
    public float rotateSpeed = 720.0f;
    private float curRotate;

    void Update() {

        if (Vector3.Angle(PCDHuman.poseInfo.moveDir, PCDHuman.humanBone.root.forward) < 180.0f) {
            float wheelYaw = Mathf.Clamp(Mathf.Abs(Vector3.SignedAngle(PCDHuman.poseInfo.moveDir, PCDHuman.humanBone.root.forward, Vector3.up)) - 1f, 0, wheelYawMax);
            float wheelYawSign = -Mathf.Sign(Vector3.SignedAngle(PCDHuman.poseInfo.moveDir, PCDHuman.humanBone.root.forward, Vector3.up));
            Quaternion wheelRot = Quaternion.AngleAxis(wheelYawSign * wheelYaw, Vector3.up);
            frontWheel.localRotation = Quaternion.Slerp(frontWheel.localRotation, wheelRot, wheelLerpSpeed * Time.deltaTime);
        }

        curRotate = (curRotate + rotateSpeed * Time.deltaTime * PCDHuman.poseInfo.speed / PCDHuman.animSetting.oriSpeed) % 360.0f;
        pedalAxle.localEulerAngles = new Vector3(curRotate, 0, 0);

    }
}
