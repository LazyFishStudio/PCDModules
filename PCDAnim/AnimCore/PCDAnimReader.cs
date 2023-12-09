using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDAnimReader
{
	private Dictionary<string, PCDKFReader> keyFrameDict;

	public PCDAnimReader(PCDAnim animSO) {
		keyFrameDict = new Dictionary<string, PCDKFReader>();
		foreach (var item in animSO.keyFrameList) {
			keyFrameDict[item.keyFrameName] = new PCDKFReader(item.keyFramePrefab);
		}
	}

	public PCDKFReader GetKeyFrameReader(string keyFrameName) {
		return keyFrameDict[keyFrameName];
	}
}
