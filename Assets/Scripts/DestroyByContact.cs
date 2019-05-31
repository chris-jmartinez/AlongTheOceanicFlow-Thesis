using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyByContact : MonoBehaviour {

    
    private Animator animator;
    private GameController gameController;
    private bool elementTaken = false;
    private bool hazardAttacking = false;
    private bool settingLedWrongAngle = false;

    public int points;
    [Range(0f, 0.2f)]
    public float energy;
    public AudioSource audiosourceSfx;
    public AudioClip elementGeneration;
    public AudioClip elementCaughtEffect;
    public AudioClip electricityBoundary;
    public AudioClip wrongEffect;


	// Use this for initialization
	void Start () {

        GameObject gameControllerObject = GameObject.FindWithTag("GameController"); //cercherà il primo oggetto nella scena con questo tag
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.Log("Cannot find 'gamecontroller' script");
        }

        
         animator = GetComponent<Animator>();
        
        
	}
	
	

    private void OnEnable()
    {
        //Function called when the object is reactivated (we're using Object pooling) (generated)
        //It can happen for various reasons that those booleans are still true even though they shouldn't be, so we put them to false
        elementTaken = false;
        hazardAttacking = false;
        settingLedWrongAngle = false;
        PlaySfxElementGeneration();
    }


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Player" || other.tag == "CameraCollider")
        {

            if (gameObject.name.Contains("TorusBubble") && !elementTaken) {

                ManageDolphinDetection(other.gameObject);
                
            }
            else if ( gameObject.name.Contains("Food") && !elementTaken)
            {
                elementTaken = true;
                animator.SetTrigger("OctopusTaken");
                StartCoroutine(deactivateAfterTime());
            }
            else if (gameObject.name.Contains("Hazard") && !hazardAttacking)
            {
                hazardAttacking = true;
                animator.SetBool("Attacking", true);
                StartCoroutine(deactivateAfterTime());

            }
            else if (gameObject.name.Contains("Obstacle") && !elementTaken)
            {
                elementTaken = true;
                animator.SetBool("Taken", true);
                StartCoroutine(deactivateAfterTime());

            }
            else if (gameObject.name.Contains("HealthPowerup") && !elementTaken)
            {
                elementTaken = true;
                animator.SetTrigger("1UpTaken");
                StartCoroutine(deactivateAfterTime());

            }
            else if (gameObject.name.Contains("DoubleX2Powerup") && !elementTaken)
            {
                elementTaken = true;
                animator.SetTrigger("X2Taken");

                PlaySfxElementCaught();

                if (!gameController.DoubleX2)
                {
                    this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; //If the Powerup x2 has been taken, we must stop its velocity otherwise it will hit boundaries deactivating its script and the x2 timer 
                    StartCoroutine(TimerX2());
                }
                else 
                {
                    GameController.instance.DoubleX2CurrentTimer = gameController.levelsParams[gameController.currentLevel].doubleX2Timer;
                    StartCoroutine(deactivateAfterTime()); //If the x2 powerup has been already taken, this one just resets the x2timer, and deactivates itself
                }

            }


        }
    }


    private void ManageDolphinDetection(GameObject dolphinTag)
    {
        if (CorrectRotationDolphin(dolphinTag))
        {
            elementTaken = true;
            animator.SetTrigger("TorusBubbleTaken");
            Debug.Log("Torus Taken, animator triggered");

            if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL)
                GameController.instance.CounterCaughtRingsTutorial++;
            else if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL_BASIC && !GameController.instance.GeneratorIsMoving)
            {
                if (!(GameController.instance.GeneratorMoveRight || GameController.instance.GeneratorMoveLeft || GameController.instance.GeneratorMoveCenterAndDown || GameController.instance.GeneratorMovementComplete))
                    GameController.instance.CounterCaughtRingsTutorial++;
                switch (GameController.instance.CounterCaughtRingsTutorial)
                {
                    case 1:
                        GameController.instance.GeneratorMoveRight = true;
                        break;
                    case 2:
                        GameController.instance.GeneratorMoveLeft = true;
                        break;
                    case 3:
                        GameController.instance.GeneratorMoveCenterAndDown = true;
                        break;
                    case 4:
                        GameController.instance.GeneratorMovementComplete = true;
                        break;
                }
            }

            StartCoroutine(deactivateAfterTime());
        }
        else
        {
            Debug.Log("Dolphin not correctly rotated");
            elementTaken = true;
            PlaySfxWrong();
            animator.SetTrigger("Wrong");
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerArrow>().ArrowSetQuiteOk();
            GameObject.FindGameObjectWithTag("ArrowCamera").GetComponent<PlayerArrow>().ArrowSetQuiteOk();
            if (settingLedWrongAngle == false)
            {               
                settingLedWrongAngle = true;                
                StartCoroutine(WrongSetLed());               
            }

            StartCoroutine(WrongRingDeactivate());
        }
    }


    private IEnumerator WrongSetLed()
    {
        WebClientSender.instance.SetLedRed();
        yield return new WaitForSeconds(1f);
        //WebClientSender.instance.ResetLedDolphin();
        settingLedWrongAngle = false;
    }

    private IEnumerator CorrectSetLed()
    {
        WebClientSender.instance.SetLedGreen();
        yield return new WaitForSeconds(1f);
        //WebClientSender.instance.ResetLedDolphin();
    }

    private bool CorrectRotationDolphin(GameObject other)
    {
        //Computing the deltaAngle (difference of rotation) between the dolphin arrow and the bubble arrow
        //float diffX = Mathf.Abs(Mathf.DeltaAngle(this.transform.eulerAngles.x, other.transform.eulerAngles.x));
        float diffX = Vector3.Angle(this.transform.up, other.transform.up);
        float diffY = Vector3.Angle(this.transform.right, other.transform.right);
        float diffZ = Vector3.Angle(this.transform.forward, other.transform.forward);

        float errorToleranceRings = GameController.instance.levelsParams[GameController.instance.currentLevel].errorToleranceRings;

        //if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL)
        //    errorToleranceRings = 10f;  //Decomment this line if you want a fixed error tolerance for tutorials and not customizable from the infinity settings

        //If that difference is less than a threshold, the bubble is taken
        if (diffX <= errorToleranceRings && diffY <= errorToleranceRings && diffZ <= errorToleranceRings)
        {
            Debug.LogFormat("Correct rotation, bubble taken. Values diff X Y Z: {0}  {1}  {2} ", diffX, diffY, diffZ);
            
            return true;
        }

        Debug.LogFormat("Incorrect rotation. Diff X Y Z: {0}  {1}  {2} ", diffX, diffY, diffZ);

        return false;

    }

    private IEnumerator deactivateAfterTime()
    {
        if (elementCaughtEffect != null)
            PlaySfxElementCaught();

        if (gameObject.name.Contains("HealthPowerup"))
        {
            WebClientSender.instance.SetLedGreen();
            yield return new WaitForSeconds(1f);
            gameController.HealthPlayer += 1;
            this.gameObject.SetActive(false);
            elementTaken = false;
        }

        if (gameObject.name.Contains("DoubleX2Powerup"))
        {
            WebClientSender.instance.SetLedGreen();
            this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            yield return new WaitForSeconds(1f);
            this.gameObject.SetActive(false);
            elementTaken = false;
        }


        if (gameObject.name.Contains("TorusBubble"))
        {
            WebClientSender.instance.SetLedGreen();
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerArrow>().ArrowSetOk();
            GameObject.FindGameObjectWithTag("ArrowCamera").GetComponent<PlayerArrow>().ArrowSetOk();
            yield return new WaitForSeconds(1f);


            if (GameController.instance.gameModeSelected != GameController.GameMode.TUTORIAL  && GameController.instance.gameModeSelected != GameController.GameMode.TUTORIAL_BASIC)
            {
                gameController.AddScore(points);
                DataController.instance.currentMatch.levels.Last().yesRings++;

                double rateYesRingsCalculation = ((double)DataController.instance.currentMatch.levels.Last().yesRings) / DataController.instance.currentMatch.levels.Last().totRings;
                double rateYesRingsRounded = System.Math.Round(rateYesRingsCalculation, 2, System.MidpointRounding.AwayFromZero);
                DataController.instance.currentMatch.levels.Last().rateYesRings = rateYesRingsRounded;
            }
            
            this.gameObject.SetActive(false);
            elementTaken = false;
        }

        if (gameObject.name.Contains("Food"))
        {
            WebClientSender.instance.SetLedGreen();
            yield return new WaitForSeconds(1f);
            gameController.AddScore(points);
            gameController.AddEnergy(energy);
            this.gameObject.SetActive(false);
            elementTaken = false;
        }

        if (gameObject.name.Contains("Hazard"))
        {
            WebClientSender.instance.SetLedRed();
            gameController.AddScore(points);
            gameController.HealthPlayer -= 1;
            yield return new WaitForSeconds(2f);     
            
            hazardAttacking = false;
            animator.SetBool("Attacking", false);
        }

        if (gameObject.name.Contains("Obstacle"))
        {
            WebClientSender.instance.SetLedRed();
            gameController.HealthPlayer -= 1;
            gameController.AddScore(points);
            yield return new WaitForSeconds(1f);
            this.gameObject.SetActive(false);
            elementTaken = false;
        }

    }

    private IEnumerator WrongRingDeactivate()
    {
        yield return new WaitForSeconds(1.15f);
        elementTaken = false;
        this.gameObject.SetActive(false);
    }


    //Timer for the X2 powerup increasing velocity of flowing objects and music.
    //Timer X2 implemented here due to some dependencies with the respective object, and to avoid running two nested coroutines
    private IEnumerator TimerX2()
    {
        //SOUND       
        //SoundManager.instance.PlaySoundtrackX2(0.4f, 1f, true); //In case we want to play a soundtrack starting from the point the current is leaving (requires well composed sountracks)
        SoundManager.instance.PitchTransition(1.5f, 1.5f);
 
        yield return new WaitForSeconds(1f); //Wait for the animation to complete (scales object so it's invisible)


        gameController.levelsParams[gameController.currentLevel].speedElements *= 2;
        gameController.DoubleX2 = true;

        UIGame.instance.x2Banner.SetActive(true);
        GameObject[] elementsFlow = GameObject.FindGameObjectsWithTag("ElementFlow");
        foreach (GameObject element in elementsFlow)
        {
            if (element.activeInHierarchy && (element != this.gameObject))
                element.GetComponent<Rigidbody>().velocity = GameController.instance.spawnGenerators[0].transform.up * (gameController.levelsParams[gameController.currentLevel].speedElements); 
            // element.transform.up * (gameController.levelsParams[gameController.currentLevel].speedElements); //ALTERNATIVE INTERESTING FEATURE: rings fired at their direction at the end of the x2 powerup
        }
        
        GameController.instance.DoubleX2CurrentTimer = gameController.levelsParams[gameController.currentLevel].doubleX2Timer;

        while (GameController.instance.DoubleX2CurrentTimer >= 0)
        {
            yield return new WaitForSeconds(1f);
            GameController.instance.DoubleX2CurrentTimer--;
            

            if (!GameController.instance.spawnWavesIsActive)
                break;
            
        }
        
        //SOUND
        if (GameController.instance.spawnWavesIsActive)
            SoundManager.instance.PitchTransition(1f, 1.5f); //SoundManager.instance.PlaySoundtrackTravelFlow(0.4f, 1f, true);

        gameController.levelsParams[gameController.currentLevel].speedElements /= 2;

        elementsFlow = GameObject.FindGameObjectsWithTag("ElementFlow"); //I have to find all elements again because some of them will be new
        foreach (GameObject element in elementsFlow)
        {
            if (element.activeInHierarchy && (element != this.gameObject))
                element.GetComponent<Rigidbody>().velocity = GameController.instance.spawnGenerators[0].transform.up * (gameController.levelsParams[gameController.currentLevel].speedElements); // element.transform.up * (gameController.levelsParams[gameController.currentLevel].speedElements); //FEATURE INTERESSANTE: anelli sparati nella loro direzione a fine x2
        }

        UIGame.instance.x2Banner.SetActive(false);
        gameController.DoubleX2 = false;

        elementTaken = false;
        this.gameObject.SetActive(false);
    }



    public void PlaySfxElectricityBoundary()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(electricityBoundary, 0.35f);
    }

    public void PlaySfxElementCaught()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(elementCaughtEffect, 0.5f);
    }

    public void PlaySfxElementGeneration()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(elementGeneration, 0.5f);
    }

    public void PlaySfxWrong()
    {
        if (DataController.instance.audioEffectsIsActive)
            audiosourceSfx.PlayOneShot(wrongEffect, 0.37f);
    }



}
