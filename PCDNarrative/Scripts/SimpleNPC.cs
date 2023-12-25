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
			ChatManager.Instance.controller.StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
			EasyEvent.TriggerEvent(string.Format("TalkWith[{0}]", GetComponent<DialogueActor>().name));
			return true;
		}
	}
}
