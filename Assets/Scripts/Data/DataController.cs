using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;


public class DataController : MonoBehaviour {

    public static DataController instance = null;
    public List<MatchData> allMatches;
    public List<MatchData> top3;
    public MatchData currentMatch;

    //elements to save in unity PLAYER PREFS
    public int numMatchesCounter; //Number that keeps track of all the matches played
    public bool musicIsActive;
    public bool audioEffectsIsActive;
    public bool smartObjectIsActive;
    public bool languageItalian;
    public bool textures;

    private string gameDataFilename = "dataMatches.json";
    private string filePath;

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
        DontDestroyOnLoad(gameObject);
        filePath = string.Format("{0}" + Path.DirectorySeparatorChar + "{1}", Application.persistentDataPath, gameDataFilename);
        Debug.Log("Path to save/load files: " + Application.persistentDataPath);

        musicIsActive = true;
        audioEffectsIsActive = true;
        languageItalian = true;
        smartObjectIsActive = false;
        textures = false;

        numMatchesCounter = 0; //First time application is started the value should be 0, then if we already played the game we will have the value stored in playerPrefs

        //PlayerPrefs.DeleteAll(); //Use to eliminate settings

        if (PlayerPrefs.HasKey("prefsInfinityModeDefault") == false)
        {
            SavePrefsInfinity(true);
        }
         
