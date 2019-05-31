using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.SpatialMapping;

public class ChestEvent : MonoBehaviour
{
    public string correctPassword = "123";
    public GameObject passwordHint;

    public GameObject chestWooden;
    public GameObject passwordPanel;
    public GameObject keyPanel;
    public ParticleSystem chestLight;
    public ParticleSystem chestBubbles;
    public GameObject treasureMap;
    public AudioClip unlockChestSound;
    public AudioClip goldFoundSound;
    public AudioClip mapFoundSound;
    private bool includeChildrenParticle = true;

    private string myTempPassword = "";
    private List<GameObject> mySelectedButtons = new List<GameObject>();

    public bool key1found, key2found = false;

    private bool enteredTrigger = false;
    private bool chestOpened = false;



    void Start()
    {
        
        if (GameController.instance.currentLevel == 3)
        {
            //string randomNum = Random.Range(0, 10000).ToString("D4"); //This was generating a number also with zeroes, but in out password panel we didn't put a 0 digit because with that digit the panel was ugly
            string randomNum = Random.Range(1, 10).ToString() + Random.Range(1, 10).ToString() + Random.Range(1, 10).ToString() + Random.Range(1, 10).ToString();
            correctPassword = randomNum;
            passwordHint.GetComponentInChildren<TextMesh>().text = correctPassword;
            StartCoroutine(AttachPswToRandomPainting());
        }

    }

    public IEnumerator AttachPswToRandomPainting()
    {
        yield return new WaitForSeconds(2f);
        GameObject[] paintingsInRoom = GameObject.FindGameObjectsWithTag("Painting");
        if (paintingsInRoom.Length != 0)
        {
            GameObject extractedRandomPainting = paintingsInRoom[Random.Range(0, paintingsInRoom.Length)];
            GameObject newPasswordHint = Instantiate(passwordHint, 
                                                    extractedRandomPainting.transform.GetChild(0).transform.position, 
                                                    extractedRandomPainting.transform.rotation);
            DecorationsSpawnerManager.Instance.AddToSpawnedWallsObjects(newPasswordHint);
        }
        else //If unfortunately there are no paintings (rare case), we put the password at the center of the first wall we find
        {
            List<GameObject> walls = new List<GameObject>();
            walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
            GameObject firstWall = walls[0];
            SurfacePlane plane = firstWall.GetComponent<SurfacePlane>();
            Vector3 positionCenterFirstWall = firstWall.GetComponent<Collider>().bounds.center + (plane.PlaneThickness * 1.01f * plane.SurfaceNormal);
            Quaternion rotationFirstWall = Quaternion.LookRotation(firstWall.transform.forward, Vector3.up);
            GameObject newPasswordHint = Instantiate(passwordHint,
                                                    positionCenterFirstWall,
                                                    rotationFirstWall);
            DecorationsSpawnerManager.Instance.AddToSpawnedWallsObjects(newPasswordHint);
        }
    }

