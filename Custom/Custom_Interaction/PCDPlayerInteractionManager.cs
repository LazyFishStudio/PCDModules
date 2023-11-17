using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using UnityEngine.XR;
using UnityEngine.UI;

#region Interface
public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    private partial void HandlePickAction();
    private partial void HandlePullAction();
    private partial void HandleDropHoldingAction();
    private partial void HandleRestPullingAction();
    private partial void HandleFastPickFromBackpackAction();
    private partial void HandleFastPutIntoBackpackAction();
    private partial void HandleThrowAction();
}
#endregion

#region Internal
public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    public InteractInput interactInput;
    public GameObject virtualInput;
    private bool backpackEnabled { get => BackpackManager.Inst != null; }

    private void Start() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        actionManager.RegisterActionHandler(this as IPCDActionHandler);
    }

    protected override void Update() {
        base.Update();

        interactInput.interactBackpack = InputManager.GetKeyDown(KeyCode.T);
        interactInput.pick = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.restPull = InputManager.GetKeyUp(KeyCode.Mouse0);
        interactInput.interactTrigger = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.interactHold = InputManager.GetKey(KeyCode.Mouse0);
        interactInput.drop = InputManager.GetKeyDown(KeyCode.Mouse1);
    }
}

[System.Serializable]
public class InteractInput
{
    public bool pick;
    public bool drop;
    public bool restPull;
    public bool interactBackpack;
    public bool interactTrigger;
    public bool interactHold;
}
#endregion

#region Implement
public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    private partial void HandlePickAction() {
        var pickable = focusing.GetFocusComponent<PickableObject>();
        if (!pickable.CheckPickCond(this))
            return;

        IPlaceable placeable = focusing.GetFocusComponent<IPlaceable>();
        if (placeable != null && placeable.attachedPlace != null) {
            placeable.RemovedFrom(interactComp);
            InteractLogger.LogInteractEvent("RemoveItem", gameObject, (placeable as Component).gameObject);
        } else {
            interactComp.Pick(pickable);
            InteractLogger.LogInteractEvent("Pick", gameObject, pickable.gameObject);
            if (backpackEnabled && pickable.shape == PCDObjectProperties.Shape.Stick) {
                /* ���ԷŽ����� */
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
            /* ��Ҫ�ӱ�����ɾ�� */
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
            /* ��Ҫ�ӱ�����ɾ�� */
            BackpackManager.RemoveSelectingItem();
        }
    }
}
#endregion

#region Refractor
public partial class PCDPlayerInteractionManager : BaseInteractionManager, IPCDActionHandler
{
    public string playerName = "P1";

    public void RegisterActionOnUpdate() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        /* 注册 Pick 和 Pull 操作 */
        if (interactComp.holdingItem == null && focusing != null) {
            PullableObject pullable = focusing.GetFocusComponent<PullableObject>();
            if (pullable != null) {
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", "抓住", HandlePullAction);
            }
            PickableObject pickable = focusing.GetFocusComponent<PickableObject>();
            if (pullable == null && pickable != null) {
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", "拾取", HandlePickAction);
            }
        }

        /* 注册 Place，ResetPull 和 Drop 操作 */
        if (interactComp.holdingItem != null) {
            IPlaceSlot slot = (focusing != null ? focusing.GetFocusComponent<IPlaceSlot>() : null);
            IPlaceable placeable = interactComp.holdingItem.GetComponent<IPlaceable>();
            if (slot != null && placeable != null) {
                string interactDesc = "放置";
                if (slot is PCDGenericSlot genericSlot && genericSlot.interactDesc != null && genericSlot.interactDesc != "") {
                    interactDesc = genericSlot.interactDesc;
                }
                actionManager.RegisterAction(playerName, "SecondInteract", "GetKeyDown", interactDesc, () => {
                    InteractLogger.LogInteractEvent("PlaceItem", gameObject, (placeable as Component).gameObject);
                    placeable.PlacedTo(interactComp, slot);
                });
            } else {
                PCDHumanInteractSM player = interactComp as PCDHumanInteractSM;
                if (player.pullingObject != null) {
                    actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyUp", null, HandleRestPullingAction);
                } else if (player.holdingObject != null) {
                    actionManager.RegisterAction(playerName, "SecondInteract", "GetKeyDown", "放下", HandleDropHoldingAction);
                }
            }
        }

        /* 注册背包操作 */
        if (backpackEnabled) {
            if (interactComp.holdingItem == null) {
                actionManager.RegisterAction(playerName, "F", "GetKeyDown", null, HandleFastPickFromBackpackAction);
            } else {
                actionManager.RegisterAction(playerName, "F", "GetKeyDown", "放入背包", HandleFastPutIntoBackpackAction);
            }
        }

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
            actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", interactType, () => {
                item.OnInteract(interactComp);
                InteractLogger.LogInteractEvent("InteractItem", gameObject, (item as Component).gameObject);
            });
        }
    }

    private void RegisterHoldInteractable(IHoldInteractable item, string interactType) {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();

        if (!item.IsInteracting && item.CheckInteractCond(interactComp)) {
            /* 空闲，满足条件，注册开始交互事件 */
            actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyDown", interactType, () => {
                item.OnInteractStart(interactComp);
                InteractLogger.LogInteractEvent("InteractItemStart", gameObject, (item as Component).gameObject);
                if (item.OnInteractStay(interactComp)) {
                    item.OnInteractFinish(interactComp);
                    InteractLogger.LogInteractEvent("InteractItemFinish", gameObject, (item as Component).gameObject);
                }
            });
        }
        if (item.IsInteracting && item.interactor == interactComp) {
            if (item.CheckInteractCond(interactComp)) {
                /* 正在交互，仍然满足交互条件，注册继续和中止事件 */
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKey", interactType, () => {
                    if (item.OnInteractStay(interactComp)) {
                        item.OnInteractFinish(interactComp);
                        InteractLogger.LogInteractEvent("InteractItemFinish", gameObject, (item as Component).gameObject);
                    }
                });
                actionManager.RegisterAction(playerName, "FirstInteract", "GetKeyUp", null, () => {
                    item.OnInteractTerminate(interactComp);
                    InteractLogger.LogInteractEvent("InteractItemTerminate", gameObject, (item as Component).gameObject);
                });
            } else {
                /* 正在交互，不再满足交互条件，立即中止 */
                item.OnInteractTerminate(interactComp);
                InteractLogger.LogInteractEvent("InteractItemTerminate", gameObject, (item as Component).gameObject);
            }
        }
    }
}
#endregion