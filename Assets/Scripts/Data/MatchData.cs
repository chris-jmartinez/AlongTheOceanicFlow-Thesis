using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchData {
    public int numMatch; //KEY
    public string dateTime; //dateTime has to be a string because DateTime isn't serialized well to json (it works just on UWP) (backup: DateTime dateTime)
    public string namePlayer;
    public int scorePlayer;
    public GameController.GameMode gameMode;
    public List<LevelData> levels;

    //Constructor invoked when new MatchData is used
    public MatchData()
    {
        this.numMatch = -1;
        this.dateTime = (DateTime.Now).ToString(); 
        this.namePlayer = "";
        this.scorePlayer = 0;
        this.gameMode = GameController.GameMode.IDLE;
        this.levels = new List<LevelData>();
        AddNewLevel();

    }

    public void AddNewLevel()
    {
        this.levels.Add(new LevelData());
    }

    //Functions Conversion from DateTime to String and viceversa
    //DateTime dateTime = DateTime.Now;
    //string dateTimeString = dateTime.ToString();
    //DateTime.Parse(dateTimeNewString)    Convert.ToDateTime(dateTimeNewString)
}
