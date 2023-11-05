using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableWeapon : PickableObject {
    public override void OnPicked() {
        PCDWeapon weapon = GetComponent<PCDWeapon>();
        weapon.SetOwner(picker.transform);
    }
}
