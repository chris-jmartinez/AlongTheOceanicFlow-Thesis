using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintPanel : MonoBehaviour {

    private bool enteredTrigger = false;
    public GameObject hintPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (!enteredTrigger && (other.tag == "Player" || other.tag == "CameraCollider"))
        {
            enteredTrigger = true;
            if (hintPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CangrejoOneEyePanelIdle"))
                hintPanel.GetComponent<Animator>().SetBool("Appear", true);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enteredTrigger && (other.tag == "Player" || other.tag == "CameraCollider"))
        {
            enteredTrigger = false;
            if (hintPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CangrejoOneEyePanelAppears"))
                hintPanel.GetComponent<Animator>().SetBool("Appear", false);

        }
    }
}
