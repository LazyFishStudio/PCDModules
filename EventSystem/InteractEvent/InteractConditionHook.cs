using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace PCD.EventSystem
{
	static public class InteractConditionHook
	{
		static public void InstallConditionHook(InteractEventConfig config, InteractConditionType conditionType, string arg) {
			switch (conditionType) {
				case InteractConditionType.CheckTargetComponent: {
					Type type = Type.GetType(arg);
					if (type != null) {
						config.AddCondition((interactEvent) => {
							return interactEvent.obj2 != null && interactEvent.obj2.GetComponent(type) != null;
						});
					} else {
						Debug.LogError("Type \"" + arg + "\" not found!");
					}
					break;
				}
				case InteractConditionType.OneObjectOnlyPassOnces: {
					HashSet<GameObject> visited = new HashSet<GameObject>();
					config.AddCondition((interactEvent) => {
						if (!visited.Contains(interactEvent.obj2)) {
							visited.Add(interactEvent.obj2);
							return true;
						}
						return false;
					});
					break;
				}
			}
		}
	}

	public enum InteractConditionType { CheckTargetComponent, OneObjectOnlyPassOnces };
}
