using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDSkeleton : MonoBehaviour
{
    private Dictionary<string, PCDBone> pcdBoneDict;
    private bool isInit;
    public PCDHumanBone humanBone;


    void Awake() {
        humanBone = new();
        humanBone.root = GetBone("Root");
        humanBone.head = GetBone("Head");
        humanBone.body = GetBone("Body");
        humanBone.lShoulder = GetBone("LShoulder");
        humanBone.rShoulder = GetBone("RShoulder");
        humanBone.lPelvis = GetBone("LPelvis");
        humanBone.rPelvis = GetBone("RPelvis");
        humanBone.lHand = GetBone("LHand");
        humanBone.rHand = GetBone("RHand");
        humanBone.lFoot = GetBone("LFoot");
        humanBone.rFoot = GetBone("RFoot");
    }

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

public class PCDHumanBone {
    public PCDBone root;
    public PCDBone head;
    public PCDBone body;
    public PCDBone lShoulder;
    public PCDBone rShoulder;
    public PCDBone lPelvis;
    public PCDBone rPelvis;
    public PCDBone lHand;
    public PCDBone rHand;
    public PCDBone lFoot;
    public PCDBone rFoot;
}
