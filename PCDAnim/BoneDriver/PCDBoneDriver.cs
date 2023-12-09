using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bros.Utils;
using DG.Tweening;

public class PCDBoneDriver
{
	public PCDBone attachedBone;

	public PCDBoneDriver(PCDBone bone) {
		attachedBone = bone;
	}

	public virtual bool CheckBoneOwnership() {
		return attachedBone.owner == this;
	}
	public virtual bool TryGetOwnership(bool force = false) {
		if (CheckBoneOwnership())
			return true;
		if (attachedBone.owner != null || !attachedBone.forceOwner) {
			attachedBone.ResetOwnership();
			attachedBone.SetOwnership(this, force);
			return true;
		}
		return false;
	}

	public void FadeBoneToKeyFrame(PCDKFReader keyFrame, float fadeTime = 0.05f, AnimationCurve curve = null) {
		FadeBoneToTransInfo(keyFrame.GetBoneInfo(attachedBone.boneName), fadeTime, curve);
	}

	public void FadeBoneToWorldArchor(Transform target) {
		
	}

	public void FadeBoneToTransInfo(BoneTransInfo info, float fadeTime = 0.05f, AnimationCurve curve = null) {
		if (!CheckBoneOwnership())
			return;

		Debug.Log("DoMove to: " + info);
		attachedBone.transform.DOKill();
		if (curve == null) {
			attachedBone.transform.DOLocalMove(info.localPosition, fadeTime);
			attachedBone.transform.DOLocalRotateQuaternion(info.localRotation, fadeTime);
			attachedBone.transform.DOScale(info.localScale, fadeTime);
		} else {
			attachedBone.transform.DOLocalMove(info.localPosition, fadeTime).SetEase(curve);
			attachedBone.transform.DOLocalRotateQuaternion(info.localRotation, fadeTime).SetEase(curve);
			attachedBone.transform.DOScale(info.localScale, fadeTime).SetEase(curve);
		}
	}
}
