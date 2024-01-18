using UnityEngine;
using InteractSystem;

[RequireComponent(typeof(PCDFocusable))]
public class PCDTriggerInteractable : MonoBehaviour, ITriggerInteractable, IFocusable
{
    [Header("交互内容提示")]
    [TextArea(3, 10)]
    public string interactHint;
    [Header("交互类型提示")]
    public string interactType;
    public InteractComp interactor { get; set; }

    public virtual bool CheckFocusCond(InteractionManager manager) {
        if (CheckInteractCond(manager.interactComp)) {
            UI_FocusObjectNameTips.GetInstance().content = interactHint;
            return true;
        }
        return false;
	}

    public virtual bool CheckInteractCond(InteractComp interactor) {
        return true;
    }

    public virtual bool OnInteract(InteractComp interactor) {
        Debug.Log("Prop " + gameObject.name + " used by " + interactor.gameObject.name);
        return true;
    }
}