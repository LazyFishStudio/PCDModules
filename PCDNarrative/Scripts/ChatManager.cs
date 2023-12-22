using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
    /* Whole chat script -> script, single line of chat -> chat */
    public class ChatManager : SingletonMono<ChatManager>
    {
        public GameObject chatBoxUI;
        public DialogueTreeController controller;

        private void Awake() {
            controller = GetComponent<DialogueTreeController>();
        }
    }
}
