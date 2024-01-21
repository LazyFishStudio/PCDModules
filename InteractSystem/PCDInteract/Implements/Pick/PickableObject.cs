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

    public UnityEngine.Object attachedPlaceInternal;
    public IPlaceSlot attachedPlace { get => attachedPlaceInternal as IPlaceSlot; set => attachedPlaceInternal = (UnityEngine.Object)value; }
    public PCDObjectProperties.Shape shape = PCDObjectProperties.Shape.Box;
    public float gravity = 30f;
    public bool isCantBePicked;
    public bool disablePhysicOnSlot = true;

    private void Start() {
        if (attachedPlace != null && disablePhysicOnSlot) {
            SetPhysicActive(false);
        }
	}

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

        transform.SetParent(null);

        PCDHumanInteractSM character = interactor as PCDHumanInteractSM;
        character.holdAndDropSM?.HoldObj(transform, shape);
        OnPickedBy(interactor);

        return true;
    }

    public bool DroppedBy(InteractComp interactor) {
        PCDHumanInteractSM character = interactor as PCDHumanInteractSM;
        if (character.holdAndDropSM && character.holdAndDropSM.isAttacking) {
            return false;
        }
        OnDroppedBy(interactor);
        
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

        OnPlaceTo(slot);

        return true;
	}

    public bool RemovedFrom(InteractComp interactor) {
        attachedPlace.OnRemoveItemCallback(this);
        if (interactor) {
            PickedBy(interactor);
        }

        OnRemoveFrom(interactor);
        attachedPlace = null;

        return true;
	}

    public RigidbodyInterpolation oriRbInterpolation;
    public void SetPhysicActive(bool active) {
        if (active) {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) {
                collider.enabled = true;
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) {
                rb.isKinematic = false;
                oriRbInterpolation = rb.interpolation;
                rb.interpolation = RigidbodyInterpolation.None;
            }
        } else {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) {
                collider.enabled = false;
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) {
                rb.isKinematic = true;
                rb.interpolation = oriRbInterpolation;
            }
        }
    }

    public virtual void OnDroppedBy(InteractComp interactor) {

    }

    public virtual void OnPickedBy(InteractComp interactor) {
		DOTween.Kill(transform, false);
    }

    public virtual void OnPlaceTo(IPlaceSlot slot) {

    }

    public virtual void OnRemoveFrom(InteractComp interactor) {

    }

    void OnDestroy() {
        if (picker) {
            picker.Drop();
            picker = null;
        }
    }

}