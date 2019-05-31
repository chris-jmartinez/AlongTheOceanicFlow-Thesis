using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;






public class GameController : MonoBehaviour{

    public enum GameMode
    {
        IDLE, INFINITY, ADVENTURE, TUTORIAL, TUTORIAL_BASIC
    }

    public enum PhaseOfLevel
    {
        PRE_TRAVEL_FLOW, TRAVEL_FLOW, TREASURE_HUNT, END
    }

   

    public static GameController instance = null;


    public UiMenuHandler uiMenuScript;
    public GameObject uiGame;
    public UIGame uiGameScript;
    public GameObject areaPlayer;

    private int scorePlayer = 0;
    //public int ScorePlayer { get; set; }
    public TextMesh scoreText;

    public bool DoubleX2 { get; set; }
    public int DoubleX2CurrentTimer { get; set; }



    private int healthPlayer;
    
    public int HealthPlayer
    {
        get
        {
            return healthPlayer;
        }
        set
        {
            if (value <= -1)
                return;

            if (healthPlayer == (value + 1))
            {
                uiGameScript.PlayHealthDecreased();
                UIHealthLightning.Play();
            }
            else
            {
                uiGameScript.PlayParticleHealthIncreased();
            }
                

            healthPlayer = value;
            UIHealthText.text = healthPlayer.ToString();

            UpdateUIHealthAnimator();
            CheckPlayerDead();
           
        }
    }

    private float energyPlayer = 1;

    public float EnergyPlayer
    {
        get
        {
            return energyPlayer;
        }
        set
        {
            energyPlayer = value;
            UIEnergyText.text = (energyPlayer*100f).ToString("F0") + "%";
            UIEnergyBarBase.transform.localScale = new Vector3(value, UIEnergyBarBase.transform.localScale.y, UIEnergyBarBase.transform.localScale.z);
            if (energyPlayer <= 0 && !UIEnergyBar.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("EnergyLowAnimation"))
            {
                uiGameScript.SetActiveLowEnergySound(true);
                UIEnergyBar.GetComponent<Animator>().SetBool("LowEnergy", true);
            }        
            else if (energyPlayer > 0 && !UIEnergyBar.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("EnergyAnimation"))
            {
                uiGameScript.SetActiveLowEnergySound(false);
                UIEnergyBar.GetComponent<Animator>().SetBool("LowEnergy", false);
            }
                
        }
    }
 

    public Animator UIHealthAnimator;
    public ParticleSystem UIHealthLightning;
    public TextMesh UIHealthText;
    public GameObject UIEnergyBar;
    public GameObject UIEnergyBarBase;
    public TextMesh UIEnergyText;
    
    public GameObject countdownStartGame;
    public GameObject labelCountdown;
    private TextMesh labelCountdownTextMesh;
    public int countdownToStartGame;

    public bool startedGame = false;
    public bool countdownStarted = false;
    public bool spawnWavesIsActive = false;
    public bool stillElementsLeft = false;

    public GameObject genericMessage;
    public TextMesh labelGenericMessage;
    public TextMesh textGenericMessage;

    public GameObject genericMessageSmall;
    public TextMesh labelGenericMessageSmall;
    public TextMesh textGenericMessageSmall;

    public GameObject winMessage;
    public GameObject loseMessage;

    public Transform cameraPosition;
    public Transform[] spawnGenerators;
    public GameObject rotatingSpawnGenerator;
    public Transform generatorsPosition;
    public GameObject directionalIndicatorGenerators;
    public GameObject directionalIndicatorGeneratorRotating;
    public GameObject planctonFlow;

    [System.Serializable]
    public class LevelsParameters
    {
        public int healthPlayerInit;
        [Range(1, 4)]
        public int activeGenerators;
        public float startWait;
        public float spawnWait;
        public float waveWait;
        public int elementsPerWave;
        public int numberOfWaves;

        public float speedElements;

        public Vector3 elementsRotationMin;
        public Vector3 elementsRotationMax;
        public float errorToleranceRings;

        public bool energyConsumingIsActive;
        public float energyConsumingTimeInterval;
        public float energyConsumingValue;

        public float spawnWaitPowerups;
        public int doubleX2Timer;

        public bool culturalBitsIsActive;

        public List<GameObject> elements;
        public List<GameObject> powerups;
        public List<GameObject> treasureHuntObjects;
        public List<GameObject> culturalMessages;
    }
    public LevelsParameters[] levelsParams;

   

    

