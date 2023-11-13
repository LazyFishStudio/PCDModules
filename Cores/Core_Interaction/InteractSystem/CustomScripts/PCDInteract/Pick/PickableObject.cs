using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using DG.Tweening;
// using DG.Tweening;

[RequireComponent(typeof(PCDFocusable))]
public class PickableObject : MonoBehaviour, IFocusable, IPickable, IPlaceable {
    [TextArea]
    public string pickHint;
    public InteractComp picker;
    public IPlaceSlot attachedPlace { get; set; }
    public PCDObjectProperties.Shape shape = PCDObjectProperties.Shape.Box;
    public float gravity = 30f;
    public bool isCantBePicked;
    public bool disablePhysicOnSlot = true;

	private void FixedUpdate() {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (picker == null && rb != null) {
            rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
		}
	}

	public bool CheckPickCond(InteractionManager manager) {
        if (manager.interactComp.holdingItem != null)
            return false;
        if (picker != null)
            return false;
        if (isCantBePicked) {
            return false;
        }
        return true;
    }

    public virtual bool CheckFocusCond(InteractionManager manager) {
        bool cond = CheckPickCond(manager);
        if (cond) {
            UI_FocusObjectNameTips.GetInstance().content = pickHint;
        }
        return cond;
    }

    public bool PickedBy(InteractComp interactor) {
        

        SetPhysicActive(false);
        picker = interactor;
        picker.holdingItem = gameObject;

        PCDHumanInteractSM character = interactor as PCDHumanInteractSM;
        character.holdAndDropSM?.HoldObj(transform, shape);
        OnPicked();

        return true;
    }

    public bool DroppedBy(InteractComp interactor) {
        PCDHumanInteractSM character = interactor as PCDHumanInteractSM;
        if (character.holdAndDropSM && character.holdAndDropSM.isAttacking) {
            return false;
        }
        OnDropped();
        
        SetPhysicActive(true);
        transform.SetParent(null);
        picker.holdingItem = null;
        picker = null;

        character.holdAndDropSM?.HoldObj(null, shape);

        IHoldInteractable interactable = GetComponent<IHoldInteractable>();
        if (interactable != null && interactable.interactor != null) {
            interactable.OnInteractTerminate(interactor);
		}


        return true;
    }

    public bool ThrownBy(InteractComp interactor) {
        DroppedBy(interactor);

        Vector3 forward = (interactor.transform.forward.normalized.ClearY() + Vector3.up * 1f).normalized;
        // TODO: ��������������Ӱ��
        GetComponent<Rigidbody>().AddForce(forward * 20f, ForceMode.VelocityChange);

        return true;
    }

    public bool PlacedTo(InteractComp interactor, IPlaceSlot slot) {
        if (interactor) {
            DroppedBy(interactor);
        }
        if (disablePhysicOnSlot) {
            SetPhysicActive(false);
        }
        attachedPlace = slot;
        transform.localRotation = Quaternion.identity;
        attachedPlace.OnAcceptItemCallback(interactor, this);
        return true;
	}

    public bool RemovedFrom(InteractComp interactor) {
        attachedPlace.OnRemoveItemCallback(this);
        attachedPlace = null;
        if (interactor) {
            PickedBy(interactor);
        }
        return true;
	}

    public void SetPhysicActive(bool active) {
        if (active) {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) {
                collider.enabled = true;
            }
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().isKinematic = false;
            }
        } else {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) {
                collider.enabled = false;
            }
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    public virtual void OnDropped() {

    }

    public virtual void OnPicked() {
		DOTween.Kill(transform, false);
    }

    void OnDestroy() {
        if (picker) {
            picker.Drop();
            picker = null;
        }
    }

}
