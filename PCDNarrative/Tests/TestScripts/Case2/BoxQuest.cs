using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
    public class BoxQuest : MonoBehaviour
    {
        public DialogueTree dialogueTree;

        private void Awake() {
            EasyEvent.RegisterOnceCallback("PickQuestBox", () => {
                var player = ChatManager.Instance.player;
                player.GetComponent<DialogueTreeController>().StartDialogue(dialogueTree, player.GetComponent<DialogueActor>(), null);
            });
        }
    }
}

