using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData {
    public int level;
    public int yesRings;
    public int totRings;
    public double rateYesRings;
    public double errToleranceRings;
    public int avoidedObstacles;
    public int totObstacles;
    public double rateAvoidedObstacles;
    public double timeTreasure;

    public LevelData()
    {
        level = -1;
        yesRings = 0;
        totRings = 0;
        rateYesRings = 0;
        errToleranceRings = -1;
        avoidedObstacles = 0;
        totObstacles = 0;
        rateAvoidedObstacles = 0;
        timeTreasure = 0;
    }
}
