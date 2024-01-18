using UnityEngine;

namespace InteractSystem
{
    public interface IInteractableBase
    {
        public InteractComp interactor { get; set; }
        public bool CheckInteractCond(InteractComp interactor);
    }

    public interface ITriggerInteractable : IInteractableBase
    {
        public bool OnInteract(InteractComp interactor);
    }

    public interface IHoldInteractable : IInteractableBase
    {
        public bool IsInteracting { get; }
        public void OnInteractStart(InteractComp interactor);
        public bool OnInteractStay(InteractComp interactor);
        public void OnInteractFinish(InteractComp interactor);
        public void OnInteractTerminate(InteractComp interactor);
    }
}
