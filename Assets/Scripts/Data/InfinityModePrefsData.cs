using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InfinityModePrefsData {

    [Range(1, 4)]
    public int activeGenerators;
    public int healthPlayerInit;
    public int elementsPerWave;
    public int numberOfWaves;
    public float spawnWait;
    public float waveWait;
    public float speedElements;
    public float errorToleranceRings;
    public float elementsRotationMinY; //substituted vector3 with a float that is better serializable (in theory it shouldn' give problems anyway with vector3, but just for making sure)
    public float elementsRotationMaxY;

    public bool obstaclesIsActive;
    public bool sharksIsActive;
    public bool energyConsumingIsActive;
    public bool powerupsIsActive;
    public bool culturalBitsIsActive;

    public float energyConsumingTimeInterval;
    public int doubleX2Timer;
    public float spawnWaitPowerups;

    //I created this class because unfortunately gameObjects can't be serialized well, serialization is broken and works only sometimes
    //public List<GameObject> elements;
    //public List<GameObject> powerups;
    //public List<GameObject> treasureHuntObjects;
    //public List<GameObject> culturalMessages;
}
