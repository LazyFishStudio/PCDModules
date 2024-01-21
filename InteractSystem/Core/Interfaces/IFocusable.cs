using UnityEngine;

namespace InteractSystem
{
	public interface IFocusable
	{
		public bool CheckFocusCond(InteractComp interactor);
	}
}
