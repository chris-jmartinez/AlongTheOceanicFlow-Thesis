using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    public ParticleSystem dolphinTouchLight;

    private const string TYPE_TOUCH = "touch";
    private const string ID_HEAD = "0";
    private const string ID_DORSAL_FIN = "5";
    private const string ACTION_PRESSED = "1";
    private const string ACTION_RELEASED = "0";

    private const string TYPE_DECOR = "decor";
    private const string ID_NEW_DECOR = "9";
    private const string ACTION_DECOR = "1";

    public void CheckEvent(EventObject eventObj)
    {
        string type = eventObj.getType();
        string id = eventObj.getID();
        string active = eventObj.getActive();
        string duration = eventObj.getDuration();

        if (type == TYPE_TOUCH && id == ID_HEAD && active == ACTION_PRESSED && duration == null)
            DolphinHeadPressed();
        else if (type == TYPE_TOUCH && id == ID_DORSAL_FIN && active == ACTION_PRESSED && duration == null)
            DolphinDorsalFinPressed();

        if (type == TYPE_DECOR && id == ID_NEW_DECOR && active == ACTION_DECOR && duration == null && 
           (GameController.instance.currentPhaseOfLevel != GameController.PhaseOfLevel.TRAVEL_FLOW) 
           )
        {
            DecorationsSpawnerManager.Instance.DecorationAtStart();
        }
                

    }


    private void DolphinHeadPressed()
    {
        Debug.Log("DOLPHIN HEAD PRESSED!");
        dolphinTouchLight.Play();
        GameObject.FindGameObjectWithTag("CameraCollider").GetComponent<PlayerSounds>().PlayYahoo();
        if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TRAVEL_FLOW)
            GameController.instance.AddEnergy(0.03f);
    }

    private void DolphinDorsalFinPressed()
    {
        //Play sound yeehaa
        GameObject.FindGameObjectWithTag("CameraCollider").GetComponent<PlayerSounds>().PlayYeehaw();
        Debug.Log("DOLPHIN DORSAL FIN PRESSED!!!");
        if (GameController.instance.currentPhaseOfLevel == GameController.PhaseOfLevel.TRAVEL_FLOW)
            GameController.instance.AddEnergy(0.03f);
    }
}
