using UnityEngine;
using InteractSystem;

public class PCDTriggerProp : MonoBehaviour, ITriggerInteractable
{
    [Header("交互内容提示")][TextArea(3, 10)]
    public string interactHint;
    [Header("交互类型提示")]
    public string interactType;
    public InteractComp interactor { get; set; }

    public virtual bool CheckInteractCond(InteractComp interactor) {
        /* 必须是被玩家手持才能使用 */
        return interactor.holdingItem == gameObject;
    }

    public virtual bool OnInteract(InteractComp interactor) {
        Debug.Log("Prop " + gameObject.name + " used by " + interactor.gameObject.name);
        return true;
	}
}