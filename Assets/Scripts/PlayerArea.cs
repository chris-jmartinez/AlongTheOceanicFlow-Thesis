using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArea : MonoBehaviour {

    public DolphinTrackableEventHandler playerTagScriptEvent;

    public UiMenuHandler uiMenuHandler;
    public GameController gameController;
    public GameObject goIntoAuraMessage;
    public GameObject directionalIndicatorToAura;
    public GameObject particlesArea;
    public GameObject uiGame;
    public AudioSource auraSoundSource;

    private bool coroutineTimerOutsideAuraIsRunning;
    private Coroutine timerOutsideAuraCoroutine = null;
    //private float volumeAuraSoundSource;

    

    public int timerSecondsOutsideAura = 10;

    public bool auraEntered = false;
    public bool AuraActive { get; set; }
    public bool AuraFound { get; set; }

    private void Start()
    {
        //volumeAuraSoundSource = auraSoundSource.volume;

    }
    public void SetActiveGoIntoAuraMessage(bool value)
    {
        if (value == true)
            goIntoAuraMessage.SetActive(true);
        else if (value == false)
        {
            goIntoAuraMessage.SetActive(false);
        }
    }

    public void SetActiveParticlesAura(bool value)
    {
        if (value == true)
            particlesArea.SetActive(true);
        else if (value == false)
        {
            particlesArea.SetActive(false); 
        }
    }


    public void SetActiveIndicatorToAura(bool value)
    {
        if (value == true)
            directionalIndicatorToAura.SetActive(true); 
        else if (value == false)
        {
            directionalIndicatorToAura.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!AuraActive)
        {
            return;
        }

        if (!AuraFound)
        {
            AuraFound = true;
            directionalIndicatorToAura.SetActive(false);
            directionalIndicatorToAura.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = false;
        }
        
        if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.PRE_TRAVEL_FLOW
                                                                && (other.tag.Contains("CameraCollider") || other.tag.Contains("Player")) && (playerTagScriptEvent.detected || !DataController.instance.smartObjectIsActive)
                                                                && !auraEntered) //Conditions in AND so the player must get the dolphin and enter the aura to start the game
        {

            auraEntered = true; //With this bool we detect the entering in the aura just one time
            uiGame.GetComponent<UIGame>().UIGameFollowingCameraIsEnabled = true;

            AuraSetActive(false);

            StartCoroutine(StartGameAfterTime());


        }
        else if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TRAVEL_FLOW
                                                                    && other.tag.Contains("CameraCollider") && !auraEntered)
        {
            if (coroutineTimerOutsideAuraIsRunning)
            {
                StopCoroutine(TimerOutsideAura());
                goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Prendi il delfino\ne vai dentro\nl'aura!";
            }
            auraEntered = true;

            AuraSetActive(false);

        }
    }


    private void AuraSetActive(bool value)
    {
        if (value == false)
        {
            auraSoundSource.enabled = false;
            goIntoAuraMessage.SetActive(false);
            directionalIndicatorToAura.SetActive(false);
            directionalIndicatorToAura.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = false;
            particlesArea.SetActive(false);
        }
        else
        {
            auraSoundSource.enabled = true;
            uiMenuHandler.PutInFrontOfCamera(goIntoAuraMessage);
            goIntoAuraMessage.SetActive(true);
            particlesArea.SetActive(true);
            //directionalIndicatorToAura.SetActive(true);
        }
    }

   

    private IEnumerator StartGameAfterTime()
    {
        GameController.instance.currentPhaseOfLevel = GameController.PhaseOfLevel.TRAVEL_FLOW; //Placed to avoid player going out from aura in the positioning of UIGame, cheating because timer won't start (and causing some bugs, probably)
        yield return new WaitForSeconds(1.5f);
        uiGame.GetComponent<UIGame>().UIGameFollowingCameraIsEnabled = false;
        GameController.instance.StartGame();
    }




    private void OnTriggerExit(Collider other)
    {
        if (!AuraActive)
        {
            return;
        }



        if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.PRE_TRAVEL_FLOW
                                                 && other.tag.Contains("CameraCollider") && auraEntered)
        {

            goIntoAuraMessage.SetActive(true);
            auraSoundSource.enabled = true;
            //directionalIndicatorToAura.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = true;
            //directionalIndicatorToAura.SetActive(true);
            //particlesArea.SetActive(true);
            auraEntered = false;


        }
        else if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TRAVEL_FLOW
                                                                    && other.tag.Contains("CameraCollider") && auraEntered)
        {
            Debug.Log("Usciti da aura in travel flow! Countdown prima di decrementare health");
            if (timerOutsideAuraCoroutine != null)
            {
                StopCoroutine(timerOutsideAuraCoroutine);
                timerOutsideAuraCoroutine = StartCoroutine(TimerOutsideAura());
            }               
            else
                timerOutsideAuraCoroutine = StartCoroutine(TimerOutsideAura());
        }

    }

   


    private IEnumerator TimerOutsideAura()
    {
        coroutineTimerOutsideAuraIsRunning = true;
        
        auraEntered = false;
        AuraSetActive(true);

        if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL  || GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL_BASIC)
        {
            goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Ritorna nell'aura!";
            yield break;
        }

        int currentTimerSeconds = timerSecondsOutsideAura;

        while (!auraEntered)
        {

            if (!AuraActive)
            {
                break;
            }

            goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Ritorna nell'aura\no perderai vita tra\n" + currentTimerSeconds;
           
            yield return new WaitForSeconds(1f);

            if (currentTimerSeconds == 0)
            {
                break;
            }
            currentTimerSeconds--;

        }

        

        while (currentTimerSeconds == 0 && !auraEntered)
        {
            if (!AuraActive)
            {
                break;
            }

            if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL  || GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL_BASIC)
            {
                break;
            }

            GameController.instance.HealthPlayer--;
            //Play audio losing health
            goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "PERDI VITA!\nTorna nell'aura!\n" + currentTimerSeconds;

            if (GameController.instance.HealthPlayer == 0)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
        }

        //stop fast music etc
        goIntoAuraMessage.SetActive(false);
        goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Prendi il delfino\ne vai dentro\nl'aura!";
        particlesArea.SetActive(false);


        coroutineTimerOutsideAuraIsRunning = false;
    }
}

