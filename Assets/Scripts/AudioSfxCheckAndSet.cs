using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSfxCheckAndSet : MonoBehaviour {
  
    private void OnEnable()
    {
        if (!DataController.instance.audioEffectsIsActive)
            GetComponent<AudioSource>().enabled = false;
        else
            GetComponent<AudioSource>().enabled = true;
    }
}
