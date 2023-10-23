using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class MapExample : PCDInteractable
{
    public GameObject mapUI; 

	private void Awake() {
		mapUI.SetActive(false);
	}

	public override bool CheckInteractCond(InteractComp interactor) {
		return interactor.holdingItem == this.gameObject;
	}

	public override bool OnInteractStay(InteractComp interactor) {
		return true;
	}

	public override void OnInteractFinish(InteractComp interactor) {
		base.OnInteractFinish(interactor);
		mapUI.SetActive(!mapUI.activeInHierarchy);
	}
}
