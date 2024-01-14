using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDFaceTextureManager : SingletonMono<PCDFaceTextureManager> {
    public Material[] faceMats;
    public int faceMatNum => faceMats.Length;
    public Material GetMatByIndex(int index) {
        return faceMats[index];
    }
}
