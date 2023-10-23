using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.QuestSystem
{
    public interface PCDQuest {
        public string questName { get; set; }
        public string questDesc { get; set; }
        public GameObject questGFXPrefab { get; }
        public abstract QuestProgress RefreshQuestProgress(out bool needRefresh);
        public abstract bool CheckQuestFinished();
        public abstract void OnQuestFinish();
    }

    public abstract class PCDQuestBase : MonoBehaviour, PCDQuest
    {
        [SerializeField] protected string _questName;
        [SerializeField] protected string _questDesc;
        [SerializeField] protected GameObject _questGFXPrefab;
        public string questName { get => _questName; set => _questName = value; }
        public string questDesc { get => _questDesc; set => _questDesc = value; }
        public GameObject questGFXPrefab { get => _questGFXPrefab; }
        public abstract QuestProgress RefreshQuestProgress(out bool needRefresh);
        public abstract bool CheckQuestFinished();
        public abstract void OnQuestFinish();
    }

    public class QuestProgress
    {
        public bool finished;
        public string details;
        public QuestProgress nextItem;
        public QuestProgress subItem;

        public QuestProgress(bool finished, string details, QuestProgress nextItem = null, QuestProgress subItem = null) {
            this.finished = finished;
            this.details = details;
            this.nextItem = nextItem;
            this.subItem = subItem;
        }
    }
}
