using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.SaveSystem.Test
{
    public class SaveBase : MonoBehaviour
    {
        public int saveValue = 0;
        public int loadValue = 10;

        public void ClickSave() {
            PCDSaveMgr.PCDSaveAll();
        }

        public void ClickLoad() {
            PCDSaveMgr.PCDLoadAll();
        }
    }
}
