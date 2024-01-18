using UnityEngine;

namespace InteractSystem
{
	public abstract class InteractionManager : MonoBehaviour
	{
		public Focusable focusing;
		public InteractComp interactComp { get { return GetComponent<InteractComp>(); } }
		public abstract bool CheckCommonFocusCond(Focusable target);
	}
}
