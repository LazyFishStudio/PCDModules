using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using System.Linq;
using DG.Tweening;

public class PCDItemSlot : ItemSlot
{
	public Material previewMat;
	public MatModifier preview;
	public bool dynamicPreview;
	public bool enablePlaceAnimation;
	public float animTime = 0.22f;
	public Ease animEase = Ease.InOutSine;


	public override void OnAcceptItemCallback(InteractComp interactor, IPlaceable item) {
		base.OnAcceptItemCallback(interactor, item);
		if (enablePlaceAnimation) {
			if (item is Component component)
				component.transform.DOLocalMoveY(0, animTime).From(1).SetEase(animEase);
		}
	}

	public override void OnRemoveItemCallback(IPlaceable item) {
		base.OnRemoveItemCallback(item);
	}

	protected void DestoryUselessComponents(List<Component> pending, int times = 0) {
		List<Component> nextPending = new List<Component>();
		foreach (var component in pending) {
			try {
				if (component is IPickable || component is IFocusable || component is Focusable) {
					Destroy(component);
					if (component is Rigidbody rb)
						rb.isKinematic = true;
				}
			} catch {
				nextPending.Add(component);
			}
		}
		if (nextPending.Count > 0 && times < 5)
			DestoryUselessComponents(nextPending, times + 1);
	}

    public void SetupPreview(GameObject item) {
		var newItem = Instantiate(item, null);
		newItem.transform.position = transform.position;
		newItem.transform.rotation = transform.rotation;
		newItem.transform.SetParent(transform);

		PickableObject pickable = newItem.GetComponent<PickableObject>();
		pickable.picker = null;
		if (pickable != null)
			Destroy(pickable);
		List<Component> allComponents = newItem.GetComponents<Component>().ToList();
		allComponents = allComponents.Concat(newItem.GetComponentsInChildren<Component>().ToList()).ToList();
		DestoryUselessComponents(allComponents);

		newItem.AddComponent<MatModifier>();
		preview = newItem.GetComponent<MatModifier>();

		// Material mat = Resources.Load<Material>("Materials/Transparent");
		preview.SetupOtherMaterial(previewMat);
		preview.ChangeIntoOtherMaterials();
	}

	public void DestoryPreview() {
		if (preview != null) {
			Destroy(preview.gameObject);
			preview = null;
		}
	}
}
