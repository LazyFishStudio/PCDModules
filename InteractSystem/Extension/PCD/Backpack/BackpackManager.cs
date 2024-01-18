using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Bros.Utils;
using InteractSystem;

public class BackpackManager : MonoBehaviour
{
	static public BackpackManager Inst;

	public InteractComp player;
	[HideInInspector] public int selectIdx = -1;
	[HideInInspector] public int prevSelected = -1;
	public GameObject selectItem { get { return selectIdx >= 0 ? holdingItems[selectIdx] : null; } }

	/* For Test Only */
	public GameObject backpack;
	public List<GameObject> holdingItems = new List<GameObject>();

	private void Awake() {
		if (Inst != null)
			throw new System.Exception("More than one BackpackManager exist!");
		Inst = this;
		CheckInitialized();
		if (player == null)
			player = GetComponent<InteractComp>();
		if (player == null)
			throw new System.Exception("BackpackManager component not in player!");
	}

	private void CheckInitialized() {
		if (backpack == null) {
			backpack = Instantiate(Resources.Load<GameObject>("Prefabs/BackpackUICamera"));
			Camera UICamera = Inst.backpack.GetComponentInChildren<Camera>(true);
			Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(UICamera);
		}
	}

	static public void AddItem(GameObject item) {
		if (item == null)
			throw new System.Exception("Can't add null item!");

		GameObject newItem = Instantiate(item);
		newItem.SetLayerRecursively("UI");
		newItem.GetComponent<Rigidbody>().isKinematic = true;
		newItem.transform.rotation = Quaternion.identity;
		Inst.holdingItems.Add(newItem);
	}

	static public void AddItemAndSelect(GameObject item) {
		AddItem(item);
		SelectInBackpack(Inst.holdingItems.Count - 1);
	}

	static public void RemoveSelectingItem() {
		int idx = Inst.selectIdx;
		if (idx < 0 || idx >= Inst.holdingItems.Count)
			throw new System.Exception("Can't remove invalid item!");

		GameObject item = Inst.holdingItems[idx];
		Inst.holdingItems.RemoveAt(idx);
		Destroy(item);
		SelectInBackpack(-1);
		Inst.prevSelected = -1;
	}

	static public void SelectInBackpack(int idx) {
		if (idx >= Inst.holdingItems.Count)
			throw new System.Exception("Can't select invalid item!");

		Inst.prevSelected = idx < 0 ? Inst.selectIdx : -1;
		Inst.selectIdx = idx;
	}

	static public PickableObject CreateBackpackItem(int idx) {
		GameObject newItem = Instantiate(Inst.holdingItems[idx]);
		newItem.SetLayerRecursively("Default");
		newItem.GetComponent<Rigidbody>().isKinematic = false;
		return newItem.GetComponent<PickableObject>();
	}
}
