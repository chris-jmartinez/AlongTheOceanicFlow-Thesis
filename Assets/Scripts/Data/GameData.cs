using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData { //IMPORTANT NOTE: all classes and properties that have to store data MUST be declared as serializable and cannot derive from Monobehaviour! Otherwise it will throw an exception when loading data

    public List<MatchData> allMatches;
	
}
