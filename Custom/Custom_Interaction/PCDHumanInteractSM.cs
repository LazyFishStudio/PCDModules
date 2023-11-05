using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Bros.Utils;
using Unity.Mathematics;
using UnityEngine;
using InteractSystem;
// using Bros.Utils;
// using DG.Tweening;

public class PCDHumanInteractSM : InteractComp
{
    public Transform controllerLookAtTarget;
    public bool inverseControllerLookAtDir;

    public PCDHumanHoldAndAttackSM holdAndDropSM;
    public PCDHumanPullSM pullSM;

    private PhysicsBasedCharacterController.CharacterManager characterManager {
        get => GetComponent<PhysicsBasedCharacterController.CharacterManager>();
    }
    public PullableObject pullingObject {
        get { return holdingItem == null ? null : holdingItem.GetComponent<PullableObject>(); }
    }
    public PickableObject holdingObject {
        get {
            if (holdingItem == null || pullingObject != null)
                return null;
            return holdingItem.GetComponent<PickableObject>();
        }
    }

    public void OnFocusEnter(Focusable focusing) {
        InteractLogger.LogInteractEvent("FocusEnter", gameObject, focusing.gameObject);

        PullableObject pullableObj = focusing.GetComponent<PullableObject>();
        if (pullableObj != null) {
            pullSM?.PullObjHover(pullableObj, pullableObj.shape);
        }

        /* 设置槽位预览 */
        PCDItemSlot slot = focusing.GetComponent<PCDItemSlot>();
        if (slot != null && slot.dynamicPreview && holdingItem != null)
            slot.SetupPreview(holdingItem);
    }
    public void OnFocusExit(Focusable focusing) {
        InteractLogger.LogInteractEvent("FocusExit", gameObject, focusing.gameObject);

        PullableObject pullableObj = focusing.GetComponent<PullableObject>();
        if (pullableObj != null && holdingItem != pullableObj.gameObject) {
            pullSM?.PullObjHover(null);
        }

        PCDItemSlot slot = focusing.GetComponent<PCDItemSlot>();
        if (slot != null && slot.dynamicPreview)
            slot.DestoryPreview();
    }
    public void OnFocusStay(Focusable focusing) {}

    private void Update() {
        holdAndDropSM?.update();
        pullSM?.update();
    }

    public override bool Pick(IPickable pickable) {
        var pickableComp = pickable as Component;
        // Debug.Log("Pick " + pickableComp.gameObject.name);
        return base.Pick(pickable);
    }

    public override bool Drop() {
        // Debug.Log("Drop " + holdingItem.name);
        return base.Drop();
    }

    public void Pull(PullableObject pullableObj) {
        // Debug.Log("Pull " + pullableObj.gameObject.name);

        Action pulledOutCallback = null;
        pulledOutCallback += RestPullingAndPickIt;
        pullSM?.PullObj(pullableObj, pullableObj.pullInfo, pulledOutCallback, pullableObj.shape);
        holdingItem = pullableObj.gameObject;

        characterManager.SetFaceLockTarget(pullableObj.transform);

        void RestPullingAndPickIt() {
            InteractLogger.LogInteractEvent("PulledOut", gameObject, pullableObj.gameObject);
            RestPulling();
            if (pullableObj.pickableObjectPrefab && pullableObj.pickableObjectPrefab.GetComponent<PickableObject>()) {
                PickableObject newItem = GameObject.Instantiate(pullableObj.pickableObjectPrefab, Vector3.down * 20.0f, Quaternion.identity).GetComponent<PickableObject>();
                Pick(newItem);
                InteractLogger.LogInteractEvent("PulledOutAndPick", gameObject, newItem.gameObject);
                InteractLogger.LogInteractEvent("Pick", gameObject, newItem.gameObject);
                if (BackpackManager.Inst != null && newItem.shape == PCDObjectProperties.Shape.Stick) {
                    /* 加入背包 */
                    BackpackManager.AddItemAndSelect(newItem.gameObject);
                }
            }
        }
    }

    public void RestPulling() {
        pullSM?.RestPulling();
        characterManager.SetFaceLockTarget(null);
        holdingItem = null;
    }
}