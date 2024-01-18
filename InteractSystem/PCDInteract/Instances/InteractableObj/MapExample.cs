using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class MapExample : PCDHoldProp
{
    public GameObject mapUI; 

	private void Awake() {
		mapUI.SetActive(false);
	}

	public override void OnInteractStart(InteractComp interactor) {
		base.OnInteractStart(interactor);
		mapUI.SetActive(true);
	}

	public override bool OnInteractStay(InteractComp interactor) {
		return false;
	}

	public override void OnInteractTerminate(InteractComp interactor) {
		base.OnInteractTerminate(interactor);
		mapUI.SetActive(false);
	}
}
