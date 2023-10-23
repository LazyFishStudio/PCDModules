using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

[RequireComponent(typeof(Focusable))]
public class PCDInteractable : MonoBehaviour, IInteractable, IFocusable
{
    [TextArea(3, 10)]
    public string interactHint;
    public string interactType;
    public InteractComp interactor { get; set; }

    public virtual bool CheckInteractCond(InteractComp interactor) { return true; }
    public virtual void OnInteractStart(InteractComp interactor) {
        this.interactor = interactor;
        Debug.Log("InteractStart");
    }
    public virtual bool OnInteractStay(InteractComp interactor) { 
        Debug.Log("InteractStay");
        return false; 
    }
    public virtual void OnInteractFinish(InteractComp interactor) {
        Debug.Log("InteractFinish");
        this.interactor = null;
    }
    public virtual void OnInteractTerminate(InteractComp interactor) {
        this.interactor = null;
    }

	public virtual bool CheckFocusCond(InteractionManager manager) {
        bool cond = CheckInteractCond(manager.interactComp);
        if (cond) {
            UI_FocusObjectNameTips.GetInstance().content = interactHint;
        }
        return cond;
    }
}
