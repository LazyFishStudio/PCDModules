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
	public string defaultAnim = "Walk";

	private PCDAnimator animator;
	private PCDHumanConfig config;

	public PCDSkeleton skeleton => config.skeleton;
	public PCDHuman.AnimSetting animSetting => config.animSetting;
	public PCDHuman.PoseInfo poseInfo => config.poseInfo;

	[SerializeField] private Transform lookAtTarget;
	[SerializeField] private PCDFoot lFoot;
	[SerializeField] private PCDFoot rFoot;
	[SerializeField] private PCDBoneDriver lHand;
	[SerializeField] private PCDBoneDriver rHand;
	[SerializeField] private PCDShoulder lShoulder;
	[SerializeField] private PCDShoulder rShoulder;
	[SerializeField] private PCDBodyHead bodyHead;

	private PCDAnimReader curAnimReader;
	private PCDKFReader curKFReader;
	private Rigidbody rb;
	[SerializeField]
	private string curKeyFrame = "Idle";
	private BoneTransInfo lFootInfo;
	private BoneTransInfo rFootInfo;

	public float rootScale => skeleton.GetBone("Root").transform.localScale.x;
	public float scaleDeltaTime => Time.deltaTime / rootScale * Mathf.Max(poseInfo.speed / animSetting.oriSpeed, 0.5f);
	private bool isAnyFootOutRange => lFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * rootScale || rFoot.GetDisToTargetPos() >= animSetting.stepTriggerDis * rootScale;
    private bool isAnyFootNotReset => isAnyFootOutRange;
    // private bool isAnyFootNotReset => isAnyFootOutRange || Mathf.Abs(skeleton.GetBone("LFoot").transform.position.y - skeleton.GetBone("Root").transform.position.y) > 0.01f || Mathf.Abs(skeleton.GetBone("RFoot").transform.position.y - skeleton.GetBone("Root").transform.position.y) > 0.01f;



	private void Awake() {
		config = GetComponent<PCDHumanConfig>();
		animator = GetComponent<PCDAnimator>();
		rb = GetComponentInParent<Rigidbody>();

		if (skeleton.GetBone("LFoot")) {
			lFoot = new(this, skeleton.GetBone("LFoot"), true);
		}
		if (skeleton.GetBone("RFoot")) {
			rFoot = new(this, skeleton.GetBone("RFoot"), true);
		}
		if (skeleton.GetBone("LHand")) {
			lHand = new PCDBoneDriver(skeleton.GetBone("LHand"), true);
		}
		if (skeleton.GetBone("RHand")) {
			rHand = new PCDBoneDriver(skeleton.GetBone("RHand"), true);
		}
		lShoulder = new PCDShoulder(skeleton.GetBone("LShoulder"), skeleton.GetBone("LHand"), true);
		rShoulder = new PCDShoulder(skeleton.GetBone("RShoulder"), skeleton.GetBone("RHand"), true);
		bodyHead = new PCDBodyHead(this, skeleton.GetBone("Body"), skeleton.GetBone("Head"));
	}

	private void Start() {
		SetAnim(defaultAnim);
		curKFReader = curAnimReader.GetKeyFrameReader(curKeyFrame);
		bodyHead.SetBodyPosLocal(curKFReader.GetBoneInfo("Body").localPosition);
		lHand.SetLocalPosition(curKFReader.GetBoneInfo("LHand").localPosition);
		rHand.SetLocalPosition(curKFReader.GetBoneInfo("RHand").localPosition);
		lFoot.SetLocalPosition(curKFReader.GetBoneInfo("LFoot").localPosition);
		lFoot.SetFootPos(skeleton.GetBone("Root").transform.position + skeleton.GetBone("Root").transform.rotation * curKFReader.GetBoneInfo("LFoot").localPosition);
		rFoot.SetLocalPosition(curKFReader.GetBoneInfo("RFoot").localPosition);
		rFoot.SetFootPos(skeleton.GetBone("Root").transform.position + skeleton.GetBone("Root").transform.rotation * curKFReader.GetBoneInfo("RFoot").localPosition);
	}

	private void UpdateFootTarget() {
		/* Update BoneInfo */
		lFootInfo = GetKFReaderSafe(curKeyFrame).GetBoneInfo("LFoot");
		rFootInfo = GetKFReaderSafe(curKeyFrame).GetBoneInfo("RFoot");

		/* Update Left-Foot */
		UpdateLFootTargetPosToStepTarget();
		lFoot.Update(poseInfo.lFootTargetPos, skeleton.GetBone("Body").transform.rotation * lFootInfo.localRotation);

		/* Update Right-Foot */
		UpdateRFootTargetPosToStepTarget();
		rFoot.Update(poseInfo.rFootTargetPos, skeleton.GetBone("Body").transform.rotation * rFootInfo.localRotation);

		/* Private function implements */
		void UpdateLFootTargetPosToStepTarget() {
			poseInfo.lFootTargetPos = (skeleton.GetBone("Root").transform.position + skeleton.GetBone("Root").transform.rotation * lFootInfo.localPosition).CopySetY(skeleton.GetBone("Root").transform.position.y);
			if (walkState == WalkState.Walking) {
				poseInfo.lFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * skeleton.GetBone("Root").transform.localScale.x;
			}
		}
		void UpdateRFootTargetPosToStepTarget() {
			poseInfo.rFootTargetPos = (skeleton.GetBone("Root").transform.position + skeleton.GetBone("Root").transform.rotation * rFootInfo.localPosition).CopySetY(skeleton.GetBone("Root").transform.position.y);
			if (walkState == WalkState.Walking) {
				poseInfo.rFootTargetPos += poseInfo.moveDir * animSetting.stepTargetOffset * skeleton.GetBone("Root").transform.localScale.x;
			}
		}
	}

	private bool CheckLeftStepFirst() {
		/* ���� Walk ���� Idle ѡ�����ĸ� Pos */
		float lFootToTargetDis = Vector3.Distance(poseInfo.lFootTargetPos, lFoot.curPos);
		float rFootToTargetDis = Vector3.Distance(poseInfo.rFootTargetPos, rFoot.curPos);
		float lFootYOffset = Mathf.Abs(skeleton.GetBone("LFoot").transform.position.y - skeleton.GetBone("Root").transform.position.y);
		float rFootYOffset = Mathf.Abs(skeleton.GetBone("RFoot").transform.position.y - skeleton.GetBone("Root").transform.position.y);

		if (Mathf.Abs(lFootToTargetDis - rFootToTargetDis) < animSetting.stepTargetOffset / 2.0f) {
			return !animSetting.stepRightFootFirst;
		} else {
			return lFootToTargetDis > rFootToTargetDis;
		}
	}

	private void UpdateBodyHeadShoulder() {
		curKFReader = GetKFReaderSafe(curKeyFrame);
		var activeFoot = (curKeyFrame == "Idle" ? null : (curKeyFrame == "LStep" ? lFoot : rFoot));
		bodyHead.UpdateBodyAndHead(curKFReader, activeFoot, lookAtTarget);
		lShoulder.UpdateShoulder();
		rShoulder.UpdateShoulder();
	}

	private void UpdateBodyParts() {
		UpdateFootTarget();
		UpdateBodyHeadShoulder();
	}

	private PCDKFReader GetKFReaderSafe(string kfName) {
		PCDKFReader curKFReader = curAnimReader.GetKeyFrameReader(kfName);
		if (curKFReader == null) {
			return curAnimReader.GetKeyFrameReader("Idle");
		} else {
			return curKFReader;
		}
	}

	private void DriveNextStep(bool isStepLeft) {
		curKFReader = GetKFReaderSafe(curKeyFrame);
		var idleKFReader = curAnimReader.GetKeyFrameReader("Idle");
		if (isStepLeft) {
			lFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				DriveHandToKF(idleKFReader);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			DriveHandToKF();
			// body.FadeBoneToKeyFrame(kfReader, animSetting.handPoseDuration);
		} else {
			rFoot.Step(() => {
				holdTime = 0f;
				walkState = WalkState.Idle;
				DriveHandToKF(idleKFReader);
				// lHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				// rHand.FadeBoneToKeyFrame(idleKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
				// SendMessage("Play", SendMessageOptions.DontRequireReceiver);
			});
			DriveHandToKF();
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
		// Debug.Log("holdTime < animSetting.stepInterval: " + (holdTime < animSetting.stepInterval));
		// Debug.Log("isSpeedSlow: " + isSpeedSlow);
		// Debug.Log("poseInfo.speed < animSetting.stepTriggerSpeed: " + poseInfo.speed + " " + animSetting.stepTriggerSpeed);
		// Debug.Log("isAnyFootNotReset: " + isAnyFootNotReset);

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

	public void SetLookAt(Transform target, float headWeight = -1.0f, float bodyWeight = -1.0f) {
		lookAtTarget = target;
		bodyHead.headLookAtWeight = headWeight >= 0 ? headWeight : animSetting.lookAtWeight_head;
		bodyHead.bodyLookAtWeight = bodyWeight >= 0 ? bodyWeight : animSetting.lookAtWeight_body;
	}

	public void ResetLookAt() {
		lookAtTarget = null;
	}

	public void SetAnim(string animName) {
		PCDAnimReader newAnimReader = animator.GetAnimReader(animName);
		if (newAnimReader == null) {
			return;
		}
		curAnimReader = newAnimReader;
	}

	public void ResetAnimToDefault() {
		SetAnim(defaultAnim);
	}

	public void DriveHandToKF(PCDKFReader targetKFReader = null) {
		if (targetKFReader == null) {
			lHand.FadeBoneToKeyFrame(curKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
			rHand.FadeBoneToKeyFrame(curKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
		} else {
			lHand.FadeBoneToKeyFrame(targetKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
			rHand.FadeBoneToKeyFrame(targetKFReader, animSetting.handPoseDuration, animSetting.handPosCurve);
		}
	}

}