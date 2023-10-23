using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class ItemSlotGem : PCDItemSlot
{
	public GameObject hint;

	public override bool CheckAcceptItem(InteractComp interactor, IPlaceable item) {
		if (!base.CheckAcceptItem(interactor, item))
			return false;
		Component component = item as Component;
		if (component == null)
			return false;
		return component.GetComponent<GemItem>() != null;
	}

	public override void OnAcceptItemCallback(InteractComp interactor, IPlaceable item) {
		base.OnAcceptItemCallback(interactor, item);
		if (hint != null)
			Destroy(hint);
		if (item is Component component) {
			GameObject gameObject = component.gameObject;
			gameObject.GetComponent<Collider>().enabled = true;
			Destroy(gameObject.GetComponent<PickableObject>());
			Destroy(gameObject.GetComponent<Focusable>());
		}
	}
}
