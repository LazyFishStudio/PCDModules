using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;

namespace PCD.Narrative
{
	public class BoxNPC : SimpleNPC
	{
		public string questName = "BoxQuest";

		public override bool OnInteract(InteractComp interactor) {
			int branchIdx = EasyQuest.IsQuestActive(questName) ? 1 : 0;
			if (branchIdx == 1 && interactor.holdingItem == null)
				branchIdx = 2;

			if (branchIdx == 1) {
				EasyQuest.RegisterQuest(questName, onFinish: () => {
					Destroy(interactor.holdingItem);
				});
			}

			ChatManager.Instance.controller.StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
			ChatManager.Instance.controller.graph.blackboard.SetVariableValue("BranchIdx", branchIdx);
			// GetComponent<DialogueTreeController>().StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
			// GetComponent<DialogueTreeController>().graph.blackboard.SetVariableValue("BranchIdx", branchIdx);
			return true;
		}
	}
}
