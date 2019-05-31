using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowVerticallyTheCamera : MonoBehaviour {

    public bool FollowingEnabled { get; set; }
    public float smoothSpeed;
    public float offsetItemFromCameraY;

	// Use this for initialization
	void Start () {
        FollowingEnabled = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (FollowingEnabled)
        {
            float yCameraTarget = Camera.main.transform.position.y;

            Vector3 target = new Vector3(transform.position.x, yCameraTarget + offsetItemFromCameraY, transform.position.z);

            float step = Time.deltaTime * smoothSpeed;
            
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
		
	}
}
