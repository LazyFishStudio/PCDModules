using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace PCD.QuestSystem
{
	public class SimpleQuestGFX : QuestGFX
	{
		public TextMeshProUGUI questNameText;
		public TextMeshProUGUI questDescText;
		public TextMeshProUGUI questProgressText;

		public override void RefreshQuestInfo(PCDQuest quest, QuestProgress progress) {
			questNameText.text = quest.questName;
			questDescText.text = quest.questDesc;
			questProgressText.text = GetProgressString(progress, "");
		}

		private string GetProgressString(QuestProgress progress, string indent) {
			string res = "";
			if (progress.finished)
				res += indent + "[¡Ì] <s>" + progress.details + "</s>";
			else
				res += indent + "[ ] " + progress.details;
			if (progress.nextItem != null)
				res += "\n" + GetProgressString(progress.nextItem, indent);
			return res;
		}
	}
}
