using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEventTrigger : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M)) {
            EasyEvent.TriggerEvent("TutorialEvent-1-Finish");
		}
    }
}
