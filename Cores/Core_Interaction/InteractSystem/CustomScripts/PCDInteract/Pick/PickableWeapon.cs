using System.Collections;
using System.Collections.Generic;
using InteractSystem;
using UnityEngine;

public class PickableWeapon : PickableObject {
    public override void OnPickedBy(InteractComp interactor) {
        PCDWeapon weapon = GetComponent<PCDWeapon>();
        weapon?.SetOwner(picker.transform);
    }
}
