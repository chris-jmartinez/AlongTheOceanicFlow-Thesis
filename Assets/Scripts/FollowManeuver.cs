
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowManeuver : MonoBehaviour {

    private Rigidbody rb;
    public Transform playerTransform;

    public Vector2 startWait; 
    public Vector2 maneuverTime;
    public Vector2 maneuverWait;
    
    //private float targetManeuverY;
    private float targetManeuverX;

    private float currentSpeedZ;
    public float smoothSpeed;

    private float automaticManeuverX;

    public GameObject boundaryPlayfield;

    public bool followPlayerOrAutomatic;
    
    // Use this for initialization
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boundaryPlayfield = GameObject.FindGameObjectWithTag("BoundaryPlayfield");
        playerTransform = Camera.main.transform;
    }

    //If we place here startCoroutine(Follow()) it will not be done the next times the objects is activated again,
    //because start function is run only 1 time in the object's lifetime
   

    private void OnEnable() //Everytime the object is activated again, there will be no coroutines running (they were killed), so we start a new coroutine
    {
        //Deactivate the bool "followPlayerOrAutomatic" in the inspector to move automatically the shark. Deactivate the script to move the shark straight.
        if (followPlayerOrAutomatic)
            StartCoroutine(FollowPlayer());
        else
            StartCoroutine(MoveAutomatically()); 
    }

    private void OnDisable()
    {
        rb.velocity = new Vector3(0f, 0f, 0f);
    }


    private IEnumerator FollowPlayer() //This coroutine is killed everytime the objects is deactivated (when the shark touches the boundary of the playfield and it's deactivated)
    {       
        targetManeuverX = 0f;
        //targetManeuverY = 0f;
        yield return new WaitForSeconds(Random.Range(startWait.x, startWait.y));

        while (gameObject.activeInHierarchy)
        {
            //targetManeuverX = playerTransform.position.x;
            //targetManeuverY = playerTransform.position.y; //I'm calculating also y but i don't really use it (it was just for testing)
            //targetManeuverX = -(transform.position.x - playerTransform.position.x);
            Vector3 positionPlayerWrtShark = transform.InverseTransformPoint(Camera.main.transform.position);
            targetManeuverX = positionPlayerWrtShark.x;

            yield return new WaitForSeconds(Random.Range(maneuverTime.x, maneuverTime.y));

            targetManeuverX = 0f;
            //targetManeuverY = 0f;
            yield return new WaitForSeconds(Random.Range(maneuverWait.x, maneuverWait.y));
            
        }
        
    }


    private IEnumerator MoveAutomatically() //This coroutine is killed everytime the objects is deactivated (when the shark touches the boundary of the playfield and it's deactivated)
    {
        automaticManeuverX = 0f; //In this case instead of seeking the player I could move the shark stupidly a bit to the left and a bit to the right, if following the player doesn't work well
        yield return new WaitForSeconds(Random.Range(startWait.x, startWait.y));


        bool randomStartingDirection = System.Convert.ToBoolean(Random.Range(0, 2));

        if (randomStartingDirection)
            automaticManeuverX = 0.1f;
        else
            automaticManeuverX = -0.1f;


        yield return new WaitForSeconds(1.5f);

        while (gameObject.activeInHierarchy)
        {
            if (randomStartingDirection)
                automaticManeuverX = -0.1f;
            else
                automaticManeuverX = 0.1f;
            yield return new WaitForSeconds(3f);


            if (randomStartingDirection)
                automaticManeuverX = 0.1f;
            else
                automaticManeuverX = -0.1f;
            yield return new WaitForSeconds(3f);

        }

    }

    
    void FixedUpdate()
    {

        Vector3 localRbVelocity = transform.InverseTransformDirection(rb.velocity);
        float newManeuverX = Mathf.MoveTowards(localRbVelocity.x, targetManeuverX, Time.deltaTime * smoothSpeed);
        //float newManeuverX = Mathf.MoveTowards(rb.velocity.x, targetManeuverX, Time.deltaTime * smoothSpeed);

        localRbVelocity.x = newManeuverX;
        

        //If you want the shark to go straight, disable the script
        if (followPlayerOrAutomatic)
            rb.velocity = transform.TransformDirection(localRbVelocity);//rb.AddRelativeForce(transform.right * newManeuverX); //FOLLOW PLAYER: use this line to follow the player
        else
        {
            rb.velocity = new Vector3(automaticManeuverX, 0.0f, currentSpeedZ); //AUTOMATIC MANEUVER: use this line to move the shark automatically
            currentSpeedZ = rb.velocity.z;
        }
            

    }
}


