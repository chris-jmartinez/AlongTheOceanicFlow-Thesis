
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class UnderworldBaseEgg : Singleton<UnderworldBaseEgg>
{
    public List<GameObject> ObjectsToHide = new List<GameObject>();
    public bool timerActive;
    public float timerUnderworld;
    public float rayDistance = 10f;


    // Called whenever the underworld is enabled.
    public void OnEnableUnderworld()
    {
        if (!gameObject.activeSelf)
        {
            // Place the underworld on the surface mesh.
            PlaceUnderworld();
        }
    }

    // Called whenever the underworld is disabled.
    public void OnDisableUnderworld()
    {
        ResetUnderworld();
    }

    /// <summary>
    /// Places the underworld at the user's gaze and makes it visible.
    /// </summary>
    private void PlaceUnderworld()
    {
        RaycastHit hitInfo;

        bool hit = Physics.Raycast(Camera.main.transform.position,
                                Camera.main.transform.forward,
                                out hitInfo,
                                rayDistance,
                                LayerMask.GetMask("Secret")); //SpatialMappingManager.Instance.LayerMask

        if (hit)
        {
            // Disable the objects that should be hidden when the underworld is displayed.
            foreach (GameObject go in ObjectsToHide)
            {
                go.SetActive(false);
            }

            //Start ClassyWhaleAnimation
            GameObject magicClassyWhale = GameObject.FindGameObjectWithTag("ClassyWhale");
            if (magicClassyWhale != null && magicClassyWhale.GetComponent<Animator>() != null)
            {
                //magicClassyWhale.GetComponent<Animator>().SetBool("ShowUnderworld", true);
                magicClassyWhale.GetComponent<SpriteRenderer>().enabled = false;
            }
            

            // Place and enable the underworld.
            gameObject.transform.position = hitInfo.point;
            //gameObject.transform.up = hitInfo.normal; //Questo comando piazzava l'oggetto correttamente sulla superficie, però ogni tanto l'underworld era ruotato in senso orario di tot gradi
            gameObject.transform.rotation = magicClassyWhale.transform.GetChild(1).transform.rotation; //Con questo statement praticamente dico di settare la rotazione locale come quella (locale) di un oggetto vuoto già pronto che ho agganciato alla balena
            

            gameObject.SetActive(true);

            // Turn off spatial mapping meshes.
            //SpatialMappingManager.Instance.gameObject.SetActive(false);

            //Turn off the mesh renderer of the walls
            List<GameObject> walls = new List<GameObject>();
            walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
            foreach (GameObject wall in walls)
                wall.GetComponent<MeshRenderer>().enabled = false;

            if (timerActive)
            {
                Invoke("ResetUnderworld", timerUnderworld);
            }
        }

        
        
    }


    /// <summary>
    /// Hides the underworld.
    /// </summary>
    private void ResetUnderworld()
    {

        //Start ClassyWhaleAnimation
        GameObject magicClassyWhale = GameObject.FindGameObjectWithTag("ClassyWhale");
        if (magicClassyWhale != null && magicClassyWhale.GetComponent<Animator>() != null)
        {
            //magicClassyWhale.GetComponent<Animator>().SetBool("ShowUnderworld", false);
            magicClassyWhale.GetComponent<SpriteRenderer>().enabled = true;
        }
        

        // Unhide the previously hidden objects.
        foreach (GameObject go in ObjectsToHide)
        {
            go.SetActive(true);
        }

        // Disable the underworld.
        gameObject.SetActive(false);

        // Turn spatial mapping meshes back on.
        //SpatialMappingManager.Instance.gameObject.SetActive(true);

        //Turn on the mesh renderer of the walls
        List<GameObject> walls = new List<GameObject>();
        walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
        foreach (GameObject wall in walls)
            wall.GetComponent<MeshRenderer>().enabled = true;
    }

    /// <summary>
    /// Checks to see if the target's mesh is visible within the Main Camera's view frustum.
    /// </summary>
    /// <returns>True, if the target's mesh is visible.</returns>
    bool IsTargetVisible()
    {
        Vector3 targetViewportPosition = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        return (targetViewportPosition.x > 0.0 && targetViewportPosition.x < 1 &&
            targetViewportPosition.y > 0.0 && targetViewportPosition.y < 1 &&
            targetViewportPosition.z > 0);
    }
}
