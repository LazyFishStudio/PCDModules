using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCD.EventSystem;
using PCD.QuestSystem;
using MoreMountains.Tools;

public class CatQuest : MonoBehaviour, PCDQuest, PCDSimpleListener, MMEventListener<InteractEvent>
{
	[SerializeField] private string _questName = "CatQuest";
	[SerializeField] private string _questDesc = "Cat need your help!";
	[SerializeField] private GameObject _questGFXPrefab;
	public string questName { get => _questName; set => _questName = value; }
	public string questDesc { get => _questDesc; set => _questDesc = value; }
	public GameObject questGFXPrefab { get => _questGFXPrefab; }


	public GameObject player;
	public GameObject cat;
	public BoxCollider questArea;

	private string stage = "Stage1";

	private void Start() {
		InteractEventConfig config = new InteractEventConfig(questName, InteractType.InteractStart, player, cat);
		InteractEventer.RegisterEvent(config);
	}

	public void OnMMEvent(InteractEvent interactEvent) {
		if (stage == "Finished" || interactEvent.eventName != questName)
			return;
		if (stage == "Stage1") {
			stage = "Stage2";
		} else if (stage == "Stage3") {
			stage = "Finished";
			for (int i = 0; i < 2; i++)
				Destroy(vegetables[i]);
		}
	}

	private List<GameObject> vegetables;
	private void Update() {
		if (stage == "Stage1" || stage == "Finished")
			return;

		vegetables = new List<GameObject>();
		Collider[] colliders = Physics.OverlapBox(questArea.bounds.center, questArea.bounds.extents);
		foreach (var collider in colliders) {
			// if (collider.GetComponent<Seed>() != null)
			vegetables.Add(collider.gameObject);
		}
		stage = (vegetables.Count < 2 ? "Stage2" : "Stage3");
	}

	public QuestProgress RefreshQuestProgress(out bool needRefresh) {
		needRefresh = true;
		if (stage == "Stage1") {
			return new QuestProgress(false, "Goto talk with the Cat.");
		} else if (stage == "Stage2") {
			return new QuestProgress(true, "Goto talk with the Cat.",
									 new QuestProgress(false, "Collect 2 gems for Cat. (" + vegetables.Count + "/2)"));
		} else if (stage == "Stage3") {
			return new QuestProgress(true, "Goto talk with the Cat.",
									 new QuestProgress(true, "Collect 2 gems for Cat. (" + vegetables.Count + "/2)",
													   new QuestProgress(false, "Talk with the Cat again.")));
		} else if (stage == "Finished") {
			return new QuestProgress(true, "Goto talk with the Cat.",
									 new QuestProgress(true, "Collect 2 gems for Cat.",
													   new QuestProgress(true, "Talk with the Cat again.")));
		}
		return new QuestProgress(false, "");
	}

	public bool CheckQuestFinished() {
		return stage == "Finished";
	}

	public void OnQuestFinish() {

	}
}
