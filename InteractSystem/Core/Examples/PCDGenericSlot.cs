using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
	public class PCDGenericSlot : PCDItemSlot
	{
		public string interactDesc;
		public List<GenericSlotHook> conditionHooks;
		private List<Func<GameObject, bool>> conditions;

		protected virtual void Awake() {
			conditions = new List<Func<GameObject, bool>>();

			foreach (var hook in conditionHooks) {
				RegisterCondition(hook.GetHookFunction());
			}
		}

		public override bool CheckAcceptItem(InteractComp interactor, IPlaceable item) {
			if (!base.CheckAcceptItem(interactor, item))
				return false;
			foreach (var condition in conditions) {
				if (!condition.Invoke((item as MonoBehaviour).gameObject))
					return false;
			}
			return true;
		}

		public void RegisterCondition(Func<GameObject, bool> condition) {
			conditions.Add(condition);
		}
	}
}