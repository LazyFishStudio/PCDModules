using UnityEngine;
using InteractSystem;

public class PCDTriggerProp : MonoBehaviour, ITriggerInteractable
{
    [Header("����������ʾ")][TextArea(3, 10)]
    public string interactHint;
    [Header("����������ʾ")]
    public string interactType;
    public InteractComp interactor { get; set; }

    public virtual bool CheckInteractCond(InteractComp interactor) {
        /* �����Ǳ�����ֲֳ���ʹ�� */
        return interactor.holdingItem == gameObject;
    }

    public virtual bool OnInteract(InteractComp interactor) {
        Debug.Log("Prop " + gameObject.name + " used by " + interactor.gameObject.name);
        return true;
	}
}