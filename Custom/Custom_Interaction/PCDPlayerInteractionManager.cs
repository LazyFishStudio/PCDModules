using System;
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

        ResetInputActions();
        GetInputActions();
        HandleInputActions();

        /*
        if (true) {
            string hint = "";
            foreach (InputDesc desc in inputDescs) {
                if (desc.keyDesc != null)
                    hint += string.Format("{0}：{1} ", desc.ToString(), desc.keyDesc);
                else
                    hint += string.Format("{0}：null ", desc.ToString());
            }
            Debug.Log(hint);
        }
        */

        /*
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
        */
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

#region Refractor
public partial class PCDPlayerInteractionManager : BaseInteractionManager
{
    public struct InputDesc
	{
        public string keyName; /* Pick, Drop, etc */
        public string keyState; /* GetKeyDown, GetKey, GetKeyUp */
        public string keyDesc; /* text description */

        public InputDesc(string name, string state, string desc) {
            keyName = name;
            keyState = state;
            keyDesc = desc;
        }

        public override string ToString() {
            return keyName + keyState;
		}
    }

    public List<InputDesc> inputDescs;
    public Dictionary<string, Action> inputActions;

    private void ResetInputActions() {
        if (inputDescs == null)
            inputDescs = new List<InputDesc>();
        inputDescs.Clear();
        if (inputActions == null)
            inputActions = new Dictionary<string, Action>();
        inputActions.Clear();
	}

    private void RegisterAction(string name, string type, string textDesc, Action action) {
        var desc = new InputDesc(name, type, textDesc);
        inputDescs.Add(desc);
        if (inputActions.ContainsKey(desc.ToString()))
            Debug.LogError("Input Map Has Conflict!");
        inputActions[desc.ToString()] = action;
    }

    private void GetInputActions() {
        if (interactComp.holdingItem == null && focusing != null) {
            PullableObject pullable = focusing.GetFocusComponent<PullableObject>();
            if (pullable != null) {
                RegisterAction("Pick", "GetKeyDown", "抓住", HandlePullAction);
            }
            PickableObject pickable = focusing.GetFocusComponent<PickableObject>();
            if (pullable == null && pickable != null) {
                RegisterAction("Pick", "GetKeyDown", "拾取", HandlePickAction);
            }
        }

        if (interactComp.holdingItem != null) {
            IPlaceSlot slot = (focusing != null ? focusing.GetFocusComponent<IPlaceSlot>() : null);
            IPlaceable placeable = interactComp.holdingItem.GetComponent<IPlaceable>();
            if (slot != null && placeable != null) {
                RegisterAction("Drop", "GetKeyDown", "放置", () => {
                    InteractLogger.LogInteractEvent("PlaceItem", gameObject, (placeable as Component).gameObject);
                    placeable.PlacedTo(interactComp, slot);
                });
            } else {
                PCDHumanInteractSM player = interactComp as PCDHumanInteractSM;
                if (player.pullingObject != null) {
                    RegisterAction("ResetPull", "GetKeyUp", null, HandleRestPullingAction);
                } else if (player.holdingObject != null) {
                    RegisterAction("Drop", "GetKeyDown", "放下", HandleDropHoldingAction);
                }
            }
        }

        if (backpackEnabled) {
            if (interactComp.holdingItem == null) {
                RegisterAction("Backpack", "GetKeyDown", null, HandleFastPickFromBackpackAction);
            } else {
                RegisterAction("Backpack", "GetKeyDown", "放入背包", HandleFastPutIntoBackpackAction);
            }
        }

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

    private void HandleInputActions() {
        if (interactInput.pick) {
            if (inputActions.TryGetValue("PickGetKeyDown", out Action action)) {
                action?.Invoke();
			}
		}
        if (interactInput.drop) {
            if (inputActions.TryGetValue("DropGetKeyDown", out Action action)) {
                action?.Invoke();
            }
        }
        if (interactInput.interactBackpack) {
            if (inputActions.TryGetValue("BackpackGetKeyDown", out Action action)) {
                action?.Invoke();
            }
        }
        if (interactInput.restPull) {
            if (inputActions.TryGetValue("ResetPullGetKeyUp", out Action action)) {
                action?.Invoke();
            }
        }
        if (interactInput.interactTrigger) {
            if (inputActions.TryGetValue("InteractGetKeyDown", out Action action)) {
                action?.Invoke();
            }
        }
        if (interactInput.interactHold) {
            if (inputActions.TryGetValue("InteractGetKey", out Action action)) {
                action?.Invoke();
            }
        }
        if (!interactInput.interactHold) {
            if (inputActions.TryGetValue("InteractGetKeyUp", out Action action)) {
                action?.Invoke();
            }
        }
    }

    private void RegisterTriggerInteractable(ITriggerInteractable item, string interactType) {
        if (item.CheckInteractCond(interactComp)) {
            RegisterAction("Interact", "GetKeyDown", interactType, () => {
                item.OnInteract(interactComp);
                InteractLogger.LogInteractEvent("InteractItem", gameObject, (item as Component).gameObject);
            });
        }
    }

    private void RegisterHoldInteractable(IHoldInteractable item, string interactType) {
        if (!item.IsInteracting && item.CheckInteractCond(interactComp)) {
            /* 空闲，满足条件，注册开始交互事件 */
            RegisterAction("Interact", "GetKeyDown", interactType, () => {
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
                RegisterAction("Interact", "GetKey", interactType, () => {
                    if (item.OnInteractStay(interactComp)) {
                        item.OnInteractFinish(interactComp);
                        InteractLogger.LogInteractEvent("InteractItemFinish", gameObject, (item as Component).gameObject);
                    }
                });
                RegisterAction("Interact", "GetKeyUp", null, () => {
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