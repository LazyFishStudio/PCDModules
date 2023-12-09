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
	public PCDAnimator animator;
	public PCDHuman.PCDHumanBoneSetting humanBone;
	public PCDHuman.AnimSetting animSetting;

	public PCDHuman.PoseInfo poseInfo;
	[SerializeField] private PCDFoot lFoot;
	[SerializeField] private PCDFoot rFoot;
	[SerializeField] private PCDBoneDriver lHand;
	[SerializeField] private PCDBoneDriver rHand;
	[SerializeField] private PCDBoneDriver body;

	private Rigidbody rb;
	private string curKeyFrame = "Idle";
	private BoneTransInfo lFootInfo;
	private BoneTransInfo rFootInfo;

	public float scaleDeltaTime => Time.deltaTime / humanBone.rootScale * Mathf.Max(poseInfo.speed / animSetting.oriSpeed, 0.5f);
	private bool isAnyFootOutRange => lFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale || rFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * humanBone.rootScale;
    private bool isAnyFootNotReset => isAnyFootOutRange || Mathf.Abs(humanBone.lFoot.position.y - humanBone.root.position.y) > 0.01f || Mathf.Abs(humanBone.rFoot.position.y - humanBone.root.position.y) > 0.01f;

	private void Awake() {
		animator = GetComponent<PCDAnimator>();
		rb = GetComponentInParent<Rigidbody>();

		if (humanBone.isLLegActive) {
			lFoot = new(this, humanBone.lFoot.GetComponent<PCDBone>());
		}
		if (humanBone.isRLegActive) {
			rFoot = new(this, humanBone.rFoot.GetComponent<PCDBone>());
		}
		if (true) {
			lHand = new PCDBoneDriver(humanBone.lHand.GetComponent<PCDBone>());
			lHand.TryGetOwnership();
		}
		if (true) {
			rHand = new PCDBoneDriver(humanBone.rHand.GetComponent<PCDBone>());
			rHand.TryGetOwnership();
		}
		body = new PCDBoneDriver(humanBone.body.GetComponent<PCDBone>());
		body.TryGetOwnership();
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

	private float holdTime = 0f;

	private void UpdateWalkLoop() {
		if (walkState == WalkState.Walking) {
			UpdateFootTarget();
			return;
		}

		bool isSpeedSlow = poseInfo.speed < animSetting.stepTriggerSpeed;
		/* Too slow and all foot reset, idle -> just return */
		if (isSpeedSlow && !isAnyFootNotReset) {
			UpdateFootTarget();
			return;
		}

		/* Step Interval */
		holdTime += scaleDeltaTime;
		if (holdTime < animSetting.stepInterval) {
			UpdateFootTarget();
			return;
		}

		/* Update KeyFrame information */
		bool isStepLeft = CheckLeftStepFirst();
		curKeyFrame = isSpeedSlow ? "Idle" : (isStepLeft ? "LStep" : "RStep");

		/* Calculate targetPos */
		UpdateFootTarget();

		/* Drive body */
		walkState = WalkState.Walking;
		var kfReader = animator.GetAnimReader("Walk").GetKeyFrameReader(curKeyFrame);
		var idleKFReader = animator.GetAnimReader("Walk").GetKeyFrameReader("Idle");
		if (isStepLeft) {
			lFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				lHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				rHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				body.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			lHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
			rHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
			body.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
		} else {
			rFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				lHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				rHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				body.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			lHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
			rHand.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
			body.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
		}
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