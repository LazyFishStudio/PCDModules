using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PCDSkeleton))]
public class PCDHumanConfig : MonoBehaviour
{
    public PCDSkeleton skeleton;
	public PCDHuman.AnimSetting animSetting;
	public PCDHuman.PoseInfo poseInfo;

	private void Awake() {
		skeleton = GetComponent<PCDSkeleton>();
	}

	private void OnValidate() {
		skeleton = GetComponent<PCDSkeleton>();
	}

}
