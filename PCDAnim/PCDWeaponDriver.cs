using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PCDWeaponDriver : PCDBoneDriver
{
    public Transform weaponBoneTarget;
    public PCDWeaponDriver(PCDBone weaponBone, Transform weaponBoneTarget = null, bool autoTryGetOwnship = false) : base(weaponBone, autoTryGetOwnship) {
		this.weaponBoneTarget = weaponBoneTarget;
	}

    public override void OnGetOwnership() {
        attachedBone.transform.SetParent(weaponBoneTarget);
        attachedBone.transform.localPosition = Vector3.zero;
        attachedBone.transform.localRotation = Quaternion.identity;
    }

}
