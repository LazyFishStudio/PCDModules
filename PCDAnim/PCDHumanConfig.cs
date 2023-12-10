using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanConfig : MonoBehaviour
{
	public bool	initSkeleton;
    public PCDSkeleton skeleton;
	public PCDHuman.AnimSetting animSetting;
	public PCDHuman.PoseInfo poseInfo;

	private void Awake() {
		InitBones();
	}

	private void OnValidate() {
		if (initSkeleton) {
			InitBones();
			initSkeleton = false;
		}
	}

	public void InitBones() {
		skeleton = new();
		PCDBone[] pcdBones = transform.GetComponentsInChildrenAndChildren<PCDBone>();
		
		string skeletonLog = gameObject.name + " PCDSkeleton Init Finish, BoneList: \n";
		foreach (PCDBone pcdBone in pcdBones) {
			skeleton.AddBone(pcdBone);
			skeletonLog += pcdBone.boneName + '\n';
		}
		Debug.Log(skeletonLog);
    }
}