    [Header("Prefabs for Pooling")]
    public GameObject torusBubble;
    public GameObject food;
    public GameObject obstacle1Bottle;
    public GameObject obstacle2Can;
    public GameObject shark1;
    public GameObject shark2;
    public GameObject healthPowerup;
    public GameObject x2Powerup;
    public GameObject lightningEffect;
   

    public GameMode gameModeSelected;
    public PhaseOfLevel currentPhaseOfLevel;
    public int currentLevel;

    private float spawnWaitPowerupsTimer;
    private float maxWaitFoodOctopus = 80f;
    private float foodOctopusCronometer = 0;

    public int CounterCaughtRingsTutorial { set; get; }
    public bool GeneratorIsMoving { set; get; }
    public bool GeneratorMoveRight { set; get; }
    public bool GeneratorMoveLeft { set; get; }
    public bool GeneratorMoveCenterAndDown { set; get; }
    public bool GeneratorMovementComplete { set; get; }

    private System.DateTime timeStartTreasureHunt;
    private System.DateTime timeEndTreasureHunt;

    private bool culturalMessageDisplaying;


    private void UpdateUIHealthAnimator()
    {
        if (healthPlayer > 9)
        {
            if (!UIHealthAnimator.GetCurrentAnimatorStateInfo(0).IsName("HealthGoodAnimation"))
                UIHealthAnimator.SetTrigger("HealthGood");
        }
        else if (healthPlayer <= 9 && healthPlayer >= 5)
        {
            if (!UIHealthAnimator.GetCurrentAnimatorStateInfo(0).IsName("HealthMediumAnimation"))
                UIHealthAnimator.SetTrigger("HealthMedium");
        }

        else
        {
            if (!UIHealthAnimator.GetCurrentAnimatorStateInfo(0).IsName("HealthBadAnimation"))
                UIHealthAnimator.SetTrigger("HealthBad");
        }
    }

