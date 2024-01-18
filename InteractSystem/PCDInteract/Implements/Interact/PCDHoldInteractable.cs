using UnityEngine;
using InteractSystem;

public class PCDHoldInteractable : MonoBehaviour, IHoldInteractable, IFocusable
{
    [Header("交互内容提示")]
    [TextArea(3, 10)]
    public string interactHint;
    [Header("交互类型提示")]
    public string interactType;
    public InteractComp interactor { get; set; }
    public bool IsInteracting { get => interactor != null; }

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

    public virtual void OnInteractStart(InteractComp interactor) {
        this.interactor = interactor;
    }

    public virtual bool OnInteractStay(InteractComp interactor) {
        Debug.Log("Prop " + gameObject.name + " using by " + interactor.gameObject.name);
        return false;
    }

    public virtual void OnInteractFinish(InteractComp interactor) {
        this.interactor = null;
    }

    public virtual void OnInteractTerminate(InteractComp interactor) {
        this.interactor = null;
    }
}