        LoadGameData();
        LoadPrefs();

    }



    public void CreateNewDataMatch()
    {
        currentMatch = new MatchData();

    }
    public void SaveCurrentMatchData()
    {
        numMatchesCounter++;
        PlayerPrefs.SetInt("numMatchesCounter", numMatchesCounter);

        currentMatch.numMatch = numMatchesCounter;
        //currentMatch.scorePlayer = GameController.instance.ScorePlayer;
       
        allMatches.Add(currentMatch);

        //Incompatibility issue: for ordering from last to first match I could use dateTime but due to Incompatibilies in converting dateTime to json, it's better to order by numberOfMatch
        //allMatches.Sort((x, y) => y.dateTime.CompareTo(x.dateTime)); //To order in place, without creating another list
        allMatches.Sort((x, y) => y.numMatch.CompareTo(x.numMatch));

        top3 = allMatches.OrderByDescending(x => x.scorePlayer).Take(3).ToList(); //Ordering using LINQ and creating another list

        GameData allMatchesToSave = new GameData();
        allMatchesToSave.allMatches = allMatches;

        string newGameDataAsJson = JsonUtility.ToJson(allMatchesToSave, true); //INCOMPATIBILITY: In hololens it doesn't save the Date variable. (beautifier or not, it's indifferent)
        //string newGameDataAsJson = JsonConvert.SerializeObject(allMatchesToSave);
        //string newGameDataAsJsonFormatted = JValue.Parse(newGameDataAsJson).ToString(Formatting.Indented); //I prefer jsonUtility pretty print, but it's the same

        byte[] gameDataBytes = Encoding.ASCII.GetBytes(newGameDataAsJson);

        UnityEngine.Windows.File.WriteAllBytes(filePath, gameDataBytes);

        //FUTURE WORK MAYBE: could try to save automatically in Documents folder a pretty json version: but it seems it's possible only with a save file picker (where user decides where to save the file).
        //This code "Windows.Storage" is only accessible outside unity, so if i'm using this method I have to put a !if_UnityEditor
        //Debug.Log("Documents filepath" + Windows.Storage.KnownFolders.DocumentsLibrary.Path);


        //Old version: streaming assets: shouldn't be used to store game data file. Additionally, it didn't work on hololens, just on unity
        //string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFilename); //Non funziona su hololens, solo in unity
        //string newGameDataAsJson = JsonUtility.ToJson(allMatchesToSave, true); //Non funziona in hololens, solo in unity
        //File.WriteAllText(filePath, newGameDataAsJson); Ok in unity ma Crasha in Hololens

    }

    public void printCurrentMatchData()
    {
        Debug.Log("CURRENT MATCH: numMatch: " + currentMatch.numMatch + " Date: " + currentMatch.dateTime + " namePlayer: " + currentMatch.namePlayer + " score: " + currentMatch.scorePlayer + " GameMode: " + currentMatch.gameMode + "\n");
        foreach (LevelData level in currentMatch.levels)
        {
            Debug.Log("Level: " + level.level + " yesRings: " + level.yesRings + " totRings: " + level.totRings + " rateRings: " + level.rateYesRings + " errTol: " + level.errToleranceRings + " timeTreasure: " + level.timeTreasure + "\n");
        }
    }

    private void LoadGameData()
    {
        //UnityEngine.Windows.File.Delete(filePath);
        

        if (File.Exists(filePath))
        {
            //string dataAsJson = File.ReadAllText(filePath); //Used in Unity editor but doesn't work in hololens
            byte[] gameDataBytes = UnityEngine.Windows.File.ReadAllBytes(filePath);
            string dataAsJson = Encoding.ASCII.GetString(gameDataBytes);

            Debug.Log("Loaded game data json: " + dataAsJson);
            if (dataAsJson.Length != 0)
            {
                GameData loadedGameData = new GameData();

                //loadedGameData = JsonConvert.DeserializeObject(dataAsJson) as GameData;
                loadedGameData = JsonUtility.FromJson<GameData>(dataAsJson);
               
                allMatches = loadedGameData.allMatches;
                top3 = allMatches.OrderByDescending(x => x.scorePlayer).Take(3).ToList();
            }
            else
            {
                Debug.Log("Empty data file. Do a match");
            }
            
            

        }
        else
        {
            Debug.Log("Cannot load game data. File " + filePath + " doesn't exist");
            CreateNewGameDataStatsFile();
        }

        

    }

    public void CreateNewGameDataStatsFile()
    {
        //Let's create a new file and RESET statistics
        string newGameDataAsJson = "";
        byte[] gameDataBytes = Encoding.ASCII.GetBytes(newGameDataAsJson);
        UnityEngine.Windows.File.WriteAllBytes(filePath, gameDataBytes);
        Debug.Log("Created new empty game data file, at filepath: " + filePath);

        allMatches.Clear();
        top3.Clear();
    }


    

    private void LoadPrefs()
    {
        
        if (PlayerPrefs.HasKey("numMatchesCounter"))
        {
            numMatchesCounter = PlayerPrefs.GetInt("numMatchesCounter");
        }

        if (PlayerPrefs.HasKey("prefsMusic"))
        {
            musicIsActive = Convert.ToBoolean(PlayerPrefs.GetInt("prefsMusic"));
        }

        if (PlayerPrefs.HasKey("prefsAudioEffects"))
        {
            audioEffectsIsActive = Convert.ToBoolean(PlayerPrefs.GetInt("prefsAudioEffects"));
        }

        if (PlayerPrefs.HasKey("prefsSmartObject"))
        {
            smartObjectIsActive = Convert.ToBoolean(PlayerPrefs.GetInt("prefsSmartObject"));
        }

        if (PlayerPrefs.HasKey("prefsLanguage"))
        {
            languageItalian = Convert.ToBoolean(PlayerPrefs.GetInt("prefsLanguage"));
        }

        if (PlayerPrefs.HasKey("prefsTextures"))
        {
            textures = Convert.ToBoolean(PlayerPrefs.GetInt("prefsTextures"));
            UiMenuHandler.instance.UpdateTextures();
        }

        if (PlayerPrefs.HasKey("prefsInfinityMode"))
        {
            //GameController.instance.levelsParams[0] = JsonUtility.FromJson<GameController.LevelsParameters>(PlayerPrefs.GetString("prefsInfinityMode"));
            LoadPrefsInfinity(false);
        }
    }

    public void SavePrefsInfinity(bool isDefaultPrefs)
    {
        //Unfortunately, serializing and converting gameobjects or complex types works bad sometimes, so we can't directly convert levelparams[0]
        //string prefsInfinityMode = JsonUtility.ToJson(GameController.instance.levelsParams[0]);
        //PlayerPrefs.SetString("prefsInfinityMode", prefsInfinityMode);

        //Solution saving separately only needed fields and with simple type:
        InfinityModePrefsData newInfinityPrefsData = new InfinityModePrefsData();
        newInfinityPrefsData.activeGenerators = GameController.instance.levelsParams[0].activeGenerators;
        newInfinityPrefsData.healthPlayerInit = GameController.instance.levelsParams[0].healthPlayerInit;
        newInfinityPrefsData.elementsPerWave = GameController.instance.levelsParams[0].elementsPerWave;
        newInfinityPrefsData.numberOfWaves = GameController.instance.levelsParams[0].numberOfWaves;
        newInfinityPrefsData.spawnWait = GameController.instance.levelsParams[0].spawnWait;
        newInfinityPrefsData.waveWait = GameController.instance.levelsParams[0].waveWait;

        newInfinityPrefsData.speedElements = GameController.instance.levelsParams[0].speedElements;
        newInfinityPrefsData.errorToleranceRings = GameController.instance.levelsParams[0].errorToleranceRings;
        newInfinityPrefsData.elementsRotationMaxY = GameController.instance.levelsParams[0].elementsRotationMax.y;
        newInfinityPrefsData.elementsRotationMinY = GameController.instance.levelsParams[0].elementsRotationMin.y;

        bool foundObstacle = false;
        foreach (GameObject element in GameController.instance.levelsParams[0].elements)
        {
            if (element.name.Contains("Obstacle"))
            {
                foundObstacle = true;
            }
        }
        newInfinityPrefsData.obstaclesIsActive = (foundObstacle) ? true : false;

        bool foundShark = false;
        foreach (GameObject element in GameController.instance.levelsParams[0].elements)
        {
            if (element.name.Contains("Shark"))
            {
                foundShark = true;
            }
        }
        newInfinityPrefsData.sharksIsActive = (foundShark) ? true : false;
        newInfinityPrefsData.energyConsumingIsActive = GameController.instance.levelsParams[0].energyConsumingIsActive;
        newInfinityPrefsData.powerupsIsActive = (GameController.instance.levelsParams[0].powerups.Count > 0) ? true : false;
        newInfinityPrefsData.culturalBitsIsActive = GameController.instance.levelsParams[0].culturalBitsIsActive;

        newInfinityPrefsData.energyConsumingTimeInterval = GameController.instance.levelsParams[0].energyConsumingTimeInterval;
        newInfinityPrefsData.doubleX2Timer = GameController.instance.levelsParams[0].doubleX2Timer;
        newInfinityPrefsData.spawnWaitPowerups = GameController.instance.levelsParams[0].spawnWaitPowerups;

        string prefsInfinityMode = JsonUtility.ToJson(newInfinityPrefsData);
        if (isDefaultPrefs)
            PlayerPrefs.SetString("prefsInfinityModeDefault", prefsInfinityMode);
        else
            PlayerPrefs.SetString("prefsInfinityMode", prefsInfinityMode);
    }

    public void LoadPrefsInfinity(bool isDefaultPrefs)
    {
        InfinityModePrefsData infinityPrefs;

        if (isDefaultPrefs)
        {
            infinityPrefs = JsonUtility.FromJson<InfinityModePrefsData>(PlayerPrefs.GetString("prefsInfinityModeDefault"));
        }
        else
        {
            infinityPrefs = JsonUtility.FromJson<InfinityModePrefsData>(PlayerPrefs.GetString("prefsInfinityMode"));
        }

        GameController.instance.levelsParams[0].activeGenerators = infinityPrefs.activeGenerators;
        GameController.instance.levelsParams[0].healthPlayerInit = infinityPrefs.healthPlayerInit;
        GameController.instance.levelsParams[0].elementsPerWave = infinityPrefs.elementsPerWave;
        GameController.instance.levelsParams[0].numberOfWaves = infinityPrefs.numberOfWaves;
        GameController.instance.levelsParams[0].spawnWait = infinityPrefs.spawnWait;
        GameController.instance.levelsParams[0].waveWait = infinityPrefs.waveWait;

        GameController.instance.levelsParams[0].speedElements = infinityPrefs.speedElements;
        GameController.instance.levelsParams[0].errorToleranceRings = infinityPrefs.errorToleranceRings;
        GameController.instance.levelsParams[0].elementsRotationMax.y = infinityPrefs.elementsRotationMaxY;
        GameController.instance.levelsParams[0].elementsRotationMin.y = infinityPrefs.elementsRotationMinY;

        GameController.instance.levelsParams[0].energyConsumingTimeInterval = infinityPrefs.energyConsumingTimeInterval;
        GameController.instance.levelsParams[0].doubleX2Timer = infinityPrefs.doubleX2Timer;
        GameController.instance.levelsParams[0].spawnWaitPowerups = infinityPrefs.spawnWaitPowerups;

        GameObject[] elementsArray = GameController.instance.levelsParams[0].elements.ToArray();

        if (infinityPrefs.energyConsumingIsActive)
        {
            GameController.instance.levelsParams[0].energyConsumingIsActive = true;
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.food);
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.food);
        }
        else
        {
            GameController.instance.levelsParams[0].energyConsumingIsActive = false;
            foreach (GameObject element in elementsArray)
            {
                if (element.name.Contains("Octopus"))
                    GameController.instance.levelsParams[0].elements.Remove(element);
            }
        }
            

        if (infinityPrefs.obstaclesIsActive)
        {
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.obstacle1Bottle);
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.obstacle2Can);
        }
        else
        {
            foreach (GameObject element in elementsArray)
            {
                if (element.name.Contains("Obstacle"))
                    GameController.instance.levelsParams[0].elements.Remove(element);
            }
        }

        if (infinityPrefs.sharksIsActive)
        {
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.shark1);
            GameController.instance.levelsParams[0].elements.Add(GameController.instance.shark2);
        }
        else
        {
            foreach (GameObject element in elementsArray)
            {
                if (element.name.Contains("Shark"))
                    GameController.instance.levelsParams[0].elements.Remove(element);
            }
        }

        if (infinityPrefs.powerupsIsActive)
        {
            GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
            GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
            GameController.instance.levelsParams[0].powerups.Add(GameController.instance.healthPowerup);
            GameController.instance.levelsParams[0].powerups.Add(GameController.instance.x2Powerup);
        }
        else
        {
            if (GameController.instance.levelsParams[0].powerups.Count > 0)
                GameController.instance.levelsParams[0].powerups.Clear();
        }

        GameController.instance.levelsParams[0].culturalBitsIsActive = infinityPrefs.culturalBitsIsActive;



    }

    public void ResetDataInfinityPrefs()
    {
        bool isDefaultPrefs = true; //variable created only for readability

        if (PlayerPrefs.HasKey("prefsInfinityModeDefault"))
        {
            //Unfortunately I cannot deserialize directly GameObjects and stuff like that, so we need an intermediate function LoadPrefsInfinity that loads each one of the fields, otherwise it would have been more simple to do:
            //GameController.LevelsParameters defaultInfinityPrefs = JsonUtility.FromJson<GameController.LevelsParameters>(PlayerPrefs.GetString("prefsInfinityModeDefault"));
            //GameController.instance.levelsParams[0] = defaultInfinityPrefs;

            
            LoadPrefsInfinity(isDefaultPrefs);
            SavePrefsInfinity(!isDefaultPrefs);
        }

        
    }

    public void SavePrefsMusic()
    {

        PlayerPrefs.SetInt("prefsMusic", Convert.ToInt32(musicIsActive));
    }

    public void SavePrefsAudioEffects()
    {
        PlayerPrefs.SetInt("prefsAudioEffects", Convert.ToInt32(audioEffectsIsActive));
    }

    public void SavePrefsSmartObject()
    {
        PlayerPrefs.SetInt("prefsSmartObject", Convert.ToInt32(smartObjectIsActive));
    }
    public void SavePrefsLanguage()
    {
        PlayerPrefs.SetInt("prefsLanguage", Convert.ToInt32(languageItalian));
    }

    public void SavePrefsTextures()
    {
        PlayerPrefs.SetInt("prefsTextures", Convert.ToInt32(textures));
    }


}
