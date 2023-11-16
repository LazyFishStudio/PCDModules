using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDWeapon : MonoBehaviour {
    public string interactDesc;
    public Transform owner;
    public PCDWeaponProperties.ToolType toolType = PCDWeaponProperties.ToolType.Null;
    public PCDWeaponProperties.WeaponSharpness sharpness = PCDWeaponProperties.WeaponSharpness.Dull;
    public GameObject attackAnimPrefab;
    public DamageArea[] damageAreas;

    void Awake() {
        damageAreas = GetComponentsInChildren<DamageArea>();
    }

    public virtual void OnAttackStart(Transform attacker) {
        // Debug.Log("attack start");
        GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(true);
        SendMessage("Play", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void OnAttackEnd(Transform attacker) {
        // Debug.Log("attack end");
        GetComponentInChildren<DamageArea>()?.SetDamageDetectActive(false);
    }

    public void SetOwner(Transform newOwner) {
        if (newOwner == null) {
            owner = null;
            SetDamageAreaAttacker(null);
            return;
        }

        owner = newOwner;
        SetDamageAreaAttacker(newOwner);

    }

    private void SetDamageAreaAttacker(Transform attacker) {
        if (damageAreas == null) {
            return;
        }
        foreach (DamageArea area in damageAreas) {
            area.attacker = attacker;
        }
    }

}
