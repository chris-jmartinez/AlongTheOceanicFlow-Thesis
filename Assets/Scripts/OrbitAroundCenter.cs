using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitAroundCenter : MonoBehaviour {

    public GameObject centerOfRotation; //The object we will rotate around
    public float speed;
   
    // Update is called once per frame
    void Update () {
        OrbitAround();
	}


    private void OrbitAround()
    {
        transform.RotateAround(centerOfRotation.transform.position, Vector3.up, speed * Time.deltaTime);
    }


   
}
