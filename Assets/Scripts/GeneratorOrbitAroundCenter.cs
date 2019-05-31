using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorOrbitAroundCenter : MonoBehaviour {

    public GameObject centerOfRotation; //The object we will rotate around
    public float avgDistanceHeadFloorWhenSitting = 1.2f;
    public float speedRight = 5;
    public float speedLeft = -5;
    public float speedVerticalSmooth = 0.1f;
    private float defaultZPosGeneratorRotating;
    private void Start()
    {
        defaultZPosGeneratorRotating = transform.localPosition.z;
    }

  

    public IEnumerator OrbitRightUntilPosDegrees(float positiveDegreesRight)
    {
        float angle;
        do
        {
            angle = transform.localEulerAngles.y;
            angle = (angle > 180) ? angle - 360 : angle;
            transform.RotateAround(centerOfRotation.transform.position, Vector3.up, speedRight * Time.deltaTime);
            yield return null;
            Debug.Log(angle);

        } while (angle < positiveDegreesRight);
        
    }

    public IEnumerator OrbitLeftUntilNegDegrees(float negativeDegreesLeft)
    {
        float angle;
        do
        {
            angle = transform.localEulerAngles.y;
            angle = (angle > 180) ? angle - 360 : angle;
            transform.RotateAround(centerOfRotation.transform.position, Vector3.up, speedLeft * Time.deltaTime);
            yield return null;
            Debug.Log(angle);
        } while (angle > negativeDegreesLeft);

    }


    public IEnumerator OrbitUntilCenter()
    {
        float angle = transform.localEulerAngles.y;
        angle = (angle > 180) ? angle - 360 : angle;
        if (angle > 0)
        {
            do
            {
                angle = transform.localEulerAngles.y;
                angle = (angle > 180) ? angle - 360 : angle;
                transform.RotateAround(centerOfRotation.transform.position, Vector3.up, speedLeft * Time.deltaTime);
                yield return null;
                Debug.Log(angle);
            } while (angle > 0);
        }
        else if (angle < 0)
        {
            do
            {
                angle = transform.localEulerAngles.y;
                angle = (angle > 180) ? angle - 360 : angle;
                transform.RotateAround(centerOfRotation.transform.position, Vector3.up, speedRight * Time.deltaTime);
                yield return null;
                Debug.Log(angle);
            } while (angle < 0);
        }
       
    }


    public IEnumerator GoSitDown()
    {
        
        float targetAboveFloor = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor)[0].transform.position.y + avgDistanceHeadFloorWhenSitting;
        Vector3 target = new Vector3(transform.position.x, targetAboveFloor, transform.position.z);

        while (transform.position.y != targetAboveFloor)
        {
            float step = Time.deltaTime * speedVerticalSmooth;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            yield return null;
        }
        
    }

    public IEnumerator ReturnUpToParentCenter()
    {       
        Vector3 target = new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);

        while(transform.localPosition.y != 0f)
        {
            float step = Time.deltaTime * speedVerticalSmooth;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, step);
            yield return null;
        }
        
    }

    public void ResetPositionGeneratorRotating()
    {
        transform.localPosition = new Vector3(0f, 0f, defaultZPosGeneratorRotating);
        transform.localEulerAngles = new Vector3(-90, 0f, 0f);
    }
}
