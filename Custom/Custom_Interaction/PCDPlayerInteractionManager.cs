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
        DebugTipsText.GetInstance().AddTips("左键：拾取/交互/使用工具");
        DebugTipsText.GetInstance().AddTips("右键：丢弃/收回工具");
    }

    protected override void Update() {
        base.Update();

        interactInput.interactBackpack = InputManager.GetKeyDown(KeyCode.T);
        interactInput.pick = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.restPull = InputManager.GetKeyUp(KeyCode.Mouse0);
        interactInput.interact = InputManager.GetKeyDown(KeyCode.Mouse0);
        interactInput.drop = InputManager.GetKeyDown(KeyCode.Mouse1);

        HandleInteractInput();
    }

    void LateUpdate() {
        interactInput.pick = false;
        interactInput.restPull = false;
        interactInput.interact = false;
    }

    private void HandleInteractInput() {
        /* ����ʰȡ�ͷ��� */
        if (interactInput.pick) {
            if (interactComp.holdingItem == null) {
                HandlePickAndPullAction();
			} else {
                // IPlaceSlot slot = (focusing != null ? focusing.GetComponent<IPlaceSlot>() : null);
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
                IPlaceSlot slot = (focusing != null ? focusing.GetComponent<IPlaceSlot>() : null);
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
                Debug.Log(1);
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

        IInteractable interactable = null;
        if (interactComp.holdingItem != null)
            interactable = interactComp.holdingItem.GetComponent<IInteractable>();
        if (interactable != null) {
            /* ��������Ʒ���� */
            bool interacting = interactable.interactor != null;
            if (interactInput.interact) {
                if (!interacting) {
                    interactable.OnInteractStart(interactComp);
                    InteractLogger.LogInteractEvent("InteractStart", gameObject, interactComp.holdingItem);
                }
                if (interactable.OnInteractStay(interactComp)) {
                    interactable.OnInteractFinish(interactComp);
                    InteractLogger.LogInteractEvent("InteractFinish", gameObject, interactComp.holdingItem);
                }
            } else {
                if (interacting) {
                    interactable.OnInteractTerminate(interactComp);
                    InteractLogger.LogInteractEvent("InteractTerminate", gameObject, interactComp.holdingItem);
                }
            }
        } else if (interactComp.holdingItem != null && focusing == null) {
            /* Ͷ�����ɽ�����Ʒ */
            if (interactInput.interact) {
                // HandleThrowAction();
            }
        }

        /* ��������Ʒ���� */
        IInteractable interactItem = null;
        if (focusing != null && focusing.GetComponent<IInteractable>() != null) {
            interactItem = focusing.GetComponent<IInteractable>();
            bool interacting = interactItem.interactor != null;
            if (interactItem != null) {
                if (interactInput.interact) {
                    if (!interacting) {
                        InteractLogger.LogInteractEvent("InteractStart", gameObject, focusing.gameObject);
                        interactItem.OnInteractStart(interactComp);
                    }
                    if (interactItem.OnInteractStay(interactComp)) {
                        interactItem.OnInteractFinish(interactComp);
                        InteractLogger.LogInteractEvent("InteractFinish", gameObject, focusing.gameObject);
                    }
                } else {
                    if (interacting) {
                        interactItem.OnInteractTerminate(interactComp);
                        InteractLogger.LogInteractEvent("InteractTerminate", gameObject, focusing.gameObject);
                    }
                }
            }
        }

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
    }

    private void HandlePickAndPullAction() {
        if (focusing == null)
            return;

        PullableObject pullable = focusing.GetComponent<PullableObject>();
        if (pullable != null) {
            HandlePullAction();
            return;
		}

        PickableObject pickable = focusing.GetComponent<PickableObject>();
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
    public bool interact;
}
#endregion

#region Implement
public partial class PCDPlayerInteractionManager : BaseInteractionManager
{
    private partial void HandlePickAction() {
        var pickable = focusing.GetComponent<PickableObject>();
        if (!pickable.CheckPickCond(this))
            return;

        IPlaceable placeable = focusing.GetComponent<IPlaceable>();
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
        var pullable = focusing.GetComponent<PullableObject>();
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