    public void OpenPasswordUI()
    {
        //passwordPanel.SetActive(true);
        if (passwordPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PasswordPanelIdle"))
            passwordPanel.GetComponent<Animator>().SetBool("Appear", true);
    }

    public void ClosePasswordUI()
    {
        if (passwordPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PasswordPanelAppears"))
            passwordPanel.GetComponent<Animator>().SetBool("Appear", false);
        ResetNumberOfUIPassword();
        //Invoke("DeactivatePasswordPanel", 1f);
    }

    public void OpenKeyUI()
    {
        //keyPanel.SetActive(true);
        if (keyPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PasswordPanelIdle"))
            keyPanel.GetComponent<Animator>().SetBool("Appear", true);

        switch (GameController.instance.currentLevel)
        {
            case 1:
                keyPanel.transform.GetComponentInChildren<TextMesh>().text = "CERCA LA CHIAVE";
                break;
            case 2:
                keyPanel.transform.GetComponentInChildren<TextMesh>().text = "CERCA LE 2 CHIAVI";
                break;
        }
           
            
    }

    public void CloseKeyUI()
    {
        if (keyPanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PasswordPanelAppears"))
            keyPanel.GetComponent<Animator>().SetBool("Appear", false);
        //Invoke("DeactivateKeyPanel", 1f);
    }

    //private void DeactivatePasswordPanel()
    //{
    //    passwordPanel.SetActive(false);
    //}

    //private void DeactivateKeyPanel()
    //{
    //    keyPanel.SetActive(false);
    //}

    public void SetNumberFromUIPassword(GameObject selectedUINumber)
    {
        mySelectedButtons.Add(selectedUINumber);

        //Change it's button color status.
        selectedUINumber.transform.GetChild(2).gameObject.SetActive(true);

        //Text is the child
        string number = selectedUINumber.GetComponentInChildren<TextMesh>().text;
        myTempPassword += number;

        Debug.Log("My Password is: " + myTempPassword);

        if (myTempPassword.Length == correctPassword.Length)
        {
            if (CheckMyPassword())
            {
                UnlockTheChest();
            }
            else
            {
                ResetNumberOfUIPassword();
            }
        }
    }

    private bool CheckMyPassword()
    {
        if (myTempPassword.Equals(correctPassword))
            return true;
        else
            return false;
    }

    public void ResetNumberOfUIPassword()
    {
        Debug.Log("Reset UI Password");

        myTempPassword = "";

        foreach (GameObject g in mySelectedButtons)
            g.transform.GetChild(2).gameObject.SetActive(false); //turn off color on selected buttons

        mySelectedButtons.Clear();
    }

    private void UnlockTheChest()
    {
        Debug.Log("Unlock the chest!");
        WebClientSender.instance.SetLedGreen();
        //Deactivate colliders in order to better select the treasure map
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
        foreach (BoxCollider collider in passwordPanel.GetComponentsInChildren<BoxCollider>())
            collider.enabled = false;
        keyPanel.GetComponentInChildren<BoxCollider>().enabled = false;

        ClosePasswordUI();

        chestWooden.GetComponent<Animator>().SetTrigger("Opened");

        chestOpened = true;

        chestWooden.layer = 10; //Set the layer of the chestwooden to "Ignore gaze" (Ignore gaze is at layer number 10) so the player can gaze to the objects inside the chest

        chestLight.Play(includeChildrenParticle);
        chestBubbles.Play(includeChildrenParticle);
        treasureMap.GetComponent<Animator>().SetTrigger("Opened");

        if (DataController.instance.audioEffectsIsActive)
            PlaySound(unlockChestSound);
    }

    private void PlaySound(AudioClip clipToPlay)
    {
        GetComponent<AudioSource>().PlayOneShot(clipToPlay);
    }

    public void MapFound()
    {
        Debug.Log("Map selected, end treasure initiated");
        if (DataController.instance.audioEffectsIsActive)
            PlaySound(mapFoundSound);
        StartCoroutine(GameController.instance.TreasureHuntFinished());
    }

    public void GoldFound()
    {
        if (DataController.instance.audioEffectsIsActive)
            PlaySound(goldFoundSound);
        GameController.instance.AddScore(300);
        //DISPLAY IN A MESSAGE THAT WE GOT 300+ SCORE
    }

    public void ItemFound(GameObject itemFound)
    {
        switch (itemFound.name)
        {
            case "Key1(Clone)":
                key1found = true;
                break;
            case "Key2(Clone)":
                key2found = true;
                break; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger box");

        switch (GameController.instance.currentLevel)
        {
            case 1:
                if (key1found && !chestOpened && !enteredTrigger)
                {
                    enteredTrigger = true;
                    UnlockTheChest();
                }
                else if(!key1found && !chestOpened && !enteredTrigger)
                {
                    enteredTrigger = true;
                    OpenKeyUI();
                }
                break;
            case 2:
                if (key1found && key2found && !chestOpened && !enteredTrigger)
                {
                    enteredTrigger = true;
                    UnlockTheChest();
                }
                else if((!key1found || !key2found) && !chestOpened && !enteredTrigger)
                {
                    enteredTrigger = true;
                    OpenKeyUI();
                }
                break;
            case 3:
                if (!chestOpened && !enteredTrigger && (other.tag == "Player" || other.tag == "CameraCollider"))
                {
                    //Debug.Log("Opened panel");
                    enteredTrigger = true;
                    OpenPasswordUI();
                }
                break;
        }

        
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameController.instance.currentLevel == 3 && !chestOpened && enteredTrigger && (other.tag == "Player" || other.tag == "CameraCollider"))
        {
            //Debug.Log("ClosedPanel");
            enteredTrigger = false;
            ClosePasswordUI();
        }

        if ( (GameController.instance.currentLevel == 1 || GameController.instance.currentLevel == 2) && !chestOpened && enteredTrigger && (other.tag == "Player" || other.tag == "CameraCollider"))
        {
            enteredTrigger = false;
            CloseKeyUI();
        }
    }
}
