using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;
using System.Linq;
using HoloToolkit.Examples.InteractiveElements;
using System;
using HoloToolkit.Unity;

public class UiMenuHandler : MonoBehaviour, IInputClickHandler {

    public static UiMenuHandler instance = null;

    public GameObject playerTag;
    public DolphinTrackableEventHandler playerTagScriptEvent;
    public GameObject uiMenu;
    public GameObject mainMenu;
    public GameObject moveMenu;
    public GameObject exploreMenu;
    public GameObject settingsInfinityMenu;
    public GameObject statsMenu;
    public GameObject pauseMenu;
    public GameObject scanningMenu;
    public GameObject scanningMenuText;
    public GameObject rotatingOrbs;
    public GameObject scanningMenuOkButton;
    public GameObject goIntoAuraMessage;
    public GameObject directionalIndicatorAura;

    public GameObject insertPlayerNameMenu;
    public TextMesh labelInsertedPlayerName;
    public GameObject insertPlayerNameOkButton;

    public GameObject areaPlayer;

    private string myTempPlayerName;
    public int PlayerNameMaxDigits;

    public TextMesh highscoresDataScoreTextMesh;
    public TextMesh highscoresDataNamesTextMesh;
    public TextMesh statsDataTextMesh;


    public int numMatchesToPrint = 4;

    public Material cursorMaterial;

    public Material wallsMaterial;
    public Material floorsMaterial;

    private bool movePlayfieldWithGazeEnabled;
    private string nameOfLastPlayer = "";

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
    void Start () {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        cursorMaterial.color = new Color(1f, 1f, 1f, 1.0f);
        myTempPlayerName = "";
    }
	
	

    public void ScanningComplete()
    {
        rotatingOrbs.SetActive(false);
        scanningMenuText.GetComponent<TextMesh>().text = "\n\n\nSCANNING COMPLETED!";
        scanningMenuOkButton.SetActive(true);
    }

    public void ScanningCompleteOkPressed()
    {

        SoundManager.instance.LowerCurrentMusic(0.1f, 1f);

        DecorationsSpawnerManager.Instance.DecorationAtStart();
        
        Invoke("ShowMainMenu", 8f);
        
    }


    


    public void ReadyToPlay(GameObject buttonGameSelected)
    {
        SoundManager.instance.LowerCurrentMusic(0.1f, 1f);
        DataController.instance.CreateNewDataMatch();
        WebClientSender.instance.OpenEyesDolphin();
        WebClientSender.instance.ResetLedDolphin();

        areaPlayer.GetComponent<PlayerArea>().SetActiveGoIntoAuraMessage(true);
        areaPlayer.GetComponent<PlayerArea>().SetActiveIndicatorToAura(true);
        areaPlayer.GetComponent<PlayerArea>().SetActiveParticlesAura(true);
        areaPlayer.GetComponent<PlayerArea>().auraSoundSource.enabled = true;
        areaPlayer.GetComponent<PlayerArea>().AuraFound = false;

        playerTagScriptEvent.firstTimeDetected = false;

        switch (buttonGameSelected.name)
        {
            case "InfinityButton":
                GameController.instance.gameModeSelected = GameController.GameMode.INFINITY;
                GameController.instance.currentLevel = 0;
                if (DataController.instance.numMatchesCounter+1 >= 3)
                    Utilities.Shuffle(GameController.instance.levelsParams[0].culturalMessages);
                break;
            case "AdventureButton":
                GameController.instance.gameModeSelected = GameController.GameMode.ADVENTURE;
                GameController.instance.currentLevel = 1;
                break;
            case "TutorialButton":
                GameController.instance.gameModeSelected = GameController.GameMode.TUTORIAL;
                GameController.instance.currentLevel = 0;
                break;
            case "TutorialBasicButton":
                GameController.instance.gameModeSelected = GameController.GameMode.TUTORIAL_BASIC;
                GameController.instance.currentLevel = 0;
                break;
        }
        DataController.instance.currentMatch.gameMode = GameController.instance.gameModeSelected;


        GameController.instance.currentPhaseOfLevel = GameController.PhaseOfLevel.PRE_TRAVEL_FLOW;

        areaPlayer.GetComponent<PlayerArea>().AuraActive = true;
        areaPlayer.GetComponent<PlayerArea>().auraEntered = false;

        GameController.instance.HealthPlayer = GameController.instance.levelsParams[GameController.instance.currentLevel].healthPlayerInit;
        GameController.instance.EnergyPlayer = 1f;
        GameController.instance.InitScore();

        if (GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL  ||  GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL_BASIC)
        {
            GameController.instance.UIHealthText.text = "∞";
            GameController.instance.scoreText.text = "TUTORIAL";
        }
    }

