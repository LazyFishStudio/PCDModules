using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDSkeleton : MonoBehaviour
{
    private Dictionary<string, PCDBone> pcdBoneDict;
    private bool isInit;

    public void InitBones() {
        pcdBoneDict = new();
		PCDBone[] pcdBones = transform.GetComponentsInChildrenAndChildren<PCDBone>();
		
		string skeletonLog = gameObject.name + " PCDSkeleton Init Finish, BoneList: \n";
		foreach (PCDBone pcdBone in pcdBones) {
			AddBone(pcdBone);
			skeletonLog += pcdBone.boneName + '\n';
		}
		Debug.Log(skeletonLog);
        isInit = true;
    }

    public void AddBone(PCDBone newBone) {
        if (pcdBoneDict.ContainsKey(newBone.boneName)) {
            return;
        }
        pcdBoneDict.Add(newBone.boneName, newBone);
    }

    public PCDBone GetBone(string boneName) {
        if (!isInit) {
            InitBones();
        }
        return pcdBoneDict[boneName];
    }

}
