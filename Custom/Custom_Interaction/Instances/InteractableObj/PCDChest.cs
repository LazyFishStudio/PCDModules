using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDChest : PCDInteractable {
    public string chestTag;
    public Transform chestLid;
    public GameObject[] inChestItemsPrefabs;
    public GameObject chestOpenEffectPrefab;
    private bool isOpened;
    public override bool CheckInteractCond(InteractComp interactor) { 
        if (!interactor.holdingItem) {
            return false;
        }
        PCDKey key = interactor.holdingItem.GetComponent<PCDKey>();
        if (!key) {
            return false;
        }
        return key.keyTag == chestTag;
    }

    public override bool OnInteractStay(InteractComp interactor) { 
        return true; 
    }

    public override void OnInteractFinish(InteractComp interactor) {
        Open();
        this.interactor = null;
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

    public void Lock() {
        if (!isOpened) {
            return;
        }
        if (chestLid) {
            chestLid.localEulerAngles = Vector3.zero;
        }
    }

}
