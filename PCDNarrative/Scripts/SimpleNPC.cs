using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
	public class SimpleNPC : PCDTriggerInteractable
	{
		public DialogueTree dialogueTree;

		public override bool OnInteract(InteractComp interactor) {
			if (ChatManager.Instance.controller)
				;

			ChatManager.Instance.controller.StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
			return true;
		}
	}
}
