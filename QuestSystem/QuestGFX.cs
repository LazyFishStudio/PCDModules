using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.QuestSystem
{
    public abstract class QuestGFX : MonoBehaviour {
        public abstract void RefreshQuestInfo(PCDQuest quest, QuestProgress progress);
    }
}
