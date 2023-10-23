using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDWeapon : MonoBehaviour {
    public PCDWeaponProperties.ToolType toolType = PCDWeaponProperties.ToolType.Null;
    public PCDWeaponProperties.WeaponSharpness sharpness = PCDWeaponProperties.WeaponSharpness.Dull;
    public GameObject attackAnimPrefab;
    public virtual void OnAttackStart(Transform attacker) {
        // Debug.Log("attack start");
        GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(true);
        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void OnAttackEnd(Transform attacker) {
        // Debug.Log("attack end");
        GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(false);
    }
}
