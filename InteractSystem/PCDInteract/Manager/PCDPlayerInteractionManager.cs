using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using UnityEngine.XR;
using UnityEngine.UI;

#if false
public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    private partial void HandlePickAction() {
        var pickable = focusing.GetFocusComponent<PickableObject>();
        if (!pickable.CheckPickCond(interactComp))
            return;

        IPlaceable placeable = focusing.GetFocusComponent<IPlaceable>();
        if (placeable != null && placeable.attachedPlace != null) {
            placeable.RemovedFrom(interactComp);
            InteractLogger.LogInteractEvent("RemoveItem", gameObject, (placeable as Component).gameObject);
        } else {
            interactComp.Pick(pickable);
            InteractLogger.LogInteractEvent("Pick", gameObject, pickable.gameObject);
            if (backpackEnabled && pickable.shape == PCDObjectProperties.Shape.Stick) {
                BackpackManager.AddItemAndSelect(pickable.gameObject);
            }
        }
    }

    private partial void HandlePullAction() {
        var pullable = focusing.GetFocusComponent<PullableObject>();
        var player = interactComp as PCDHumanInteractSM;
        player.Pull(pullable);
        InteractLogger.LogInteractEvent("StartPull", gameObject, pullable.gameObject);
    }

    private partial void HandleDropHoldingAction() {
        var pickable = interactComp.holdingItem.GetComponent<PickableObject>();
        if (pickable == null)
            return;

        interactComp.Drop();
        InteractLogger.LogInteractEvent("Drop", gameObject, pickable.gameObject);
        if (backpackEnabled && pickable.shape == PCDObjectProperties.Shape.Stick) {
            BackpackManager.RemoveSelectingItem();
        }
    }

    private partial void HandleRestPullingAction() {
        var player = interactComp as PCDHumanInteractSM;
        GameObject pullObject = (player.holdingItem != null ? player.holdingItem.gameObject : null);

        player.RestPulling();
        InteractLogger.LogInteractEvent("ResetPull", gameObject, pullObject);
    }

    private partial void HandleFastPickFromBackpackAction() {
        if (!backpackEnabled || BackpackManager.Inst.prevSelected < 0)
            return;

        PickableObject item = BackpackManager.CreateBackpackItem(BackpackManager.Inst.prevSelected);
        interactComp.Pick(item);
        InteractLogger.LogInteractEvent("Pick", gameObject, item.gameObject);
        BackpackManager.SelectInBackpack(BackpackManager.Inst.prevSelected);
    }

    private partial void HandleFastPutIntoBackpackAction() {
        if (!backpackEnabled || interactComp.holdingItem == null)
            return;

        var pickable = interactComp.holdingItem.GetComponent<PickableObject>();
        if (pickable == null || pickable.shape != PCDObjectProperties.Shape.Stick)
            return;

        interactComp.Drop();
        InteractLogger.LogInteractEvent("Drop", gameObject, pickable.gameObject);
        Destroy(pickable.gameObject);
        BackpackManager.SelectInBackpack(-1);
    }

    private partial void HandleThrowAction() {
        var pickable = interactComp.holdingItem.GetComponent<PickableObject>();
        if (pickable == null)
            return;

        pickable.ThrownBy(interactComp);
        InteractLogger.LogInteractEvent("Throw", gameObject, pickable.gameObject);
        if (backpackEnabled && pickable.shape == PCDObjectProperties.Shape.Stick) {
            BackpackManager.RemoveSelectingItem();
        }
    }
}
#endif


