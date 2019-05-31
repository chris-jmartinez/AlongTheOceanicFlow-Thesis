using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGame : MonoBehaviour {

    public static UIGame instance = null;
    public FollowVerticallyTheCamera followVerticallyScript;
    public GameObject health, hunger, score, startGameMenu, continueGameMenu, x2Banner;

    public AudioSource healthUIAudioSfx, energyUIAudioSfx;
    public AudioClip healthElectricity;
    public ParticleSystem healthIncreased, energyIncreased;
    public bool UIGameFollowingCameraIsEnabled
    {
        get { return followVerticallyScript.FollowingEnabled; }
        set { followVerticallyScript.FollowingEnabled = value; }
    }

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        
    }

    public void PlayHealthDecreased()
    {
        if (DataController.instance.audioEffectsIsActive)
            healthUIAudioSfx.PlayOneShot(healthElectricity);
    }

    public void PlayParticleHealthIncreased()
    {
        healthIncreased.Play();
    }

    public void PlayParticleEnergyIncreased()
    {
        energyIncreased.Play();
    }

    public void SetActiveLowEnergySound(bool value)
    {
        if (!DataController.instance.audioEffectsIsActive)
            return;

        if (value == true)
            energyUIAudioSfx.enabled = true;
        else
            energyUIAudioSfx.enabled = false;
    }
    

    
}
