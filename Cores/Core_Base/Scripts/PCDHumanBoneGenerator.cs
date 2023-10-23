using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanBoneGenerator : MonoBehaviour {
    public PCDHuman human;
    public bool generateHumanBone;
    public float pelvisHeight = 0.85f;
    public float pelvisWidth = 0.5f;
    public float shoulderHeight = 1.65f;
    public float shoulderWidth = 0.5f;
    public Vector3 lPelvisPosLocal => new Vector3(-0.5f * pelvisWidth, pelvisHeight, 0);
    public Vector3 rPelvisPosLocal => new Vector3(0.5f * pelvisWidth, pelvisHeight, 0);
    public Vector3 lShoulderPosLocal => new Vector3(-0.5f * shoulderWidth, shoulderHeight, 0);
    public Vector3 rShoulderPosLocal => new Vector3(0.5f * shoulderWidth, shoulderHeight, 0);

    void OnValidate() {
        if (generateHumanBone) {
            human.humanBone.lPelvis.localPosition = lPelvisPosLocal;
            human.humanBone.rPelvis.localPosition = rPelvisPosLocal;
            human.humanBone.lFoot.localPosition = lPelvisPosLocal.ClearY();
            human.humanBone.rFoot.localPosition = rPelvisPosLocal.ClearY();
            human.humanBone.lShoulder.localPosition = lShoulderPosLocal;
            human.humanBone.rShoulder.localPosition = rShoulderPosLocal;
            human.humanBone.lHand.localPosition = lShoulderPosLocal + lShoulderPosLocal.ClearY() * 3.0f;
            human.humanBone.rHand.localPosition = rShoulderPosLocal + rShoulderPosLocal.ClearY() * 3.0f;
            generateHumanBone = false;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;

        Gizmos.DrawLine(human.humanBone.root.position + lPelvisPosLocal, human.humanBone.root.position + rPelvisPosLocal);
        Gizmos.DrawLine(human.humanBone.root.position + lShoulderPosLocal, human.humanBone.root.position + rShoulderPosLocal);

        Gizmos.DrawLine(human.humanBone.root.position + lPelvisPosLocal, human.humanBone.root.position + lPelvisPosLocal.ClearY());
        Gizmos.DrawLine(human.humanBone.root.position + rPelvisPosLocal, human.humanBone.root.position + rPelvisPosLocal.ClearY());
        Gizmos.DrawLine(human.humanBone.root.position + lShoulderPosLocal, human.humanBone.root.position + lShoulderPosLocal + lShoulderPosLocal.ClearY() * 3.0f);
        Gizmos.DrawLine(human.humanBone.root.position + rShoulderPosLocal, human.humanBone.root.position + rShoulderPosLocal + rShoulderPosLocal.ClearY() * 3.0f);


    }
    
}
