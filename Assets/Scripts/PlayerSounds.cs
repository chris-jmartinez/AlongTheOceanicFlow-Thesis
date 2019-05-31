using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

    public AudioSource audiosourceSfx;
    public AudioClip immersionSound;
    public AudioClip yeehawSound;
    public AudioClip yahoo;
    public AudioClip playerAirtapsButton;
    public AudioClip playerAirtapsStartButton;
    private bool immersion;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Water") && !immersion && DataController.instance.audioEffectsIsActive)
        {
            immersion = true;
            StartCoroutine(CoPlayDelayedClip(immersionSound, 0.3f));
            Invoke("ResetImmersion", 3f);
        }
    }

    private void ResetImmersion()
    {
        immersion = false;
    }

    public void PlayYeehaw()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(yeehawSound);
    }

    public void PlayYahoo()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(yahoo);
    }

    public void PlayButtonSound()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(playerAirtapsButton);
    }

    public void PlayStartButtonSound()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(playerAirtapsStartButton);
    }

    private IEnumerator CoPlayDelayedClip(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audiosourceSfx.PlayOneShot(clip);
    }
}
