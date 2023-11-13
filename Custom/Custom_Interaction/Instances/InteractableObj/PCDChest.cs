using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDChest : PCDTriggerInteractable {
    public string chestTag;
    public Transform chestLid;
    public GameObject[] inChestItemsPrefabs;
    public GameObject chestOpenEffectPrefab;
    private bool isOpened;

    public override bool CheckInteractCond(InteractComp interactor) {
        if (interactor.holdingItem == null)
            return false;

        PCDKey key = interactor.holdingItem.GetComponent<PCDKey>();
        return key && key.keyTag == chestTag;
    }

    public override bool OnInteract(InteractComp interactor) { 
        Open();
        return true;
    }

    public void Open() {
        if (isOpened) {
            return;
        }
        if (chestLid) {
            chestLid.localEulerAngles = new Vector3(-75.0f, 0, 0);
        }
        if (inChestItemsPrefabs != null && inChestItemsPrefabs.Length > 0) {
            for (int i = 0; i < inChestItemsPrefabs.Length; i++) {
                GameObject.Instantiate(inChestItemsPrefabs[i], transform.position + transform.forward + Vector3.up, transform.rotation);
            }
        }
        if (chestOpenEffectPrefab) {
            GameObject.Instantiate(chestOpenEffectPrefab, transform.position, transform.rotation);
        }
        isOpened = true;
    }
}
