using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using UnityEngine.XR;
using UnityEngine.UI;

#region Interface
public partial class PCDPlayerInteractionManager : BaseInteractionManager
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
public partial class PCDPlayerInteractionManager : BaseInteractionManager
{
    public InteractInput interactInput;
    public GameObject virtualInput;
    private bool backpackEnabled { get => BackpackManager.Inst != null; }

    void Start() {
    }

    protected override void Update() {
        base.Update();

        interactInput.interactBackpack = InputManager.GetKeyDown(KeyCode.T);
        interactInput.pick = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.restPull = InputManager.GetKeyUp(KeyCode.Mouse0);
        interactInput.interactTrigger = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.interactHold = InputManager.GetKey(KeyCode.Mouse0);
        interactInput.drop = InputManager.GetKeyDown(KeyCode.Mouse1);

        HandleInteractInput();
    }

    void LateUpdate() {
        interactInput.pick = false;
        interactInput.restPull = false;
        justPick = false;
    }

    private bool justPick = false;
    private void HandleInteractInput() {
        /* ����ʰȡ�ͷ��� */
        if (interactInput.pick) {
            if (interactComp.holdingItem == null) {
                HandlePickAndPullAction();
                if (interactComp.holdingItem != null)
                    justPick = true;
            } else {
                // IPlaceSlot slot = (focusing != null ? focusing.GetFocusComponent<IPlaceSlot>() : null);
                // IPlaceable placeable = interactComp.pickingItem.GetComponent<IPlaceable>();
                // if (slot != null && placeable != null) {
                //     placeable.PlacedTo(interactComp, slot);
                // } else {
                //     HandleDropAndResetPullAction();
                // }
			}
        }

        if (interactInput.drop) {
            if (interactComp.holdingItem == null) {
                // HandlePickAndPullAction();
			} else {
                IPlaceSlot slot = (focusing != null ? focusing.GetFocusComponent<IPlaceSlot>() : null);
                IPlaceable placeable = interactComp.holdingItem.GetComponent<IPlaceable>();
                if (slot != null && placeable != null) {
                    InteractLogger.LogInteractEvent("PlaceItem", gameObject, (placeable as Component).gameObject);
                    placeable.PlacedTo(interactComp, slot);
                } else {
                    HandleDropAndResetPullAction();
                }
			}
        }

        if (interactInput.restPull && interactComp.holdingItem != null) {
            PCDHumanInteractSM player = interactComp as PCDHumanInteractSM;
            if (player.pullingObject != null) {
                HandleRestPullingAction();
            }
        }

        /* ����������ݲ��� */
        if (backpackEnabled && interactInput.interactBackpack) {
            if (interactComp.holdingItem == null) {
                HandleFastPickFromBackpackAction();
			} else {
                HandleFastPutIntoBackpackAction();
			}
		}

        /* 处理使用手上的物品 */
        if (interactComp.holdingItem != null) {
            ITriggerInteractable triggerItem = interactComp.holdingItem.GetComponent<ITriggerInteractable>();
            IHoldInteractable holdItem = interactComp.holdingItem.GetComponent<IHoldInteractable>();
            if (triggerItem != null) {
                if (interactInput.interactTrigger && triggerItem.CheckInteractCond(interactComp)) {
                    triggerItem.OnInteract(interactComp);
                    InteractLogger.LogInteractEvent("InteractItem", gameObject, interactComp.holdingItem);
                }
			} else if (holdItem != null) {
                if (interactInput.interactHold && holdItem.CheckInteractCond(interactComp)) {
                    /* 按住交互键，且满足交互条件 */
                    if (!holdItem.IsInteracting && interactInput.interactTrigger && !justPick) {
                        /* 并没有人与之交互，开始交互 */
                        holdItem.OnInteractStart(interactComp);
                        InteractLogger.LogInteractEvent("InteractItemStart", gameObject, interactComp.holdingItem);
                    }
                    if (holdItem.IsInteracting && holdItem.interactor == interactComp) {
                        /* 当前角色正在与其交互，继续交互 */
                        if (holdItem.OnInteractStay(interactComp)) {
                            holdItem.OnInteractFinish(interactComp);
                            InteractLogger.LogInteractEvent("InteractItemFinish", gameObject, interactComp.holdingItem);
                        }
					}
				} else if (holdItem.IsInteracting && holdItem.interactor == interactComp) {
                    /* 正在交互，但是把交互键松开了 */
                    holdItem.OnInteractTerminate(interactComp);
                    InteractLogger.LogInteractEvent("InteractItemTerminate", gameObject, interactComp.holdingItem);
                }
			}
        }

        /* 处理面前的可交互物品 */
        if (focusing != null) {
            ITriggerInteractable triggerItem = focusing.GetFocusComponent<ITriggerInteractable>();
            IHoldInteractable holdItem = focusing.GetFocusComponent<IHoldInteractable>();
            if (triggerItem != null) {
                if (interactInput.interactTrigger && triggerItem.CheckInteractCond(interactComp)) {
                    triggerItem.OnInteract(interactComp);
                    InteractLogger.LogInteractEvent("InteractItem", gameObject, focusing.gameObject);
                }
            } else if (holdItem != null) {
                if (interactInput.interactHold && holdItem.CheckInteractCond(interactComp)) {
                    /* 按住交互键，且满足交互条件 */
                    if (!holdItem.IsInteracting && interactInput.interactTrigger) {
                        /* 并没有人与之交互，开始交互 */
                        holdItem.OnInteractStart(interactComp);
                        InteractLogger.LogInteractEvent("InteractItemStart", gameObject, focusing.gameObject);
                    }
                    if (holdItem.IsInteracting && holdItem.interactor == interactComp) {
                        /* 当前角色正在与其交互，继续交互 */
                        if (holdItem.OnInteractStay(interactComp)) {
                            holdItem.OnInteractFinish(interactComp);
                            InteractLogger.LogInteractEvent("InteractItemFinish", gameObject, focusing.gameObject);
                        }
                    }
                } else if (holdItem.IsInteracting && holdItem.interactor == interactComp) {
                    /* 正在交互，但是把交互键松开了 */
                    holdItem.OnInteractTerminate(interactComp);
                    InteractLogger.LogInteractEvent("InteractItemTerminate", gameObject, focusing.gameObject);
                }
            }
        }

#if false
        /* ���ý�����ʾ���� */
        if (InteractHintManager.Inst != null) {
            InteractHintManager.HideHintText();
            if (interactable != null && interactable.interactor == null && interactComp.holdingItem != null) {
                PCDInteractable item = interactComp.holdingItem.GetComponent<PCDInteractable>();
                if (item != null && item.interactType != "") {
                    InteractHintManager.ShowHintText(item.interactType, InteractHintType.MouseLeft);
                }
            }
            var hintItem = interactItem as PCDInteractable;
            if (hintItem != null && hintItem.interactType != "" && hintItem.CheckInteractCond(interactComp)) {
                InteractHintManager.ShowHintText(hintItem.interactType, InteractHintType.MouseLeft);
            }
        }
#endif
    }

    private void HandlePickAndPullAction() {
        if (focusing == null)
            return;

        PullableObject pullable = focusing.GetFocusComponent<PullableObject>();
        if (pullable != null) {
            HandlePullAction();
            return;
		}

        PickableObject pickable = focusing.GetFocusComponent<PickableObject>();
        if (pickable != null) {
            HandlePickAction();
            return;
		}
    }

    private void HandleDropAndResetPullAction() {
        PCDHumanInteractSM player = interactComp as PCDHumanInteractSM;
        if (player.pullingObject != null) {
            HandleRestPullingAction();
            return;
        }
        if (player.holdingObject != null) {
            HandleDropHoldingAction();
		}
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
public partial class PCDPlayerInteractionManager : BaseInteractionManager
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