using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDDoor : PCDTriggerInteractable
{
	public GameObject product;

	public override bool CheckInteractCond(InteractComp interactor) {
		return interactor.holdingItem != null && interactor.holdingItem.GetComponent<PCDKey>() != null;
	}

	public override bool OnInteract(InteractComp interactor) {
		var picking = interactor.holdingItem;
		interactor.Drop();
		Destroy(picking);
		Destroy(gameObject);
		interactor = null;

		product.SetActive(true);
		return true;
	}
}
