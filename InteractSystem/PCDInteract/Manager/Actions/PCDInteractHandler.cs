using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

/*
 * ** MUST** check interact condition before calling these!
 */
public partial class PCDInteractHandler : MonoBehaviour
{
    public partial void HandlePick(PickableObject pickable);
    public partial void HandleDrop();
    public partial void HandleThrow();
    public partial void HandlePull(PullableObject pullable);
    public partial void HandleRestPulling();
    public partial void HandleFastPickFromBackpack();
    public partial void HandleFastPutIntoBackpack();
    public partial void HandlePlaceItemToSlot(PickableObject pickable, IPlaceSlot slot);
    public partial void HandleInteractTrigger(ITriggerInteractable item);
    public partial void HandleInteractHold(IHoldInteractable item);
    public partial void HandleInteractHoldTerminate(IHoldInteractable item);
}

public partial class PCDInteractHandler : MonoBehaviour
{
    private InteractComp interactor;

    private void Awake() {
        interactor = GetComponent<InteractComp>();
    }

    private bool GetHodingPickable(out PickableObject pickable) {
        pickable = null;
        if (!interactor.holdingItem)
            return false;
        pickable = interactor.holdingItem.GetComponent<PickableObject>();
        if (!pickable)
            return false;
        return true;
    }

    public partial void HandlePick(PickableObject pickable) {
        if (!pickable.CheckPickCond(interactor))
            return;

        IPlaceable placeable = pickable as IPlaceable;
        if (placeable.attachedPlace != null) {
            placeable.RemovedFrom(interactor);
        } else {
            interactor.Pick(pickable);
        }
    }
    public partial void HandleDrop() {
        if (!GetHodingPickable(out PickableObject pickable))
            return;

        interactor.Drop();
    }
    public partial void HandleThrow() {
        if (!GetHodingPickable(out PickableObject pickable))
            return;

        pickable.ThrownBy(interactor);
    }
    public partial void HandlePull(PullableObject pullable) {
        (interactor as PCDHumanInteractSM).Pull(pullable);
    }
    public partial void HandleRestPulling() {
        (interactor as PCDHumanInteractSM).RestPulling();
    }
    public partial void HandleFastPickFromBackpack() {

	}
    public partial void HandleFastPutIntoBackpack() {

	}
    public partial void HandlePlaceItemToSlot(PickableObject pickable, IPlaceSlot slot) {
        (pickable as IPlaceable).PlacedTo(interactor, slot);
    }
    public partial void HandleInteractTrigger(ITriggerInteractable item) {
        if (!item.CheckInteractCond(interactor))
            return;

        item.OnInteract(interactor);
    }
    public partial void HandleInteractHold(IHoldInteractable item) {
        if (!item.IsInteracting && item.CheckInteractCond(interactor))
            item.OnInteractStart(interactor);
        if (item.IsInteracting && item.interactor == interactor) {
            if (item.CheckInteractCond(interactor)) {
                if (item.OnInteractStay(interactor)) {
                    item.OnInteractFinish(interactor);
                }
            } else {
                item.OnInteractTerminate(interactor);
            }
        }
    }
    public partial void HandleInteractHoldTerminate(IHoldInteractable item) {
        if (item.IsInteracting && item.interactor == interactor)
            item.OnInteractTerminate(interactor);
    }
}
