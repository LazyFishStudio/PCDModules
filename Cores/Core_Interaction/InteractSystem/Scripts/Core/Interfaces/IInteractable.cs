using UnityEngine;

namespace InteractSystem
{
    public interface IInteractable
    {
        public InteractComp interactor { get; set; }
        public bool CheckInteractCond(InteractComp interactor);
        public void OnInteractStart(InteractComp interactor);
        /// <summary>
        /// Return true iff interact is finished
        /// </summary>
        public bool OnInteractStay(InteractComp interactor);
        public void OnInteractFinish(InteractComp interactor);
        public void OnInteractTerminate(InteractComp interactor);
    }
}
