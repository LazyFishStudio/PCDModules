using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Update foot position, left or right foot state, notify other components to play animation
 */
public class PCDWalkMgr : MonoBehaviour
{
	public enum WalkState { Idle, Walking };
	public WalkState walkState = WalkState.Idle;

	private PCDAnimator animator;
	private PCDHumanConfig config;

	public PCDHuman.PCDHumanBoneSetting humanBone => config.humanBone;
	public PCDHuman.AnimSetting animSetting => config.animSetting;
	public PCDHuman.PoseInfo poseInfo => config.poseInfo;

	[SerializeField] private PCDFoot lFoot;
	[SerializeField] private PCDFoot rFoot;
	[SerializeField] private PCDBoneDriver lHand;
	[SerializeField] private PCDBoneDriver rHand;
	[SerializeField] private PCDShoulder lShoulder;
	[SerializeField] private PCDShoulder rShoulder;
	[SerializeField] private PCDBodyHead bodyHead;

	private Rigidbody rb;
	private string curKeyFrame = "Idle";
	private BoneTransInfo lFootInfo;
	private BoneTransInfo rFootInfo;

	public float scaleDeltaTime => Time.deltaTime / humanBone.rootScale * Mathf.Max(poseInfo.speed / animSetting.oriSpeed, 0.5f);
	private bool isAnyFootOutRange => lFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale || rFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale;
    private bool isAnyFootNotReset => isAnyFootOutRange || Mathf.Abs(humanBone.lFoot.position.y - humanBone.root.position.y) > 0.01f || Mathf.Abs(humanBone.rFoot.position.y - humanBone.root.position.y) > 0.01f;

	private void Awake() {
		config = GetComponent<PCDHumanConfig>();
		animator = GetComponent<PCDAnimator>();
		rb = GetComponentInParent<Rigidbody>();

		if (humanBone.isLLegActive) {
			lFoot = new(this, humanBone.lFoot.GetComponent<PCDBone>(), true);
		}
		if (humanBone.isRLegActive) {
			rFoot = new(this, humanBone.rFoot.GetComponent<PCDBone>(), true);
		}
		if (true) {
			lHand = new PCDBoneDriver(humanBone.lHand.GetComponent<PCDBone>(), true);
		}
		if (true) {
			rHand = new PCDBoneDriver(humanBone.rHand.GetComponent<PCDBone>(), true);
		}
		lShoulder = new PCDShoulder(humanBone.lShoulder.GetComponent<PCDBone>(), humanBone.lHand.GetComponent<PCDBone>(), true);
		rShoulder = new PCDShoulder(humanBone.rShoulder.GetComponent<PCDBone>(), humanBone.rHand.GetComponent<PCDBone>(), true);
		bodyHead = new PCDBodyHead(this, humanBone.body.GetComponent<PCDBone>(), humanBone.head.GetComponent<PCDBone>());
	}

	private void UpdateFootTarget() {
		/* Update BoneInfo */
		lFootInfo = animator.GetAnimReader("Walk").GetKeyFrameReader(curKeyFrame).GetBoneInfo("LFoot");
		rFootInfo = animator.GetAnimReader("Walk").GetKeyFrameReader(curKeyFrame).GetBoneInfo("RFoot");

		/* Update Left-Foot */
		UpdateLFootTargetPosToStepTarget();
		lFoot.Update(poseInfo.lFootTargetPos, humanBone.body.rotation * lFootInfo.localRotation);

		/* Update Right-Foot */
		UpdateRFootTargetPosToStepTarget();
		rFoot.Update(poseInfo.rFootTargetPos, humanBone.body.rotation * rFootInfo.localRotation);

		/* Private function implements */
		void UpdateLFootTargetPosToStepTarget() {
			poseInfo.lFootTargetPos = (humanBone.root.position + humanBone.root.rotation * lFootInfo.localPosition).CopySetY(humanBone.root.position.y);
			if (walkState == WalkState.Walking) {
				poseInfo.lFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * humanBone.rootScale;
			}
		}
		void UpdateRFootTargetPosToStepTarget() {
			poseInfo.rFootTargetPos = (humanBone.root.position + humanBone.root.rotation * rFootInfo.localPosition).CopySetY(humanBone.root.position.y);
			if (walkState == WalkState.Walking) {
				poseInfo.rFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * humanBone.rootScale;
			}
		}
	}

	private bool CheckLeftStepFirst() {
		/* ���� Walk ���� Idle ѡ�����ĸ� Pos */
		float lFootToTargetDis = Vector3.Distance(poseInfo.lFootTargetPos, lFoot.curPos);
		float rFootToTargetDis = Vector3.Distance(poseInfo.rFootTargetPos, rFoot.curPos);

		if (Mathf.Abs(lFootToTargetDis - rFootToTargetDis) < animSetting.stepTargetOffset / 2.0f) {
			return !animSetting.stepRightFootFirst;
		} else {
			return lFootToTargetDis > rFootToTargetDis;
		}
	}

	private void UpdateBodyHeadShoulder() {
		var kfReader = animator.GetAnimReader("Walk").GetKeyFrameReader(curKeyFrame);
		var activeFoot = (curKeyFrame == "Idle" ? null : (curKeyFrame == "LStep" ? lFoot : rFoot));
		bodyHead.UpdateBodyAndHead(kfReader, activeFoot);
		lShoulder.UpdateShoulder();
		rShoulder.UpdateShoulder();
	}

	private void UpdateBodyParts() {
		UpdateFootTarget();
		UpdateBodyHeadShoulder();
	}

	private void DriveNextStep(bool isStepLeft) {
		var kfReader = animator.GetAnimReader("Walk").GetKeyFrameReader(curKeyFrame);
		var idleKFReader = animator.GetAnimReader("Walk").GetKeyFrameReader("Idle");
		if (isStepLeft) {
			lFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				lHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				rHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			lHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration, animSetting.handPosCurve);
			rHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration, animSetting.handPosCurve);

			// body.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
		} else {
			rFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				lHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				rHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			lHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration, animSetting.handPosCurve);
			rHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration, animSetting.handPosCurve);
		}
	}

	private float holdTime = 0f;
	private void UpdateWalkLoop() {
		if (walkState == WalkState.Walking) {
			UpdateBodyParts();
			return;
		}

		bool isStepLeft = CheckLeftStepFirst();
		bool isSpeedSlow = poseInfo.speed < animSetting.stepTriggerSpeed;
		curKeyFrame = isSpeedSlow ? "Idle" : (isStepLeft ? "LStep" : "RStep");
		UpdateBodyParts();

		holdTime += scaleDeltaTime;
		if (holdTime < animSetting.stepInterval)
			return;
		if (isSpeedSlow && !isAnyFootNotReset)
			return;

		/* Next step */
		walkState = WalkState.Walking;
		DriveNextStep(isStepLeft);
	}

	private void UpdateVelocityInfo() {
		poseInfo.velocity = rb.velocity;

		Vector3 lastMoveDir = poseInfo.moveDir;
		poseInfo.speed = poseInfo.velocity.magnitude;

		if (poseInfo.speed > 0.01f) {
			poseInfo.moveDir = poseInfo.velocity.ClearY().normalized;
		}
		if (poseInfo.moveDir == Vector3.zero || lastMoveDir == Vector3.zero) {
			poseInfo.turnAngle = 0;
		} else {
			poseInfo.turnAngle = Vector3.Angle(lastMoveDir, poseInfo.moveDir);
		}
	}

	private void Update() {
		UpdateVelocityInfo();
		UpdateWalkLoop();
	}
}