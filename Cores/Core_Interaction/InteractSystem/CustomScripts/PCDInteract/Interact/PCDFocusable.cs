using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDFocusable : Focusable
{
	public override bool FreshAndCheckFocusComps(InteractionManager manager) {
        if (!manager.CheckCommonFocusCond(this))
            return false;

        focusComps = new List<Component>();
        foreach (var focusable in GetComponents<IFocusable>()) {
            if (focusable is Component comp && focusable.CheckFocusCond(manager)) {
                focusComps.Add(comp);
            }
		}
        return focusComps.Count > 0;
	}

	public override void OnFocusEnter(InteractionManager manager) {
        GetComponentInChildren<MeshUI>()?.ShowUI();

        PCDHumanInteractSM player = manager.interactComp as PCDHumanInteractSM;
        player?.OnFocusEnter(this);
    }

    public override void OnFocusExit(InteractionManager manager) {
        GetComponentInChildren<MeshUI>()?.HideUI();

        PCDHumanInteractSM player = manager.interactComp as PCDHumanInteractSM;
        player?.OnFocusExit(this);
    }

    public override void OnFocusStay(InteractionManager manager) {
        PCDHumanInteractSM player = manager.interactComp as PCDHumanInteractSM;
        player?.OnFocusStay(this);
    }
}
