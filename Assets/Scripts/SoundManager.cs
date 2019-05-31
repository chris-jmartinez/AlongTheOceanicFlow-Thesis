using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance = null;

    public GameObject cameraColliderAudio;
    public AudioMixer audioMixerSoundtracks;
    private AudioSource cameraAudioSource1;
    private AudioSource cameraAudioSource2;
    //public AudioSource playfieldAudioSource;
    public AudioClip soundtrackMainMenu;
    public AudioClip soundtrackTravelFlow;
    public AudioClip soundtrackX2;
    public AudioClip soundtrackTreasureHunt;
    public AudioClip soundtrackWinSurvived;
    public AudioClip soundtrackLoseDeath;
    public AudioClip soundtrackSonar;

    #region internal vars
    bool currentPlayingIsSource1 = false; //is Source1 currently the active AudioSource (playing some sound right now)

    Coroutine CurSourceFadeRoutine = null;
    Coroutine NewSourceFadeRoutine = null;

    IEnumerator pitchTransitionCor;
    public float targetPitch { get; private set; }
    #endregion


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }




    private void Start()
    {
        AudioSource[] audioSourcesCamera = cameraColliderAudio.GetComponents<AudioSource>();

       
        if (audioSourcesCamera[0] == null)
            cameraAudioSource1 = cameraColliderAudio.AddComponent<AudioSource>();
        else
            cameraAudioSource1 = audioSourcesCamera[0];
        

        if (audioSourcesCamera[1] == null)
            cameraAudioSource2 = cameraColliderAudio.AddComponent<AudioSource>();  
        else
            cameraAudioSource2 = audioSourcesCamera[1];

        Invoke("PlaySonar", 1.5f);
    }

    private void PlaySonar()
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackSonar, 0.4f, 1f, false);
    }



    public void PlaySoundtrackMainMenu(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackMainMenu, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
    }

    public void PlaySoundtrackTravelFlow(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackTravelFlow, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
        //Example: CrossFade(soundtrackTravelFlow, 1f, 5f, false);
    }

    public void PlaySoundtrackX2(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackX2, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
    }



    public void PlaySoundtrackTreasureHunt(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackTreasureHunt, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
    }

    public void PlaySoundtrackWinSurvived(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackWinSurvived, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
    }

    public void PlaySoundtrackLoseDeath(float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (DataController.instance.musicIsActive)
            CrossFade(soundtrackLoseDeath, volume, fadingTime, playFromThisPoint, delay_before_crossFade);
    }

    //Gradually shifts the sound comming from our audio sources to the this clip:
    //Volume must be between 0 and 1; playFromThisPoint plays the new track starting from the same second the current track is being stopped; delay is optional parameter
    public void CrossFade(AudioClip clipToPlay, float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        StartCoroutine(Fade(clipToPlay, volume, fadingTime, playFromThisPoint, delay_before_crossFade));

    }


    //Coroutine for changing track, fading out the currentPlayingAudioSource and fading in the newAudioSource 
    private IEnumerator Fade(AudioClip playMe, float volume, float fadingTime, bool playFromThisPoint, float delay_before_crossFade = 0)
    {
        if (delay_before_crossFade > 0)
        {
            yield return new WaitForSeconds(delay_before_crossFade);
        }

        AudioSource curActiveSource, newActiveSource;
        if (currentPlayingIsSource1)
        {
            //Source1 is currently playing the most recent AudioClip
            curActiveSource = cameraAudioSource1;
            //so launch on Source2
            newActiveSource = cameraAudioSource2;
        }
        else
        {
            //otherwise, Source2 is currently active
            curActiveSource = cameraAudioSource2;
            //so play on Source1
            newActiveSource = cameraAudioSource1;
        }

        //perform the switching
        newActiveSource.clip = playMe;
        if (playFromThisPoint && curActiveSource.timeSamples < playMe.samples)
            newActiveSource.timeSamples = curActiveSource.timeSamples; //Alternative: newActiveSource.time = curActiveSource.time; (but time doesn't reflect actual timing of track, if track is compressed like an mp3)
        else
            newActiveSource.timeSamples = 0; //Alternative: newActiveSource.time = 0;
        newActiveSource.Play();
        newActiveSource.volume = 0;

        if (CurSourceFadeRoutine != null)
        {
            StopCoroutine(CurSourceFadeRoutine);
        }

        if (NewSourceFadeRoutine != null)
        {
            StopCoroutine(NewSourceFadeRoutine);
        }

        CurSourceFadeRoutine = StartCoroutine(FadeSource(curActiveSource, curActiveSource.volume, 0, fadingTime));
        NewSourceFadeRoutine = StartCoroutine(FadeSource(newActiveSource, newActiveSource.volume, volume, fadingTime));

        currentPlayingIsSource1 = !currentPlayingIsSource1;

        yield break;
    }


    public IEnumerator FadeSource(AudioSource sourceToFade, float startVolume, float endVolume, float duration)
    {
        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;

            sourceToFade.volume = Mathf.Clamp01(Mathf.Lerp(startVolume, endVolume, elapsed / duration));

            if (sourceToFade.volume == endVolume)
            {
                break;
            }

            yield return null;
        }//end while
    }




    //returns false if BOTH sources are not playing and there are no sounds are staged to be played.
    //also returns false if one of the sources is not yet initialized
    public bool IsPlaying
    {
        get
        {
            if (cameraAudioSource1 == null || cameraAudioSource2 == null)
            {
                return false;
            }

            //otherwise, both sources are initialized. See if any is playing:
            return cameraAudioSource1.isPlaying || cameraAudioSource2.isPlaying;
        }//end get
    }

    public void StopMusic()
    {
        //if (currentPlayingIsSource1)
        //    cameraAudioSource1.Stop();
        //else
        //    cameraAudioSource2.Stop();
        if (IsPlaying)
        {
            cameraAudioSource1.Stop();
            cameraAudioSource2.Stop();
        }
        
    }

    public void LowerCurrentMusic(float endVolume, float duration)
    {

        if (!DataController.instance.musicIsActive || !IsPlaying)
            return;

        AudioSource currentSource;
        if (currentPlayingIsSource1)
            currentSource = cameraAudioSource1;
        else
            currentSource = cameraAudioSource2;

        AudioClip currentTrack = currentSource.clip;

        StartCoroutine(FadeSource(currentSource, currentSource.volume, endVolume, duration));
    }

    void SetPitch(float pitch)
    {
        if (!DataController.instance.musicIsActive)
            return;

        targetPitch = pitch;

        if (pitchTransitionCor != null)
        {
            StopCoroutine(pitchTransitionCor);
        }

        AudioSource currentSource;
        if (currentPlayingIsSource1)
            currentSource = cameraAudioSource1;
        else
            currentSource = cameraAudioSource2;

        currentSource.pitch = pitch;
    }

    public void PitchTransition(float target, float duration)
    {
        if (!DataController.instance.musicIsActive)
            return;

        // set new target pitch
        targetPitch = target;
        // stop any pitch transition currently running
        if (pitchTransitionCor != null)
        {
            StopCoroutine(pitchTransitionCor);
        }

        if (duration <= Mathf.Epsilon && duration >= -Mathf.Epsilon)
        {
            SetPitch(target);
        }
        else
        {
            // create and start a new transition
            pitchTransitionCor = PitchTransitionCoroutine(target, duration);
            StartCoroutine(pitchTransitionCor);
        }
        
    }

    IEnumerator PitchTransitionCoroutine(float target, float duration)
    {
        AudioSource currentSource;
        if (currentPlayingIsSource1)
            currentSource = cameraAudioSource1;
        else
            currentSource = cameraAudioSource2;

        // starting pitch of the audio
        float from = currentSource.pitch;

        float invDuration = 1.0f / duration;

        // "counter" variable to track position within Lerp
        float progress = Time.unscaledDeltaTime * invDuration;

        // loop until we reach the end of the linear interpolation
        while (Mathf.Abs(currentSource.pitch - target) > 0.0f)
        {
            // linear interpolation from starting to target pitch
            currentSource.pitch = Mathf.Lerp(from, target, progress);

            // increase the "counter" by the fraction of duration
            progress += Time.unscaledDeltaTime / invDuration;
           
            // yield control back to the program
            yield return null;
        }
    }

    public void ReactivateMusic()
    {
        PlaySoundtrackMainMenu(0.4f, 1f, false);
    }


}
