using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFound : MonoBehaviour {



    public void KeyHasBeenFound(GameObject key)
    {
        GameObject[] treasureChests = GameObject.FindGameObjectsWithTag("TreasureChest");
        foreach (GameObject treasureChest in treasureChests)
            treasureChest.GetComponent<ChestEvent>().ItemFound(key);
        transform.gameObject.SetActive(false);
    }
}