    //Awake is always called before any Start functions
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

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);
    }



    // Use this for initialization
    void Start ()
    {
        labelCountdownTextMesh = labelCountdown.GetComponent<TextMesh>();
        scoreText.text = "Score: #";
        UIHealthText.text = ":)";

        DoubleX2 = false;
        GeneratorIsMoving = false;
        GeneratorMoveRight = false;
        GeneratorMoveLeft = false;
        GeneratorMoveCenterAndDown = false;
        GeneratorMovementComplete = false;

        ObjectPoolingManager.Instance.CreatePool(torusBubble, 15, 15);
        ObjectPoolingManager.Instance.CreatePool(food, 15, 15);
        ObjectPoolingManager.Instance.CreatePool(obstacle1Bottle, 5, 5);
        ObjectPoolingManager.Instance.CreatePool(obstacle2Can, 5, 5);
        ObjectPoolingManager.Instance.CreatePool(shark1, 15, 15);
        ObjectPoolingManager.Instance.CreatePool(shark2, 15, 15);
        ObjectPoolingManager.Instance.CreatePool(healthPowerup, 10, 10);
        ObjectPoolingManager.Instance.CreatePool(x2Powerup, 10, 10);
        ObjectPoolingManager.Instance.CreatePool(lightningEffect, 5, 5);

        gameModeSelected = GameMode.IDLE;
    
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (spawnWavesIsActive)
        {
            if (levelsParams[currentLevel].powerups.Count > 0)
                spawnWaitPowerupsTimer -= Time.deltaTime;

            if (levelsParams[currentLevel].energyConsumingIsActive)
                foodOctopusCronometer += Time.deltaTime;
        }
            
    }


    public void StartGame()
    {
        startedGame = true;

        if (!Vuforia.CameraDevice.Instance.IsActive())
            WebClientSender.instance.MoveMouthAndEyesDolphinTogether();

        PrepareLevel();          
    }


    public void PrepareLevel()
    {
        //SOUND
        SoundManager.instance.PlaySoundtrackTravelFlow(0.4f, 1f, false);
        UiMenuHandler.instance.ActivateArrowCameraIfVuforiaDisabled();

        DataController.instance.currentMatch.levels.Last().level = currentLevel;
        double errToleranceRingsRounded = System.Math.Round(levelsParams[currentLevel].errorToleranceRings, 2, System.MidpointRounding.AwayFromZero);
        DataController.instance.currentMatch.levels.Last().errToleranceRings = errToleranceRingsRounded;

        areaPlayer.GetComponent<PlayerArea>().SetActiveIndicatorToAura(false);

        if (gameModeSelected == GameMode.TUTORIAL_BASIC)
        {
            directionalIndicatorGeneratorRotating.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = true;
            directionalIndicatorGeneratorRotating.SetActive(true);
        }
        else
        {
            directionalIndicatorGenerators.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = true;
            directionalIndicatorGenerators.SetActive(true);
        }
        
        

        //Clean Floor objects and decorations (useful especially after treasure hunt)
        DecorationsSpawnerManager.Instance.CleanFloorObjects();
        DecorationsSpawnerManager.Instance.CleanWallsObjects();

        PrepareGeneratorsPositionAndActivate(true);
        StartCoroutine(CountdownThenSpawn());
    }

    

    public void PrepareGeneratorsPositionAndActivate(bool value)
    {
        generatorsPosition.position = new Vector3(generatorsPosition.position.x, cameraPosition.position.y, generatorsPosition.position.z);

        if (gameModeSelected != GameMode.TUTORIAL_BASIC && gameModeSelected != GameMode.TUTORIAL)
            for(int i=0; i<levelsParams[currentLevel].activeGenerators; i++)
            {
                if (value == true)
                {
                    spawnGenerators[i].GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    spawnGenerators[i].GetChild(0).gameObject.SetActive(false);
                }
            
            }
        else if (gameModeSelected == GameMode.TUTORIAL_BASIC)
        {
            if (value == true)
                rotatingSpawnGenerator.transform.GetChild(0).gameObject.SetActive(true);
            else
                rotatingSpawnGenerator.transform.GetChild(0).gameObject.SetActive(false);
        }
        else if (gameModeSelected == GameMode.TUTORIAL)
        {
            if (value == true)
                spawnGenerators[0].GetChild(0).gameObject.SetActive(true);
            else
                spawnGenerators[0].GetChild(0).gameObject.SetActive(false);
        }


        if (value == true)
        {
            planctonFlow.SetActive(true);
        }
        else
        {
            planctonFlow.SetActive(false);
        }
    }




    private IEnumerator CountdownThenSpawn()
    {
        currentPhaseOfLevel = PhaseOfLevel.TRAVEL_FLOW;
        countdownStarted = true;
        int timer = countdownToStartGame;

        if (countdownStartGame.activeInHierarchy == false)
        {
            uiMenuScript.PutInFrontOfCamera(countdownStartGame);
            countdownStartGame.SetActive(true);
        }

        while (timer >= 0)
        {
            if (timer == 5)
                WebClientSender.instance.ResetLedDolphin(); //Some of these commands are redundant due to the fact sometimes they are not executed because of connection unstable

            yield return new WaitForSeconds(1f);
            timer--;
            labelCountdownTextMesh.text = timer.ToString();
        }

        WebClientSender.instance.OpenEyesDolphin();
        WebClientSender.instance.ResetLedDolphin();
        

        countdownStartGame.SetActive(false);
        labelCountdownTextMesh.text = countdownToStartGame.ToString();


        if (gameModeSelected == GameMode.TUTORIAL)
            StartCoroutine(SpawnWavesTutorial());
        else if (gameModeSelected == GameMode.TUTORIAL_BASIC)
            StartCoroutine(SpawnWavesTutorialBasic());
        else
            StartCoroutine(SpawnWavesRandom()); //spawnWavesRandomCoroutine = StartCoroutine(SpawnWavesRandom()); If we want to later stop coroutine (not recommended with this complex one)

        countdownStarted = false;
    }



    //Function controlling the Infinity mode and the first phase of each level of the Adventure mode
    //It generates flowing elements randomly and divided in waves. Cultural messages are displayed in between.
    private IEnumerator SpawnWavesRandom()
    {        
        spawnWavesIsActive = true;
        stillElementsLeft = false;

        if (levelsParams[currentLevel].energyConsumingIsActive)
            StartCoroutine(EnergyConsuming());

        spawnWaitPowerupsTimer = levelsParams[currentLevel].spawnWaitPowerups;

        yield return new WaitForSeconds(levelsParams[currentLevel].startWait);

        for (int i = 0; i <= levelsParams[currentLevel].numberOfWaves - 1; i++)
        {
            for (int j=0; j <= levelsParams[currentLevel].elementsPerWave - 1; j++)
            {
                
                if (!startedGame)
                    yield break; //Kill the coroutine if the game ended (most cases, when player loses)

                //Random.InitState(System.DateTime.Now.Millisecond); //This should randomize better, but if from one iteration to another the objects are the same, probably is setting the same seed therefore producing same results

                Transform extractedSpawnGen = spawnGenerators[Random.Range(0, levelsParams[currentLevel].activeGenerators)]; //Random.range excludes the right number from the range
                string newElementName;

                if (levelsParams[currentLevel].energyConsumingIsActive && foodOctopusCronometer >= maxWaitFoodOctopus)
                { //We want to spawn some food manually if it's a long time it hasn't appeared (for good gameplay necessities)
                    foodOctopusCronometer = 0;
                    newElementName = food.name;
                }
                else if (levelsParams[currentLevel].powerups.Count > 0 && spawnWaitPowerupsTimer <= 0)
                {
                    //Spawns a powerup at precise times. After the timer has ended.
                    spawnWaitPowerupsTimer = levelsParams[currentLevel].spawnWaitPowerups;
                    newElementName = levelsParams[currentLevel].powerups[Random.Range(0, levelsParams[currentLevel].powerups.Count)].name;
                }
                else 
                { //Spawns a random element
                    newElementName = levelsParams[currentLevel].elements[Random.Range(0, levelsParams[currentLevel].elements.Count)].name;
                    
                    if (newElementName.Contains("TorusBubble"))
                        DataController.instance.currentMatch.levels.Last().totRings++;
                    else if (newElementName.Contains("Obstacle"))
                    {
                        DataController.instance.currentMatch.levels.Last().totObstacles++;
                        UpdateAvoidedObstacles();
                    }
                    else if (newElementName.Contains("Octopus"))
                        foodOctopusCronometer = maxWaitFoodOctopus;

                }

                
                GameObject newElement = ObjectPoolingManager.Instance.GetObject(newElementName);
                if (newElement == null)
                {
                    Debug.Log("New Element is null, error!");
                }

                newElement.transform.position = extractedSpawnGen.transform.position;
                newElement.transform.rotation = extractedSpawnGen.transform.rotation;
                newElement.GetComponent<Rigidbody>().velocity = newElement.transform.up * levelsParams[currentLevel].speedElements;

                if (newElementName.Contains("TorusBubble"))
                    newElement.transform.localEulerAngles += new Vector3(Random.Range(levelsParams[currentLevel].elementsRotationMin.x, levelsParams[currentLevel].elementsRotationMax.x),
                                                                         Random.Range(levelsParams[currentLevel].elementsRotationMin.y, levelsParams[currentLevel].elementsRotationMax.y),
                                                                         Random.Range(levelsParams[currentLevel].elementsRotationMin.z, levelsParams[currentLevel].elementsRotationMax.z));

                //this line has been substituted by the object pooling
                //Instantiate(newElement, extractedSpawnGen.transform.position, extractedSpawnGen.transform.rotation);  

                yield return new WaitForSeconds(levelsParams[currentLevel].spawnWait);
            }
           
            yield return new WaitForSeconds(levelsParams[currentLevel].waveWait);

            WebClientSender.instance.OpenEyesDolphin(); //Redundant but useful to open eyes in case the dolphin has closed them involuntarily (sometimes it happens)

            //The wave wait time should allow all the elements to travel through the playfield. (Approx 10 secs) Then the culture message should be displayed if present
            if (levelsParams[currentLevel].culturalBitsIsActive && i <= levelsParams[currentLevel].culturalMessages.Count() - 1  && startedGame 
                && !(i == levelsParams[currentLevel].numberOfWaves -1))
            {
                culturalMessageDisplaying = true;
                uiMenuScript.PutInFrontOfCamera(levelsParams[currentLevel].culturalMessages[i]);
                levelsParams[currentLevel].culturalMessages[i].SetActive(true);
                yield return new WaitForSeconds(10f);
                levelsParams[currentLevel].culturalMessages[i].SetActive(false);
                yield return new WaitForSeconds(1f);
                culturalMessageDisplaying = false;
            }
        }

        //END OF THE SPAWNING

        GameObject[] elementsLeft = GameObject.FindGameObjectsWithTag("ElementFlow");
        if (elementsLeft.Length != 0)
            yield return new WaitForSeconds(10f); //Wait until the last elements flow
        else
            yield return new WaitForSeconds(3f);

 
        spawnWavesIsActive = false;


        AuraAndGeneratorsDeactivation();

        //In case the arrow of the camera is active (due to vuforia inactive) deactivate arrow because the game finished / or is time for treasure mode and we don't need any arrow
        UiMenuHandler.instance.DeactivateArrowCameraIfVuforiaDisabled();

        if (!startedGame)
        {
            yield break;
        }

        if (gameModeSelected == GameMode.INFINITY)
        {
            StartCoroutine(EndGameWin());
            
        }
        else if (gameModeSelected == GameMode.ADVENTURE)
        {
            uiMenuScript.PutInFrontOfCamera(genericMessage);
            genericMessage.SetActive(true);
            labelGenericMessage.text = "CACCIA AL TESORO";
            textGenericMessage.text = "Hai attraversato la corrente oceanica,\n" +
                "ma ora ti sei smarrito in un fondale!\n" +
                "Guardati attorno e cerca qualcosa che\n" +
                "ti aiuti a proseguire la tua avventura!";

            SoundManager.instance.PlaySoundtrackTreasureHunt(0.4f, 5f, false);
            yield return new WaitForSeconds(15f);
            genericMessage.SetActive(false);
            TreasureHunt();
        }


    }



    public void AuraAndGeneratorsDeactivation()
    {
        directionalIndicatorGenerators.SetActive(false);
        directionalIndicatorGenerators.GetComponent<DirectionIndicator>().DirectionIndicatorObject.GetComponent<MeshRenderer>().enabled = false;
        PrepareGeneratorsPositionAndActivate(false);

        areaPlayer.GetComponent<PlayerArea>().AuraActive = false;
        areaPlayer.GetComponent<PlayerArea>().auraEntered = false;
        areaPlayer.GetComponent<PlayerArea>().particlesArea.SetActive(false);
        areaPlayer.GetComponent<PlayerArea>().auraSoundSource.enabled = false;
        areaPlayer.GetComponent<PlayerArea>().goIntoAuraMessage.SetActive(false);
    }




    public void IncrementAvoidedObstacles()
    {
        DataController.instance.currentMatch.levels.Last().avoidedObstacles++;
        UpdateAvoidedObstacles();
    }

    public void UpdateAvoidedObstacles()
    {
        double rateAvoidedObstaclesCalculation = ((double)DataController.instance.currentMatch.levels.Last().avoidedObstacles) / DataController.instance.currentMatch.levels.Last().totObstacles;
        double rateAvoidedObstaclesRounded = System.Math.Round(rateAvoidedObstaclesCalculation, 2, System.MidpointRounding.AwayFromZero);
        DataController.instance.currentMatch.levels.Last().rateAvoidedObstacles = rateAvoidedObstaclesRounded;
    }



    private IEnumerator SpawnWavesTutorial()
    { 
        spawnWavesIsActive = true;

        yield return new WaitForSeconds(levelsParams[currentLevel].startWait);

        labelGenericMessage.text = "TUTORIAL";
        textGenericMessage.text = "Rimani nell'aura o\ndopo 10 secondi perderai vita!\nVerranno verso di te\ndegli anelli,\nimpara a centrarli\ncon la giusta angolazione!";
        uiMenuScript.PutInFrontOfCamera(genericMessage);
        genericMessage.SetActive(true);
        yield return new WaitForSeconds(10f);
        genericMessage.SetActive(false);

        CounterCaughtRingsTutorial = 0;

        while (CounterCaughtRingsTutorial < 9)
        {

            if (!startedGame)
                yield break; //Kill the coroutine if the game ended (most cases, when player loses)

            Transform extractedSpawnGen = spawnGenerators[0]; //Random.range excludes the right number from the range

            string newElementName = torusBubble.name;
           
            GameObject newElement = ObjectPoolingManager.Instance.GetObject(newElementName);
            
            newElement.transform.position = extractedSpawnGen.transform.position;
            newElement.transform.rotation = extractedSpawnGen.transform.rotation;
            newElement.GetComponent<Rigidbody>().velocity = newElement.transform.up * levelsParams[currentLevel].speedElements;

            if (CounterCaughtRingsTutorial < 3)
            {
                newElement.transform.localEulerAngles += new Vector3(0f, 0f, 0f);
            }
            else if (CounterCaughtRingsTutorial >= 3 && CounterCaughtRingsTutorial < 6)
            {
                newElement.transform.localEulerAngles += new Vector3(0f, +20f, 0f);
            }
            else if (CounterCaughtRingsTutorial >= 6 && CounterCaughtRingsTutorial < 9)
            {
                newElement.transform.localEulerAngles += new Vector3(0f, -20f, 0f);
            }

            yield return new WaitForSeconds(levelsParams[currentLevel].spawnWait);

        }

        CounterCaughtRingsTutorial = 0;


        yield return new WaitForSeconds(8f); //Wait until the last elements flow

        spawnWavesIsActive = false;
        AuraAndGeneratorsDeactivation();
        areaPlayer.GetComponent<PlayerArea>().goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Prendi il delfino\ne vai dentro\nl'aura!";

        UiMenuHandler.instance.DeactivateArrowCameraIfVuforiaDisabled();

        if (!startedGame)
        {
            yield break;
        }

        startedGame = false;
        gameModeSelected = GameMode.IDLE;
        currentPhaseOfLevel = PhaseOfLevel.END;

        WebClientSender.instance.MoveMouthAndEyesDolphinTogether();
        labelGenericMessage.text = "FINE TUTORIAL";
        textGenericMessage.text = "Tutorial completato!\nOra sei pronto per\ngiocare una vera partita!";
        uiMenuScript.PutInFrontOfCamera(genericMessage);
        genericMessage.SetActive(true);
        yield return new WaitForSeconds(5f);
        textGenericMessage.text = "Ricorda: mentre giochi potresti\nincontrare anche:\n*Piccoli polpi (ricaricano l'energia!)\n*Ostacoli da evitare!";
        yield return new WaitForSeconds(8f);
        genericMessage.SetActive(false);

        uiMenuScript.ShowMainMenu();
    }





    private IEnumerator SpawnWavesTutorialBasic()
    {
        spawnWavesIsActive = true;

        yield return new WaitForSeconds(levelsParams[currentLevel].startWait);

        labelGenericMessage.text = "TUTORIAL BASIC";
        textGenericMessage.text = "Girati verso il generatore\ne centra gli anelli che\nverranno verso di te!";
        uiMenuScript.PutInFrontOfCamera(genericMessage);
        genericMessage.SetActive(true);
        yield return new WaitForSeconds(5f);
        genericMessage.SetActive(false);

        CounterCaughtRingsTutorial = 0;
        GeneratorOrbitAroundCenter generatorOrbitAroundCenter = rotatingSpawnGenerator.GetComponent<GeneratorOrbitAroundCenter>();
        string newElementName = torusBubble.name;
        Coroutine movingCoroutine = null;
        GeneratorIsMoving = false;

        while (CounterCaughtRingsTutorial<=9)
        {

            if (!startedGame)
                yield break; //Kill the coroutine if the game ended (most cases, when player loses)



            GameObject newElement = ObjectPoolingManager.Instance.GetObject(newElementName);

            newElement.transform.position = rotatingSpawnGenerator.transform.position;
            newElement.transform.rotation = rotatingSpawnGenerator.transform.rotation;
            newElement.GetComponent<Rigidbody>().velocity = newElement.transform.up * levelsParams[currentLevel].speedElements;


            //yield return new WaitForSeconds(levelsParams[currentLevel].spawnWait);
            yield return new WaitForSeconds(7.5f);



            if (GeneratorMoveRight && !GeneratorIsMoving)
            {
                GeneratorIsMoving = true;
                yield return new WaitForSeconds(5f);
                movingCoroutine = StartCoroutine(generatorOrbitAroundCenter.OrbitRightUntilPosDegrees(45));
                yield return movingCoroutine; //yield return new WaitForSeconds(5f); //This could be removed to make the game more challenging, rings would be fired when moving
                GeneratorIsMoving = false;
                GeneratorMoveRight = false;
            }
            else if (GeneratorMoveLeft && !GeneratorIsMoving)
            {
                GeneratorIsMoving = true;
                yield return new WaitForSeconds(5f);
                movingCoroutine = StartCoroutine(generatorOrbitAroundCenter.OrbitLeftUntilNegDegrees(-45));
                yield return movingCoroutine; //yield return new WaitForSeconds(10f);
                GeneratorIsMoving = false;
                GeneratorMoveLeft = false;
            }
            else if (GeneratorMoveCenterAndDown && !GeneratorIsMoving)
            {
                GeneratorIsMoving = true;
                yield return new WaitForSeconds(5f);
                movingCoroutine = StartCoroutine(generatorOrbitAroundCenter.OrbitUntilCenter());
                yield return movingCoroutine;
                movingCoroutine = StartCoroutine(generatorOrbitAroundCenter.GoSitDown());
                yield return movingCoroutine;
                GeneratorIsMoving = false;
                GeneratorMoveCenterAndDown = false;
            }
            else if (GeneratorMovementComplete && !GeneratorIsMoving)
            {
                //yield return new WaitForSeconds(5f);
                //StartCoroutine(generatorOrbitAroundCenter.ReturnUpToParentCenter());
                //yield return new WaitForSeconds(5f);
                GeneratorMovementComplete = false;
                break;
            }


            

        }

        CounterCaughtRingsTutorial = 0;
        GeneratorIsMoving = false;
        GeneratorMoveRight = false;
        GeneratorMoveLeft = false;
        GeneratorMoveCenterAndDown = false;
        GeneratorMovementComplete = false;

        yield return new WaitForSeconds(10f); //Wait until the last elements flow, if there are

        generatorOrbitAroundCenter.ResetPositionGeneratorRotating();

        spawnWavesIsActive = false;

        AuraAndGeneratorsDeactivation();

        areaPlayer.GetComponent<PlayerArea>().goIntoAuraMessage.GetComponentInChildren<TextMesh>().text = "Prendi il delfino\ne vai dentro\nl'aura!";

        UiMenuHandler.instance.DeactivateArrowCameraIfVuforiaDisabled();

        if (!startedGame)
        {
            yield break;
        }

        startedGame = false;
        gameModeSelected = GameMode.IDLE;
        currentPhaseOfLevel = PhaseOfLevel.END;

        WebClientSender.instance.MoveMouthAndEyesDolphinTogether();
        labelGenericMessage.text = "FINE TUTORIAL";
        textGenericMessage.text = "Tutorial completato!";
        uiMenuScript.PutInFrontOfCamera(genericMessage);
        genericMessage.SetActive(true);
        yield return new WaitForSeconds(3f);
        genericMessage.SetActive(false);

        uiMenuScript.ShowMainMenu();
    }


    private void TreasureHunt()
    {
        timeStartTreasureHunt = System.DateTime.Now;
        bool randomically = true;
        bool nonRandomically = false;

        areaPlayer.GetComponent<PlayerArea>().SetActiveParticlesAura(false);
        currentPhaseOfLevel = PhaseOfLevel.TREASURE_HUNT;

        if (currentLevel == 3)
        {     
            DecorationsSpawnerManager.Instance.SpawnWallsDecorationObjects(DecorationsSpawnerManager.Instance.paintingsWallsDecorationObjectPrefabs, randomically);
        }
        else
        {
            DecorationsSpawnerManager.Instance.SpawnWallsDecorationObjects(DecorationsSpawnerManager.Instance.gifWallsDecorationObjectPrefabs, randomically);
        }

        DecorationsSpawnerManager.Instance.CleanFloorObjects();
        
        StartCoroutine(DecorationsSpawnerManager.Instance.SpawnFloorDecorationObjects(levelsParams[currentLevel].treasureHuntObjects, nonRandomically));

    }

    public IEnumerator TreasureHuntFinished()
    {
        timeEndTreasureHunt = System.DateTime.Now;
        double minutesTimeTreasure = System.Math.Round((timeEndTreasureHunt - timeStartTreasureHunt).TotalMinutes, 2);
        DataController.instance.currentMatch.levels.Last().timeTreasure = minutesTimeTreasure;

        WebClientSender.instance.ResetLedDolphin();
        yield return new WaitForSeconds(1f);

        switch (currentLevel)
        {
            case 1:
                labelGenericMessage.text = "LIVELLO " + currentLevel + " COMPLETATO!";
                textGenericMessage.text = "Hai trovato una mappa di questa\nparte dell'Oceano! Ora sai come\nproseguire nel tuo viaggio!\n\nRitorna nell'aura\n" +
                    "per iniziare il prossimo livello!";
                uiMenuScript.PutInFrontOfCamera(genericMessage);
                genericMessage.SetActive(true);
                yield return new WaitForSeconds(10f);
                genericMessage.SetActive(false);
                NextLevel();
                break;
            case 2:
                labelGenericMessage.text = "LIVELLO " + currentLevel + " COMPLETATO!";
                textGenericMessage.text = "Hai trovato una mappa di questa\n parte dell'Oceano! Sei vicino a casa!\nOra sai come proseguire\nnel tuo viaggio!\n\nRitorna nell'aura\n" +
                    "per iniziare il prossimo livello!";
                uiMenuScript.PutInFrontOfCamera(genericMessage);
                genericMessage.SetActive(true);
                yield return new WaitForSeconds(10f);
                genericMessage.SetActive(false);
                NextLevel();
                break;
            case 3:
                StartCoroutine(EndGameWin());
                break;
        }
        
        
        
    }

    public void NextLevel()
    {
        //Calculations at the end of the level
        

        DataController.instance.printCurrentMatchData();
        currentLevel++;
        DataController.instance.currentMatch.levels.Add(new LevelData());

        //In the pretravel phase we activate the aura
        
        currentPhaseOfLevel = PhaseOfLevel.PRE_TRAVEL_FLOW;
        areaPlayer.GetComponent<PlayerArea>().AuraActive = true;
        areaPlayer.GetComponent<PlayerArea>().auraEntered = false;
        areaPlayer.GetComponent<PlayerArea>().AuraFound = false;
        areaPlayer.GetComponent<PlayerArea>().auraSoundSource.enabled = true;


        areaPlayer.GetComponent<PlayerArea>().SetActiveGoIntoAuraMessage(true);
        //areaPlayer.GetComponent<PlayerArea>().SetActiveIndicatorToAura(true);
        areaPlayer.GetComponent<PlayerArea>().SetActiveParticlesAura(true);
    }

    public IEnumerator EndGameWin()
    {
        startedGame = false;

        SoundManager.instance.PlaySoundtrackWinSurvived(0.4f, 2f, false);

        gameModeSelected = GameMode.IDLE;

        WebClientSender.instance.MoveMouthAndEyesDolphinTogether();

        winMessage.SetActive(true);
        yield return new WaitForSeconds(10f);
        winMessage.SetActive(false);

        WebClientSender.instance.ResetLedDolphin();

        DataController.instance.printCurrentMatchData();
        uiMenuScript.ShowInsertPlayerNameMenu();


    }

    public IEnumerator EndGameLose()
    {
        GameObject[] elementsLeft = GameObject.FindGameObjectsWithTag("ElementFlow");
        if (elementsLeft.Length != 0)
        {
            foreach (GameObject elementToDeactivate in elementsLeft)
                elementToDeactivate.SetActive(false);
        }

        startedGame = false;
        uiGameScript.SetActiveLowEnergySound(false);
        SoundManager.instance.PlaySoundtrackLoseDeath(0.4f, 2f, false);

        spawnWavesIsActive = false;

        currentPhaseOfLevel = PhaseOfLevel.END;
        gameModeSelected = GameMode.IDLE;

        WebClientSender.instance.MoveMouthAndEyesDolphinTogether();


        AuraAndGeneratorsDeactivation();

        
        uiMenuScript.PutInFrontOfCamera(loseMessage);
        loseMessage.SetActive(true);
        yield return new WaitForSeconds(10f);
        loseMessage.SetActive(false);
        WebClientSender.instance.ResetLedDolphin();

        DataController.instance.printCurrentMatchData();
        uiMenuScript.ShowInsertPlayerNameMenu();
        
    }

    


    public void CheckPlayerDead()
    {
        if (healthPlayer <= 0)
            StartCoroutine(EndGameLose());
    }


    public void AddScore(int scoreToAdd)
    {
        this.scorePlayer += scoreToAdd;

        if (DoubleX2)
            scorePlayer += scoreToAdd;
        
        
        scoreText.text = "Score: " + scorePlayer.ToString();

        DataController.instance.currentMatch.scorePlayer = scorePlayer;

        if (currentPhaseOfLevel == PhaseOfLevel.TREASURE_HUNT)
        {
            uiMenuScript.PutInFrontOfCamera(genericMessageSmall);
            genericMessageSmall.SetActive(true);
            labelGenericMessageSmall.text = "SCORE";
            textGenericMessageSmall.text = "+" + scoreToAdd + " punti!";
            StartCoroutine(manualTimerGenericMessage());
        }
    }

    private IEnumerator manualTimerGenericMessage()
    {
        yield return new WaitForSeconds(3f);
        genericMessageSmall.SetActive(false);
    }

    public void InitScore()
    {
        scorePlayer = 0;
        scoreText.text = "Score: " + scorePlayer.ToString();
    }


    public void AddEnergy(float energyToAdd)
    {
        if (energyPlayer + energyToAdd >= 1)
            EnergyPlayer = 1;
        else if (energyPlayer + energyToAdd <= 0)
            EnergyPlayer = 0;
        else
            EnergyPlayer = (float)(System.Math.Round(energyPlayer + energyToAdd, 2));

        if (energyToAdd > 0)
        {
            uiGameScript.PlayParticleEnergyIncreased();
        }
    }

    private IEnumerator EnergyConsuming()
    {
        while (spawnWavesIsActive)
        {
            if (culturalMessageDisplaying)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(levelsParams[currentLevel].energyConsumingTimeInterval);

            AddEnergy(-levelsParams[currentLevel].energyConsumingValue);

            if (EnergyPlayer <= 0)
            {
                HealthPlayer--;
            }
            
        }
    }

    
}




