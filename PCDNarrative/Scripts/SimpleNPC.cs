using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
	[RequireComponent(typeof(DialogueTreeController))]
	[RequireComponent(typeof(DialogueActor))]
	public class SimpleNPC : PCDTriggerInteractable
	{
		public DialogueTree dialogueTree;

		public override bool OnInteract(InteractComp interactor) {
			GetComponent<DialogueTreeController>().StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
			EasyEvent.TriggerEvent(string.Format("TalkWith[{0}]", GetComponent<DialogueActor>().name));
			return true;
		}
	}
}
