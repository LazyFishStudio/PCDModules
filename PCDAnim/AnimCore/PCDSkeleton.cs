using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDSkeleton
{
    private Dictionary<string, PCDBone> pcdBoneDict = new Dictionary<string, PCDBone>();

    public void AddBone(PCDBone newBone) {
        if (pcdBoneDict.ContainsKey(newBone.boneName)) {
            return;
        }
        pcdBoneDict.Add(newBone.boneName, newBone);
    }

    public PCDBone GetBone(string boneName) {
        return pcdBoneDict[boneName];
    }

}
