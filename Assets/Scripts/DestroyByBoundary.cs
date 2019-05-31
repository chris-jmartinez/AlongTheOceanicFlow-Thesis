using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyByBoundary : MonoBehaviour {


    
    public GameController gameController;
    public GameObject lightningEffect;
    private string lightningEffectName;

	// Use this for initialization
	void Start () {
        lightningEffectName = lightningEffect.name;
	}
	
	


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("TorusBubble"))
        {
            Animator bubbleAnimator = other.GetComponent<Animator>();
            bubbleAnimator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));

            if (GameController.instance.gameModeSelected != GameController.GameMode.TUTORIAL  &&  GameController.instance.gameModeSelected != GameController.GameMode.TUTORIAL_BASIC)
                gameController.HealthPlayer--;

            double rateYesRingsCalculation = ((double)DataController.instance.currentMatch.levels.Last().yesRings) / DataController.instance.currentMatch.levels.Last().totRings;
            double rateYesRingsRounded = System.Math.Round(rateYesRingsCalculation, 2, System.MidpointRounding.AwayFromZero);
            DataController.instance.currentMatch.levels.Last().rateYesRings = rateYesRingsRounded;

        }

        if (other.gameObject.name.Contains("Food"))
        {
            Animator foodAnimator = other.GetComponent<Animator>();
            foodAnimator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));

            //gameController.HealthPlayer += -1;
            
        }

        if (other.gameObject.name.Contains("DoubleX2Powerup"))
        {
            Animator x2Animator = other.GetComponent<Animator>();
            x2Animator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));

            

        }


        if (other.gameObject.name.Contains("HealthPowerup"))
        {
            Animator healthAnimator = other.GetComponent<Animator>();
            healthAnimator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));

            

        }

        if (other.gameObject.name.Contains("Obstacle"))
        {
            Animator obstacleAnimator = other.GetComponent<Animator>();
            obstacleAnimator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));

            GameController.instance.IncrementAvoidedObstacles();

        }

        if (other.gameObject.name.Contains("Hazard"))
        {
            Animator hazardAnimator = other.GetComponent<Animator>();
            hazardAnimator.SetTrigger("Deactivating");
            StartCoroutine(deactivateAfterTime(other.gameObject));
            //gameController.HealthPlayer += -1;
        }

        
    }

 


    private IEnumerator deactivateAfterTime(GameObject other)
    {
        transform.GetComponent<Animator>().SetTrigger("ActivateBoundary");

        GameObject newLightning = ObjectPoolingManager.Instance.GetObject(lightningEffectName);
        newLightning.transform.position = other.transform.position;
        newLightning.transform.rotation = other.transform.rotation;

        other.GetComponent<DestroyByContact>().PlaySfxElectricityBoundary();

        yield return new WaitForSeconds(1f);
            
        other.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);
        newLightning.SetActive(false);


    }
}
