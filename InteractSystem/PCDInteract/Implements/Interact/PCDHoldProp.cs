using UnityEngine;
using InteractSystem;

public class PCDHoldProp : MonoBehaviour, IHoldInteractable
{
    [Header("����������ʾ")]
    [TextArea(3, 10)]
    public string interactHint;
    [Header("����������ʾ")]
    public string interactType;
    public InteractComp interactor { get; set; }
    public bool IsInteracting { get => interactor != null; }


    public virtual bool CheckInteractCond(InteractComp interactor) {
        /* �����Ǳ�����ֲֳ���ʹ�� */
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