public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    public string playerName = "P1";

    private PCDInteractHandler interactHandler;

    private void Awake() {
        interactHandler = GetComponent<PCDInteractHandler>();
    }

    private void Start() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        actionManager.RegisterActionHandler(this as IPCDActionHandler);
    }

    public void RegisterActionOnUpdate() {
        if (!gameObject.activeInHierarchy)
            return;
        var locker = GetComponent<PCDActLocker>();
        if (locker != null && locker.interactionLocked)
            return;

        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        /* 注册 Pick 和 Pull 操作 */
        if (interactComp.holdingItem == null && focusing != null) {
            PullableObject pullable = focusing.GetFocusComponent<PullableObject>();
            if (pullable != null) {
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", "抓住", () => { interactHandler.HandlePull(pullable); });
            }
            PickableObject pickable = focusing.GetFocusComponent<PickableObject>();
            if (pullable == null && pickable != null) {
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", "拾取", () => { interactHandler.HandlePick(pickable); });
            }
        }

        /* 注册 Place，ResetPull 和 Drop 操作 */
        if (interactComp.holdingItem != null) {
            IPlaceSlot slot = (focusing != null ? focusing.GetFocusComponent<IPlaceSlot>() : null);
            PickableObject placeable = interactComp.holdingItem.GetComponent<PickableObject>();
            if (slot != null && placeable != null) {
                string interactDesc = "放置";
                if (slot is PCDGenericSlot genericSlot && genericSlot.interactDesc != null && genericSlot.interactDesc != "")
                    interactDesc = genericSlot.interactDesc;
                if (!locker.dropLocked) {
                    actionManager.RegisterAction(playerName, "SecondInteract", "GetKeyDown", interactDesc, () => { interactHandler.HandlePlaceItemToSlot(placeable, slot); });
                }
            } else {
                PCDHumanInteractSM player = interactComp as PCDHumanInteractSM;
                if (player.pullingObject != null) {
                    /* 需要持续注册"抓住"，用以显示提示 */
                    actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyUp", "抓住", () => { interactHandler.HandleRestPulling(); });
                } else if (player.holdingObject != null) {
                    if (!locker.dropLocked) {
                        actionManager.RegisterAction(playerName, "SecondInteract", "GetKeyDown", "放下", () => { interactHandler.HandleDrop(); });
                    }
                }
            }
        }

#if false
        /* 注册背包操作 */
        if (backpackEnabled) {
            if (interactComp.holdingItem == null) {
                actionManager.RegisterAction(playerName, "F", "GetKeyDown", null, HandleFastPickFromBackpackAction);
            } else {
                actionManager.RegisterAction(playerName, "F", "GetKeyDown", "放入背包", HandleFastPutIntoBackpackAction);
            }
        }
#endif

        /* 注册手上物品使用操作 */
        if (interactComp.holdingItem != null) {
            PCDTriggerProp triggerItem = interactComp.holdingItem.GetComponent<PCDTriggerProp>();
            if (triggerItem != null) {
                RegisterTriggerInteractable(triggerItem as ITriggerInteractable, triggerItem.interactType);
            }
            PCDHoldProp holdItem = interactComp.holdingItem.GetComponent<PCDHoldProp>();
            if (holdItem != null) {
                RegisterHoldInteractable(holdItem as IHoldInteractable, holdItem.interactType);
            }
        }

        /* 注册场景物品交互操作 */
        if (focusing != null) {
            PCDTriggerInteractable triggerItem = focusing.GetFocusComponent<PCDTriggerInteractable>();
            if (triggerItem != null) {
                RegisterTriggerInteractable(triggerItem as ITriggerInteractable, triggerItem.interactType);
            }
            PCDHoldInteractable holdItem = focusing.GetFocusComponent<PCDHoldInteractable>();
            if (holdItem != null) {
                RegisterHoldInteractable(holdItem as IHoldInteractable, holdItem.interactType);
            }
        }
    }

    private void RegisterTriggerInteractable(ITriggerInteractable item, string interactType) {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        if (item.CheckInteractCond(interactComp)) {
            actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", interactType, () => { interactHandler.HandleInteractTrigger(item); });
        }
    }

    private void RegisterHoldInteractable(IHoldInteractable item, string interactType) {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        if (!item.IsInteracting && item.CheckInteractCond(interactComp)) {
            /* 空闲，满足条件，注册开始交互事件 */
            actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", interactType, () => { interactHandler.HandleInteractHold(item); });
        }
        if (item.IsInteracting && item.interactor == interactComp) {
            if (item.CheckInteractCond(interactComp)) {
                /* 正在交互，仍然满足交互条件，注册继续和中止事件 */
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKey", interactType, () => { interactHandler.HandleInteractHold(item); });
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyUp", null, () => { interactHandler.HandleInteractHoldTerminate(item); });
            } else {
                /* 正在交互，不再满足交互条件，立即中止 */
                interactHandler.HandleInteractHoldTerminate(item);
            }
        }
    }
}