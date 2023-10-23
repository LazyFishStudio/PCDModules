using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDDoor : PCDInteractable, IFocusable
{
	public GameObject product;

	public bool CheckFocusCond(InteractionManager manager) {
		return CheckInteractCond(manager.interactComp);
	}

	public override bool CheckInteractCond(InteractComp interactor) {
		return interactor.holdingItem != null && interactor.holdingItem.GetComponent<PCDKey>() != null;
	}

	public override bool OnInteractStay(InteractComp interactor) {
		base.OnInteractStay(interactor);
		return true;
	}

	public override void OnInteractFinish(InteractComp interactor) {
		base.OnInteractFinish(interactor);
		var picking = interactor.holdingItem;
		interactor.Drop();
		Destroy(picking);
		Destroy(gameObject);
		interactor = null;

		/*
		var newItem = Instantiate(product, null);
		newItem.transform.position = transform.position;
		*/
		product.SetActive(true);
	}
}
