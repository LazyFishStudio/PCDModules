using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bros.Utils;
using DG.Tweening;

public class PCDBoneDriver
{
	public PCDBone attachedBone;
	private PCDBoneDriver prevOwner;
	private BoneTransInfo lastKFBoneTransInfo;

	public PCDBoneDriver(PCDBone bone, bool autoTryGetOwnship = false) {
		attachedBone = bone;
		if (autoTryGetOwnship)
			TryGetOwnership();
	}

	public virtual bool CheckBoneOwnership() {
		return attachedBone.owner == this;
	}
	public virtual bool TryGetOwnership(bool force = false) {
		if (CheckBoneOwnership())
			return true;
		if (attachedBone.owner == null || !attachedBone.forceOwner) {
			prevOwner = attachedBone.owner;

			attachedBone.ResetOwnership();
			attachedBone.SetOwnership(this, force);
			this.OnGetOwnership();
			return true;
		}
		return false;
	}

	public virtual void ReturnOwnership() {
		if (attachedBone.owner != this)
			return;
		attachedBone.owner = prevOwner;
		attachedBone.forceOwner = false;

		prevOwner?.OnGetOwnership();
	}

	public void SetGlobalPosition(Vector3 position) {
		if (!CheckBoneOwnership())
			return;
		attachedBone.transform.position = position;
	}

	public void SetGlobalRotation(Quaternion rotation) {
		if (!CheckBoneOwnership())
			return;
		attachedBone.transform.rotation = rotation;
	}

	public void SetLocalPosition(Vector3 localPosition) {
		if (!CheckBoneOwnership())
			return;
		attachedBone.transform.localPosition = localPosition;
	}

	public void SetLocalRotation(Quaternion localRotation) {
		if (!CheckBoneOwnership())
			return;
		attachedBone.transform.localRotation = localRotation;
	}

	public void FadeBoneToKeyFrame(PCDKFReader keyFrame, float fadeTime = 0.05f, AnimationCurve curve = null) {
		FadeBoneToTransInfo(keyFrame.GetBoneInfo(attachedBone.boneName), fadeTime, curve);
	}

	public void FadeBoneToWorldArchor(Transform target) {
		
	}

	public void FadeBoneToTransInfo(BoneTransInfo info, float fadeTime = 0.05f, AnimationCurve curve = null) {
		if (!CheckBoneOwnership())
			return;
		
		float curPosToTargetDis = Vector3.Distance(info.localPosition, attachedBone.transform.localPosition);
		float lastPosePosToTargetDis = Vector3.Distance(info.localPosition, lastKFBoneTransInfo.localPosition);

		if (lastPosePosToTargetDis > 0.0001f) {
			float scaleFadeTime = fadeTime * (curPosToTargetDis / lastPosePosToTargetDis);
			fadeTime = (scaleFadeTime < 0.01f || scaleFadeTime > fadeTime) ? fadeTime : scaleFadeTime;
		}


		attachedBone.transform.DOKill();
		// if (fadeTime <= 0) {
			
		// } else {
			if (curve == null) {
				attachedBone.transform.DOLocalMove(info.localPosition, fadeTime);
				attachedBone.transform.DOLocalRotateQuaternion(info.localRotation, fadeTime);
				attachedBone.transform.DOScale(info.localScale, fadeTime);
			} else {
				attachedBone.transform.DOLocalMove(info.localPosition, fadeTime).SetEase(curve);
				attachedBone.transform.DOLocalRotateQuaternion(info.localRotation, fadeTime).SetEase(curve);
				attachedBone.transform.DOScale(info.localScale, fadeTime).SetEase(curve);
			}
		// }
		lastKFBoneTransInfo = info;
	}

	public virtual void OnGetOwnership() {
		
	}

}
