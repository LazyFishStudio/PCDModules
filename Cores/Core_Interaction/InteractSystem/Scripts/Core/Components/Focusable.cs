using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
	public abstract class Focusable : MonoBehaviour
	{
		public List<Component> focusComps;

		public virtual T GetFocusComponent<T>() where T : class {
			if (focusComps == null)
				return null;
			foreach (var comp in focusComps)
				if (comp is T)
					return comp as T;
			return null;
		} 
		public abstract bool FreshAndCheckFocusComps(InteractionManager manager);
		public abstract void OnFocusEnter(InteractionManager manager);
		public abstract void OnFocusStay(InteractionManager manager);
		public abstract void OnFocusExit(InteractionManager manager);
	}
}
