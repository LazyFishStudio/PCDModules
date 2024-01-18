using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
	[Serializable]
	public class GenericSlotHook
	{
		public SlotHookType hookType;
		public string arg;
		public GameObject GOArg;

		public Func<GameObject, bool> GetHookFunction() {
			switch (hookType) {
				case SlotHookType.CheckGOName: {
					return (item) => {
						return item.name == GOArg.name || item.name == GOArg.name + "(Clone)";
					};
				}
				case SlotHookType.CheckTargetComponent: {
					break;
				}
			}
			return null;
		}
	}

	public enum SlotHookType { CheckGOName, CheckTargetComponent };
}