    public void ShowGoIntoAura()
    {
        goIntoAuraMessage.SetActive(true);
        directionalIndicatorAura.SetActive(true);
    }

    public void ShowMainMenu()
    {
        SoundManager.instance.PlaySoundtrackMainMenu(0.4f, 1f, false);
        mainMenu.SetActive(true);
    }

    public void HideMainMenu()
    {
        mainMenu.SetActive(false);
    }
    

    public void ActivateMoveWithGaze(bool activate)
    {
        if (activate)
            movePlayfieldWithGazeEnabled = true;
        else
            movePlayfieldWithGazeEnabled = false;
    }

    //When the Move menu is entered, we have two ways to move the Playfield:
    //1) we move air-tapping a point in the floor.
    //2) we move saying "Move here".
    //Both ways call the function "MoveHereThePlayfield", and the point of the floor where the cursor is pointing, it's used as a point to place the playfield
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (eventData == null || eventData.selectedObject == null || eventData.selectedObject.name == "MoveButton" || eventData.selectedObject.name == "OkButtonMoveMenu" || eventData.selectedObject.name == "BackgroundMoveMenu")
            return;

        if (movePlayfieldWithGazeEnabled)
            MoveHereThePlayfield();
        //throw new NotImplementedException();
    }

    public void MoveHereThePlayfield()
    {
        if (!movePlayfieldWithGazeEnabled)
            return;

        RaycastHit hitInfo;
        bool hit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 20f, LayerMask.GetMask("Spatial Mapping"));

        if (!hit)
            return;

        if (hitInfo.collider.gameObject.GetComponent<SurfacePlane>().PlaneType != PlaneTypes.Floor)
            return;

        GameObject playfield = GameObject.FindGameObjectWithTag("Playfield");

        Vector3 hitPosition = hitInfo.point;
        playfield.transform.position = hitPosition;

        Vector3 relativeDistance = hitPosition - Camera.main.transform.position;

        Quaternion playfieldNewRotation = Quaternion.LookRotation(relativeDistance);
        playfieldNewRotation.x = 0f;
        playfieldNewRotation.z = 0f;

        playfield.transform.position = hitPosition;
        playfield.transform.rotation = playfieldNewRotation;
    }

    public void BringMenuHere()
    {
        Debug.Log("Bring menu here said");
        if (settingsInfinityMenu.activeInHierarchy)
            PutInFrontOfCamera(settingsInfinityMenu);
        else if (statsMenu.activeInHierarchy)
            PutInFrontOfCamera(statsMenu);
        else if (exploreMenu.activeInHierarchy)
            PutInFrontOfCamera(exploreMenu);
        else if (moveMenu.activeInHierarchy)
            PutInFrontOfCamera(moveMenu);

    }

    
    public void HelpMe()
    {
        if (!(GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TREASURE_HUNT))
            return;

        GameObject casualFloor = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor)[0];

        GameObject woodenChest = GameObject.FindGameObjectWithTag("TreasureChest");
        if (woodenChest != null)
        {
            woodenChest.transform.position = Camera.main.transform.TransformPoint(Vector3.forward * 2.5f);
            woodenChest.transform.position = new Vector3(woodenChest.transform.position.x, casualFloor.transform.position.y + 0.05f, woodenChest.transform.position.z);
        }
            

        GameObject key1 = GameObject.FindGameObjectWithTag("Key1");
        if (key1 != null)
        {
            key1.transform.position = Camera.main.transform.TransformPoint(Vector3.forward * 1);
            key1.transform.position = new Vector3(key1.transform.position.x, casualFloor.transform.position.y + 0.05f, key1.transform.position.z);
        }
            

        GameObject key2 = GameObject.FindGameObjectWithTag("Key2");
        if (key2 != null)
        {
            key2.transform.position = Camera.main.transform.TransformPoint(Vector3.forward * 1.5f);
            key2.transform.position = new Vector3(key2.transform.position.x, casualFloor.transform.position.y + 0.05f, key2.transform.position.z);
        }

        GameObject password = GameObject.FindGameObjectWithTag("Password");
        if (password != null)
        {
            password.transform.position = Camera.main.transform.TransformPoint(Vector3.forward * 1.5f); 
            password.transform.LookAt(Camera.main.transform.position);
        }

    }

    public void PauseMenuFreezeGame()
    {
        if (GameController.instance.startedGame)
        {
            Time.timeScale = 0f;
            if (!pauseMenu.activeInHierarchy)
                pauseMenu.SetActive(true);
        }

    }

    public void PauseMenuContinueGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void PauseMenuReturnMainMenu()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);

        GameObject[] activeFlowElements = GameObject.FindGameObjectsWithTag("ElementFlow");
        foreach (GameObject activeFlowElement in activeFlowElements)
            activeFlowElement.SetActive(false);


        WebClientSender.instance.MoveMouthAndEyesDolphinTogether();
        WebClientSender.instance.ResetLedDolphin();

        GameController.instance.spawnWavesIsActive = false;
        GameController.instance.AuraAndGeneratorsDeactivation();
        DeactivateArrowCameraIfVuforiaDisabled();

        GameController.instance.startedGame = false;
        GameController.instance.currentPhaseOfLevel = GameController.PhaseOfLevel.END;
        GameController.instance.gameModeSelected = GameController.GameMode.IDLE;


        if (goIntoAuraMessage.activeInHierarchy)
            goIntoAuraMessage.SetActive(false);

        CleanNameAndStatisticsForNextGame();
    }


    public void PutInFrontOfCamera(GameObject objectToPutInFrontOfCamera)
    {
        Vector3 newWorldPositionInFrontOfCamera = Camera.main.transform.TransformPoint(Vector3.forward * 2); //We extract the new world position of the camera based on the local position in front of the camera
        objectToPutInFrontOfCamera.transform.position = newWorldPositionInFrontOfCamera;
    }

    public void StreamingOn()
    {
        if (Vuforia.CameraDevice.Instance.IsActive())
        {
            Vuforia.CameraDevice.Instance.Stop();

            Vuforia.CameraDevice.Instance.Deinit();

            cursorMaterial.color = new Color(0.7f, 0.2f, 0.3f, 1.0f);
            //cursorMaterial.color = new Color32(173, 173, 173, 200);

        }

        if (GameController.instance.gameModeSelected == GameController.GameMode.INFINITY ||
            GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL ||
            GameController.instance.gameModeSelected == GameController.GameMode.TUTORIAL_BASIC ||
            GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TRAVEL_FLOW)
        {
            GameObject arrowCamera = GameObject.FindGameObjectWithTag("ArrowCamera");
            arrowCamera.GetComponent<MeshRenderer>().enabled = true;
        }
        
    }

    public void ActivateArrowCameraIfVuforiaDisabled()
    {
        if (!Vuforia.CameraDevice.Instance.IsActive())
        {
            GameObject arrowCamera = GameObject.FindGameObjectWithTag("ArrowCamera");
            arrowCamera.GetComponent<MeshRenderer>().enabled = true;
        }

    }

    public void DeactivateArrowCameraIfVuforiaDisabled()
    {
        if (!Vuforia.CameraDevice.Instance.IsActive())
        {
            GameObject arrowCamera = GameObject.FindGameObjectWithTag("ArrowCamera");
            arrowCamera.GetComponent<MeshRenderer>().enabled = false;
        }

    }

    public void StreamingOff()
    {
        if (!Vuforia.CameraDevice.Instance.IsActive())
        {
            Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_DEFAULT);

            Vuforia.CameraDevice.Instance.Start();

            cursorMaterial.color = new Color(1f, 1f, 1f, 1.0f);
        }

        
        GameObject arrowCamera = GameObject.FindGameObjectWithTag("ArrowCamera");
        arrowCamera.GetComponent<MeshRenderer>().enabled = false;
        

    }

    public void OceanTexturesOn()
    {
        if (DataController.instance.textures != true)
        {
            DataController.instance.textures = true;
            UpdateTextures();
            DataController.instance.SavePrefsTextures();
            Debug.Log("Ocean texture: " + DataController.instance.textures);
        } 
    }

    public void OceanTexturesOff()
    {
        if (DataController.instance.textures != false)
        {
            DataController.instance.textures = false;
            UpdateTextures();
            DataController.instance.SavePrefsTextures();
            Debug.Log("Ocean texture: " + DataController.instance.textures);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            Debug.Log("Bloom gesture detected"); //I could quit, but now it seems there are no problems in resuming from pause //QuitApp();
    }

    public void QuitApp()
    {
        cursorMaterial.color = new Color(1f, 1f, 1f, 1.0f);
        WebClientSender.instance.CloseEyesDolphin();
        Application.Quit();
    }

    public void ShowInsertPlayerNameMenu()
    {
        PutInFrontOfCamera(insertPlayerNameMenu);
        insertPlayerNameMenu.SetActive(true);
    }

    public void SetDigitPlayerName(GameObject selectedUIDigit)
    {
        //Text is the child
        string digit = selectedUIDigit.GetComponentInChildren<TextMesh>().text;

        if (digit == "<" && myTempPlayerName.Length > 0)
        {
            myTempPlayerName = myTempPlayerName.Remove(myTempPlayerName.Length - 1);
            labelInsertedPlayerName.text = myTempPlayerName;
            if (myTempPlayerName.Length == 0)
                insertPlayerNameOkButton.SetActive(false);
            return;
        }

        if (myTempPlayerName.Length == PlayerNameMaxDigits)
            return;

        myTempPlayerName += digit;
        labelInsertedPlayerName.text = myTempPlayerName;
        
        Debug.Log("Player Name is: " + myTempPlayerName);

        if (myTempPlayerName.Length > 0)
            insertPlayerNameOkButton.SetActive(true);
        else
            insertPlayerNameOkButton.SetActive(false);
    }

    public void SetLastPlayerName()
    {
        if (nameOfLastPlayer.Length > 0)
        {
            myTempPlayerName = nameOfLastPlayer;
            labelInsertedPlayerName.text = myTempPlayerName;
            insertPlayerNameOkButton.SetActive(true);
        }
        
    }


    public void PopulateHighscoresMenu()
    {
        highscoresDataScoreTextMesh.text = "";
        highscoresDataNamesTextMesh.text = "";
        foreach (MatchData match in DataController.instance.top3)
        {
            highscoresDataScoreTextMesh.text += match.scorePlayer + "\n\n";
            highscoresDataNamesTextMesh.text += match.namePlayer + "\n\n";
        }
    }

    public void PopulateStatsMenu()
    {

        statsDataTextMesh.text = "";

        MatchData[] matchesToPrint = DataController.instance.allMatches.Take(numMatchesToPrint).ToArray();
        if (matchesToPrint.Length == 0)
        {
            statsDataTextMesh.text = "\n\n\n\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tNO MATCHES FOUND\n\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t  DO A MATCH!";
            return;
        }

        foreach (MatchData match in DataController.instance.allMatches.Take(numMatchesToPrint))
        {
            statsDataTextMesh.text += "Match: " + match.numMatch + " | Date: " + match.dateTime
                                    + " | Name: " + match.namePlayer + " | Score: " + match.scorePlayer
                                    + " | GameMode: " + match.gameMode + "\n";

            foreach (LevelData level in match.levels)
            {
                statsDataTextMesh.text += "\t\t";
                if (match.gameMode == GameController.GameMode.ADVENTURE)
                {
                    statsDataTextMesh.text += "Lev: " + level.level + " | ";
                }

                statsDataTextMesh.text += "TotRings: " + level.totRings + " | RateRings: " + (level.rateYesRings * 100d).ToString("F0") + "%" 
                                        + " | ErrorTolerance: " + level.errToleranceRings;

                if (match.gameMode == GameController.GameMode.INFINITY)
                {
                    statsDataTextMesh.text += " | TotObstacles: " + level.totObstacles + " | RateAvoidedObs: " + (level.rateAvoidedObstacles * 100d).ToString("F0") + "%";

                }

                if (match.gameMode == GameController.GameMode.ADVENTURE)
                {
                    statsDataTextMesh.text += " | TimeTreasureHunt: " + level.timeTreasure + "m";

                }
                statsDataTextMesh.text += "\n";


            }

            statsDataTextMesh.text += "\n";
        }
    }

    public void SaveNameAndStatistics()
    {
        DataController.instance.currentMatch.namePlayer = myTempPlayerName;
        DataController.instance.SaveCurrentMatchData();

        nameOfLastPlayer = myTempPlayerName;

        //Alla fine, ripulisci il campo nome etc per la prossima partita
        CleanNameAndStatisticsForNextGame();
    }


    public void CleanNameAndStatisticsForNextGame()
    {
        myTempPlayerName = "";
        labelInsertedPlayerName.text = myTempPlayerName;

        //Clean game controller stats list
        DataController.instance.currentMatch = null;

        Invoke("ScanningCompleteOkPressed", 1f);
        //Invoke("ShowMainMenu", 3f);

    }



    //SETTINGS MENU
    public void LoadGeneralSettingsMenu(GameObject generalSettingsMenu)
    {
        foreach (Transform gizmoSetting in generalSettingsMenu.transform)
        {
            switch ( gizmoSetting.gameObject.name)
            {
                case "ToggleMusic":
                    gizmoSetting.gameObject.GetComponent<InteractiveToggle>().HasSelection = DataController.instance.musicIsActive;
                    break;
                case "ToggleAudioEffects":
                    gizmoSetting.gameObject.GetComponent<InteractiveToggle>().HasSelection = DataController.instance.audioEffectsIsActive;
                    break;
                case "ToggleSmartObject":
                    gizmoSetting.gameObject.GetComponent<InteractiveToggle>().HasSelection = DataController.instance.smartObjectIsActive;
                    break;
                case "ToggleLanguage":
                    gizmoSetting.gameObject.GetComponent<InteractiveToggle>().HasSelection = DataController.instance.languageItalian;
                    break;
                case "ToggleTextures":
                    gizmoSetting.gameObject.GetComponent<InteractiveToggle>().HasSelection = DataController.instance.textures;
                    break;
            }
        }
    }

    public void ChangeGeneralSettingMenu(GameObject uxElementManipulated)
    {
        switch (uxElementManipulated.name)
        {
            case "ToggleMusic":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection) //It has been clicked the toggle, NOW it hasSelection, but I've selected the toggle to turn off, so we have to update musicIsActive = false
                {
                    DataController.instance.musicIsActive = false;
                    SoundManager.instance.StopMusic();
                }
                else
                {
                    DataController.instance.musicIsActive = true;
                    SoundManager.instance.ReactivateMusic();
                }
                DataController.instance.SavePrefsMusic();
                break;
            case "ToggleAudioEffects":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    DataController.instance.audioEffectsIsActive = false;
                }
                else
                {
                    DataController.instance.audioEffectsIsActive = true; 
                }
                DataController.instance.SavePrefsAudioEffects();
                break;
            case "ToggleSmartObject":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    DataController.instance.smartObjectIsActive = false;
                }
                else
                {
                    DataController.instance.smartObjectIsActive = true;
                }
                DataController.instance.SavePrefsSmartObject();
                break;
            case "ToggleTextures":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    DataController.instance.textures = false;
                }
                else
                {
                    DataController.instance.textures = true;
                }
                UpdateTextures();
                DataController.instance.SavePrefsTextures();
                break;
            case "ToggleLanguage":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    DataController.instance.languageItalian = false; 
                }
                else
                {
                    DataController.instance.languageItalian = true;
                }
                DataController.instance.SavePrefsLanguage();
                break;
        }
    }


    //Changes the alpha value of the textures (allows more or less immersivity, saying "Ocean Floor On/Off")
    public void UpdateTextures()
    {

        if (DataController.instance.textures)
        {
            ChangeAlpha(floorsMaterial, 1f);
            ChangeAlpha(wallsMaterial, 1f);
        }
        else
        {
            ChangeAlpha(wallsMaterial, 0f);
            ChangeAlpha(floorsMaterial, 0f);
        }

    }

    public static void ChangeAlpha(Material mat, float alphaValue)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
        mat.SetColor("_Color", newColor);
    }




    //INFINITY MODE SETTINGS MENU
    public void SaveSettingsInfinity()
    {
        bool isDefaultPrefs = false;
        DataController.instance.SavePrefsInfinity(isDefaultPrefs);
    }

    public void ResetSettingsInfinity()
    {
        DataController.instance.ResetDataInfinityPrefs();
    }

    public void LoadGizmosSettingsInfinityMenu(GameObject settingsInfinityMenuObject)
    {
        foreach (Transform infMenuObject in settingsInfinityMenuObject.transform)
        {
            switch (infMenuObject.gameObject.name)
            {
                case "SliderActiveGenerators":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].activeGenerators);
                    break;
                case "SliderInitialHealth":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].healthPlayerInit);
                    break;
                case "SliderNumElementsPerWave":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].elementsPerWave);
                    break;
                case "SliderNumWaves":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].numberOfWaves);
                    break;
                case "SliderSpawnWait":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].spawnWait);
                    break;
                case "SliderWaveWait":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].waveWait);
                    break;
                case "SliderSpeedElements":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].speedElements);
                    break;
                case "SliderErrorTolerance":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].errorToleranceRings);
                    break;
                case "SliderRotationLxRings":
                    //infMenuObject.gameObject.GetComponent<SliderGestureControl>().SetSliderValue(GameController.instance.levelsParams[0].elementsRotationMin.y);
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].elementsRotationMin.y);
                    break;
                case "SliderRotationRxRings":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].elementsRotationMax.y);
                    break;
                case "ToggleObstacles":
                    bool foundObstacle = false;
                    foreach (GameObject element in GameController.instance.levelsParams[0].elements)
                    {
                        if (element.name.Contains("Obstacle"))
                        {
                            foundObstacle = true;
                        }
                    }
                    if (foundObstacle)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = true;
                    else
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = false;
                    break;
                case "ToggleSharks":
                    bool foundShark = false;
                    foreach (GameObject element in GameController.instance.levelsParams[0].elements)
                    {
                        if (element.name.Contains("Shark"))
                        {
                            foundShark = true;
                        }
                    }
                    if (foundShark)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = true;
                    else
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = false;
                    break;
                case "ToggleEnergy":                   
                    if (GameController.instance.levelsParams[0].energyConsumingIsActive)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = true;
                    else
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = false;
                    break;
                case "ToggleCulturalBits":
                    if (GameController.instance.levelsParams[0].culturalBitsIsActive)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = true;
                    else
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = false;
                    break;
                case "TogglePowerups":
                    if (GameController.instance.levelsParams[0].powerups.Count > 0)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = true;
                    else if (GameController.instance.levelsParams[0].powerups.Count == 0)
                        infMenuObject.gameObject.GetComponent<InteractiveToggle>().HasSelection = false;
                    else
                        Debug.LogError("Problem with powerups");
                    break;
                case "SliderEnergyConsumingTime":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].energyConsumingTimeInterval);
                    break;
                case "SliderTimerX2Powerup":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].doubleX2Timer);
                    break;
                case "SliderSpawnWaitPowerups":
                    infMenuObject.gameObject.GetComponent<SliderGestureControlCustom>().SetSliderValueAndReinitAwake_Custom(GameController.instance.levelsParams[0].spawnWaitPowerups);
                    break;
            }
        }
    }

    public void ChangeSettingInfinityMode(GameObject uxElementManipulated)
    {
        //If I use foreach (for example in the case of toggles) and I modify the list, I have to create an array (or I have to use a for loop) because the foreach require the list to not being modified (it can't know how many elements there are, can't loop)
        GameObject[] elementsArray = GameController.instance.levelsParams[0].elements.ToArray();

        switch (uxElementManipulated.name)
        {
            case "SliderActiveGenerators":
                GameController.instance.levelsParams[0].activeGenerators = Convert.ToInt32(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderInitialHealth": 
                GameController.instance.levelsParams[0].healthPlayerInit = Convert.ToInt32(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderNumElementsPerWave":
                GameController.instance.levelsParams[0].elementsPerWave = Convert.ToInt32(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderNumWaves":
                GameController.instance.levelsParams[0].numberOfWaves = Convert.ToInt32(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderSpawnWait":
                GameController.instance.levelsParams[0].spawnWait = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderWaveWait":
                GameController.instance.levelsParams[0].waveWait = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderSpeedElements":
                GameController.instance.levelsParams[0].speedElements = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderErrorTolerance":
                GameController.instance.levelsParams[0].errorToleranceRings = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderRotationLxRings":
                GameController.instance.levelsParams[0].elementsRotationMin.y = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderRotationRxRings":
                GameController.instance.levelsParams[0].elementsRotationMax.y = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "ToggleObstacles":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    foreach (GameObject element in elementsArray) 
                    {
                        if (element.name.Contains("Obstacle"))
                            GameController.instance.levelsParams[0].elements.Remove(element);
                    }
                }
                else
                {
                    //Adding two obstacles to the list of elements spawned (during the game an element will be selected randomly, therefore more objects we add more probability it has to be spawned
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.obstacle1Bottle);
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.obstacle2Can);
                }
                break;
            case "ToggleSharks":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    foreach (GameObject element in elementsArray)
                    {
                        if (element.name.Contains("Shark"))
                            GameController.instance.levelsParams[0].elements.Remove(element);
                    }
                }
                else
                {
                    //Adding two obstacles to the list of elements spawned (during the game an element will be selected randomly, therefore more objects we add more probability it has to be spawned
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.shark1);
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.shark2);
                }
                break;
            case "ToggleEnergy":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    GameController.instance.levelsParams[0].energyConsumingIsActive = false;
                    foreach (GameObject element in elementsArray)
                    {
                        if (element.name.Contains("Octopus"))
                            GameController.instance.levelsParams[0].elements.Remove(element);
                    }

                }
                else
                {
                    GameController.instance.levelsParams[0].energyConsumingIsActive = true;
                    //Adding two obstacles to the list of elements spawned (during the game an element will be selected randomly, therefore more objects we add more probability it has to be spawned
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.food);
                    GameController.instance.levelsParams[0].elements.Add(GameController.instance.food);
                }
                break;
            case "ToggleCulturalBits":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    GameController.instance.levelsParams[0].culturalBitsIsActive = false;
                }
                else
                {
                    GameController.instance.levelsParams[0].culturalBitsIsActive = true;
                }
                break;
            case "TogglePowerups":
                if (uxElementManipulated.GetComponent<InteractiveToggle>().HasSelection)
                {
                    GameObject[] powerupsArray = GameController.instance.levelsParams[0].powerups.ToArray();
                    foreach (GameObject element in powerupsArray)
                    {
                        if (element.name.Contains("Health") || element.name.Contains("DoubleX2"))
                            GameController.instance.levelsParams[0].powerups.Remove(element);
                    }

                }
                else
                {
                    //Adding two obstacles to the list of elements spawned (during the game an element will be selected randomly, therefore more objects we add more probability it has to be spawned
                    GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
                    GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
                    GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
                    GameController.instance.levelsParams[0].powerups.Add(GameController.instance.x2Powerup);
                }
                break;
            case "SliderEnergyConsumingTime":
                GameController.instance.levelsParams[0].energyConsumingTimeInterval = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderTimerX2Powerup":
                GameController.instance.levelsParams[0].doubleX2Timer = Convert.ToInt32(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;
            case "SliderSpawnWaitPowerups":
                GameController.instance.levelsParams[0].spawnWaitPowerups = Utilities.TruncateFloatTwoDecimals(uxElementManipulated.GetComponent<SliderGestureControlCustom>().SliderValue);
                break;

        }
    }

    
}
