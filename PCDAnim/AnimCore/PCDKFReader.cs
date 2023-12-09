using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDKFReader
{
	private Dictionary<string, BoneTransInfo> boneDict;

	public PCDKFReader(GameObject keyFramePrefab) {
		boneDict = new Dictionary<string, BoneTransInfo>();
		for (int i = 0; i < keyFramePrefab.transform.childCount; i++) {
			Transform child = keyFramePrefab.transform.GetChild(i);
			boneDict[child.name] = new BoneTransInfo(child.localPosition, child.localRotation, child.localScale);
		}
	}

	public BoneTransInfo GetBoneInfo(string boneName) {
		return boneDict[boneName];
	}
}

public struct BoneTransInfo
{
	public Vector3 localPosition;
	public Quaternion localRotation;
	public Vector3 localScale;

	public BoneTransInfo(Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
		this.localPosition = localPosition;
		this.localRotation = localRotation;
		this.localScale = localScale;
	}
}
