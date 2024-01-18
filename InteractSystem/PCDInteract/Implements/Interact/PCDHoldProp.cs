using UnityEngine;
using InteractSystem;

public class PCDHoldProp : MonoBehaviour, IHoldInteractable
{
    [Header("交互内容提示")]
    [TextArea(3, 10)]
    public string interactHint;
    [Header("交互类型提示")]
    public string interactType;
    public InteractComp interactor { get; set; }
    public bool IsInteracting { get => interactor != null; }


    public virtual bool CheckInteractCond(InteractComp interactor) {
        /* 必须是被玩家手持才能使用 */
        return interactor.holdingItem == gameObject;
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