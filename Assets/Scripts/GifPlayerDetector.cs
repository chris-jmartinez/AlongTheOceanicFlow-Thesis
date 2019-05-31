using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GifPlayerDetector : MonoBehaviour {

    public Animator animatorParent;
    private void Start()
    {
        //animatorParent = transform.parent.GetComponent<Animator>();
        //animatorParent.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("CameraCollider"))
            animatorParent.enabled = true; //animatorParent.SetBool("Activated", true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("CameraCollider"))
            animatorParent.enabled = false; //animatorParent.SetBool("Activated", false);
    }
}
