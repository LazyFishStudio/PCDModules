using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PCD.QuestSystem
{
    public class QuestGFXManager : SingletonMono<QuestGFXManager>
	{
		public GameObject questGFXUI;
		public float padding = 50f;
		public GameObject questFinishEffectPrefab;
		public Transform questFinishEffectSpawnPos;
		public Dictionary<PCDQuest, QuestGFX> gfxDict;

		private void Awake() {
			gfxDict = new Dictionary<PCDQuest, QuestGFX>();
		}

		private void Update() {
			float totalHeight = 0;
			List<PCDQuest> pendingRemove = new List<PCDQuest>();
			foreach (var quest in QuestSystem.GetInstance().questList) {
				if (quest.CheckQuestFinished()) {
					pendingRemove.Add(quest);
					continue;
				}
				
				if (!gfxDict.TryGetValue(quest, out QuestGFX gfx)) {
					GameObject gfxPrefab = quest.questGFXPrefab;
					if (gfxPrefab?.GetComponent<QuestGFX>() == null)
						continue;
					gfx = Instantiate(gfxPrefab, questGFXUI.transform).GetComponent<QuestGFX>();
					gfxDict[quest] = gfx;
				}
				// gfx.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
				gfx.RefreshQuestInfo(quest, quest.RefreshQuestProgress(out bool needRefresh));
				gfx.transform.position = gfx.transform.position.CopySetY(questGFXUI.transform.position.y - totalHeight);
				totalHeight += gfx.GetComponent<RectTransform>().rect.height * gfx.transform.lossyScale.y + padding;
			}

			foreach (var quest in pendingRemove) {
				if (gfxDict.TryGetValue(quest, out QuestGFX questGFX)) {
					Destroy(questGFX.gameObject);
				}
				QuestSystem.RemoveQuest(quest);
				Instantiate(questFinishEffectPrefab, questFinishEffectSpawnPos.position, questFinishEffectSpawnPos.rotation);
				quest.OnQuestFinish();
			}
		}
	}
}
