using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDFocusable : Focusable
{
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
