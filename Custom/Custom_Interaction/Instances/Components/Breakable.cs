using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
	public float breakThreshold = 16f;
	public GameObject brokenProduct;
	public Vector3 brokenBias = new Vector3(0f, 1f, 0f);
	private bool broken = false;

	private void OnItemBroken() {
		if (broken)
			return;
		broken = true;

		var newItem = Instantiate(brokenProduct, null);
		newItem.transform.position = transform.position + brokenBias;
		Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
			if (collision.relativeVelocity.magnitude > breakThreshold)
				OnItemBroken();
		}
	}
}
