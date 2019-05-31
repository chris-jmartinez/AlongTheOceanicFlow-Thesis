using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

/// <summary>
/// Called by PlaySpaceManager (SpatialProcessingTest nel mio gioco) after planes have been generated from the Spatial Mapping Mesh.
/// This class will create a collection of prefab objects that have the 'Placeable' component and
/// will attempt to set their initial location on planes that are close to the user.
/// </summary>
public class DecorationsSpawnerManager : Singleton<DecorationsSpawnerManager>
{
    [Tooltip("A collection of Placeable object prefabs to generate in the world. (Both vertical or horizontal). Not used now.")]
    public List<GameObject> initialWorldObjectPrefabs;
    public GameObject playfield;

    public GameObject water;
    public GameObject waterCamera;
    public GameObject seagullsGroup;
    public List<GameObject> floorDecorationObjectPrefabs;
    public List<GameObject> gifWallsDecorationObjectPrefabs;
    public List<GameObject> paintingsWallsDecorationObjectPrefabs;

    [Tooltip("Mask to consider to avoid overlapping when spawning floor objects")]
    public LayerMask maskToConsiderOverlappingObjects;

    private List<GameObject> spawnedFloorObjects;
    private List<GameObject> spawnedWallsObjects;

    //Places horizontal objects near the user; places vertical objects at the center of each wall.
    //Used in ATOF to place gifs / paintings on the walls, and to place the Playfield near the user
    /// <summary>
    /// Generates a collection of Placeable objects in the world and sets them on planes that match their affinity.
    /// </summary>
    /// <param name="horizontalSurfaces">Horizontal surface planes (floors, tables).</param>
    /// <param name="verticalSurfaces">Vertical surface planes (walls).</param>
    public void GenerateInitialItemsInWorld(List<GameObject> horizontalSurfaces, List<GameObject> verticalSurfaces)
    {
        List<GameObject> horizontalObjects = new List<GameObject>();
        List<GameObject> verticalObjects = new List<GameObject>();

        spawnedFloorObjects = new List<GameObject>();
        spawnedWallsObjects = new List<GameObject>();

        foreach (GameObject gameItemPrefab in initialWorldObjectPrefabs)
        {
            Placeable placeable = gameItemPrefab.GetComponent<Placeable>();
            if (placeable.PlacementSurface == PlacementSurfaces.Horizontal)
            {
                horizontalObjects.Add(gameItemPrefab);
            }
            else
            {
                verticalObjects.Add(gameItemPrefab);
            }
        }

        if (horizontalObjects.Count > 0)
        {
            CreateGameObjects(horizontalObjects, horizontalSurfaces, PlacementSurfaces.Horizontal);
        }

        if (verticalObjects.Count > 0)
        {
            CreateGameObjects(verticalObjects, verticalSurfaces, PlacementSurfaces.Vertical);
        }
    }

    //Used in atof to spawn and place objects on the walls and playfield on the floor.
    /// <summary>
    /// Creates and positions a collection of Placeable game objects on SurfacePlanes in the environment.
    /// </summary>
    /// <param name="gameObjects">Collection of prefab GameObjects that have the Placeable component.</param>
    /// <param name="surfaces">Collection of SurfacePlane objects in the world.</param>
    /// <param name="surfaceType">Type of objects and planes that we are trying to match-up.</param>
    private void CreateGameObjects(List<GameObject> gameObjects, List<GameObject> surfaces, PlacementSurfaces surfaceType)
    {
        List<int> UsedPlanes = new List<int>();

        // Sort the planes by distance to user.
        surfaces.Sort((lhs, rhs) =>
       {
           Vector3 headPosition = Camera.main.transform.position;
           Collider rightCollider = rhs.GetComponent<Collider>();
           Collider leftCollider = lhs.GetComponent<Collider>();

           // This plane is big enough, now we will evaluate how far the plane is from the user's head.  
           // Since planes can be quite large, we should find the closest point on the plane's bounds to the 
           // user's head, rather than just taking the plane's center position.
           Vector3 rightSpot = rightCollider.ClosestPointOnBounds(headPosition);
           Vector3 leftSpot = leftCollider.ClosestPointOnBounds(headPosition);

           return Vector3.Distance(leftSpot, headPosition).CompareTo(Vector3.Distance(rightSpot, headPosition));
       });

        foreach (GameObject item in gameObjects)
        {
            int index = -1;
            Collider collider = item.GetComponent<Collider>();

            if (surfaceType == PlacementSurfaces.Vertical)
            {
                index = FindNearestPlane(surfaces, collider.bounds.size, UsedPlanes, true);
            }
            else
            {
                index = FindNearestPlane(surfaces, collider.bounds.size, UsedPlanes, false);
            }

            // If we can't find a good plane could put the object floating in space. (then we can instantiate it or not, see ending of the code)
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 2.0f + Camera.main.transform.right * (Random.value - 1.0f) * 2.0f;
            Quaternion rotation = Quaternion.identity;

            // If we do find a good plane we can do something smarter.
            if (index >= 0)
            {
                UsedPlanes.Add(index); //We spawn only one object for each plane (for example, for each wall), in the nearest point of it wrt the player
                GameObject surface = surfaces[index];
                SurfacePlane plane = surface.GetComponent<SurfacePlane>();
                position = surface.transform.position + (plane.PlaneThickness * plane.SurfaceNormal);
                position = AdjustPositionWithSpatialMap(position, plane.SurfaceNormal);
                rotation = Camera.main.transform.localRotation;

                if (surfaceType == PlacementSurfaces.Vertical)
                {
                    // Vertical objects should face out from the wall.
                    rotation = Quaternion.LookRotation(surface.transform.forward, Vector3.up);
                }
                else
                {
                    // Horizontal objects should face the user.
                    rotation = Quaternion.LookRotation(Camera.main.transform.position);
                    rotation.x = 0f;
                    rotation.z = 0f;
                }
            }

            //Vector3 finalPosition = AdjustPositionWithSpatialMap(position, surfaceType);

            //If it's the Playfield, we don't instantiate any prefab (the game object is already in hierarchy), we position it .
            if (item.CompareTag("Playfield"))
            {
                item.transform.position = position;

                //I case I would want to spawn directly the playfield exactly where the player is (without considering anything else), slightly different from previous method
                //float yfloorPosition = position.y;
                //item.transform.position = new Vector3(Camera.main.transform.position.x, yfloorPosition, Camera.main.transform.position.z);

                
                List<GameObject> tables = new List<GameObject>();
                tables = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table);

                //Deactivating tables (useful just to avoid Playfield is placed inside them) to not see them anymore
                foreach (GameObject table in tables)
                {
                    table.layer = 10; //We set the layer to IGNORE GAZE, in this way even if there are unexpected problems (colliders of tables detected by gaze) we ignore them setting this layer
                    table.SetActive(false);
                }
                    

                //item.transform.rotation = rotation;

                Vector3 relativeDistance = position - Camera.main.transform.position;
                Quaternion playfieldNewRotation = Quaternion.LookRotation(relativeDistance);
                playfieldNewRotation.x = 0f;
                playfieldNewRotation.z = 0f;
                item.transform.rotation = playfieldNewRotation;

                
                if (item.activeInHierarchy == false)
                {
                    item.SetActive(true);
                }

            } //Here we decide if we want to instantiate the object only if we found a good plane (case index>=0) or anyway (in this last case you should remove the "else if" and leave just "Else")
            else if (index >= 0) 
            {
                //Standard procedure to instantiate the prefabs
                GameObject gameItemObject = Instantiate(item, position, rotation) as GameObject;
                gameItemObject.transform.parent = gameObject.transform;

                if (gameItemObject.GetComponent<Placeable>().PlacementSurface == PlacementSurfaces.Vertical)
                    spawnedWallsObjects.Add(gameItemObject);
                else if (gameItemObject.GetComponent<Placeable>().PlacementSurface == PlacementSurfaces.Horizontal)
                    spawnedFloorObjects.Add(gameItemObject);
            }
            else if (index == -1)
            {
                //Due to the fact these are just decoration objects, if we can't find a plane to fit the objects, we don't instantiate them
                //Debug.Log("The object/decoration " + item.name + " could not be instantiated because there's no good plane that fits it (or they all have been already used to instantiate other objects)");
            }

        }
    }    

    /// <summary>
    /// Attempts to find a the closest plane to the user which is large enough to fit the object.
    /// </summary>
    /// <param name="planes">List of planes to consider for object placement.</param>
    /// <param name="minSize">Minimum size that the plane is required to be.</param>
    /// <param name="startIndex">Index in the planes collection that we want to start at (to help avoid double-placement of objects).</param>
    /// <param name="isVertical">True, if we are currently evaluating vertical surfaces.</param>
    /// <returns></returns>
    private int FindNearestPlane(List<GameObject> planes, Vector3 minSize, List<int> usedPlanes, bool isVertical)
    {
        int planeIndex = -1;
       
        for(int i = 0; i < planes.Count; i++)
        {
            if (usedPlanes.Contains(i))
            {
                continue;
            }

            Collider collider = planes[i].GetComponent<Collider>();
            if (isVertical && (collider.bounds.size.x < minSize.x || collider.bounds.size.y < minSize.y))
            {
                // This plane is too small to fit our vertical object.
                continue;
            }
            else if(!isVertical && (collider.bounds.size.x < minSize.x || collider.bounds.size.y < minSize.y))
            {
                // This plane is too small to fit our horizontal object.
                continue;
            }

            return i;
        }

        return planeIndex;
    }

    /// <summary>
    /// Adjusts the initial position of the object if it is being occluded by the spatial map.
    /// </summary>
    /// <param name="position">Position of object to adjust.</param>
    /// <param name="surfaceNormal">Normal of surface that the object is positioned against.</param>
    /// <returns></returns>
    private Vector3 AdjustPositionWithSpatialMap(Vector3 position, Vector3 surfaceNormal)
    {
        Vector3 newPosition = position;
        RaycastHit hitInfo;
        float distance = 0.5f;

        // Check to see if there is a SpatialMapping mesh occluding the object at its current position.
        if(Physics.Raycast(position, surfaceNormal, out hitInfo, distance, SpatialMappingManager.Instance.LayerMask))
        {
            // If the object is occluded, reset its position.
            newPosition = hitInfo.point;
        }

        return newPosition;
    }


    public void SpawnWallsDecorationObjects(List<GameObject> objectsToSpawn, bool randomically)
    {
        if (randomically)
            Utilities.Shuffle(objectsToSpawn);

        List<GameObject> walls = new List<GameObject>();
        walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);

        CreateGameObjects(objectsToSpawn, walls, PlacementSurfaces.Vertical);
    }

    public void AddToSpawnedWallsObjects(GameObject objToAdd)
    {
        spawnedWallsObjects.Add(objToAdd);
    }

    public void AddToSpawnedFloorObjects(GameObject objToAdd)
    {
        spawnedFloorObjects.Add(objToAdd);
    }



    //Spawns non-overlapping floor objects randomly. If the randomically bool is activated it spawns them in a randomically order)
    public IEnumerator SpawnFloorDecorationObjects(List<GameObject> objectsToSpawn, bool randomically)
    {
        if (randomically)
            Utilities.Shuffle(objectsToSpawn);

        Vector3 spawnPos = new Vector3(0, 0, 0);
        int spawnAttempts = 0;

        List<GameObject> floors = new List<GameObject>();
        floors = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor);


        //To avoid overlapping with walls. But this reduces the spawned objects very much
        //List<GameObject> walls = new List<GameObject>();
        //walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
        //foreach (GameObject wall in walls)
        //{
        //    wall.layer = 8; //I'm temporarily changing the layer mask of the walls in order to avoid overlapping between spawned objects and walls
        //}


        for (int i = 0; i < objectsToSpawn.Count; i++)
        {
            bool canSpawnHere = false;

            while (!canSpawnHere)
            {
                GameObject selectedFloor = floors[Random.Range(0, floors.Count)]; //If I have more than one floor, I select a random one to spawn each object

                float halfWidthFloorXLocal = selectedFloor.transform.localScale.x / 2;
                float halfWidthFloorYLocal = selectedFloor.transform.localScale.y / 2;
                float leftExtent = -halfWidthFloorXLocal;
                float rightExtent = +halfWidthFloorXLocal;
                float lowerExtent = -halfWidthFloorYLocal;
                float upperExtent = +halfWidthFloorYLocal;

                //Vector3 localCenterPointFloor = selectedFloor.transform.InverseTransformPoint(selectedFloor.transform.position);  //This results always in 0,0,0 of course
                float spawnPosXLocal = Random.Range(leftExtent + 0.1f, rightExtent - 0.1f);
                float spawnPosYLocal = Random.Range(lowerExtent + 0.1f, upperExtent - 0.1f);
                
                Vector3 spawnPosLocalNoScaling = new Vector3(spawnPosXLocal / selectedFloor.transform.localScale.x, spawnPosYLocal / selectedFloor.transform.localScale.y, 0.05f); //Corretto
                Vector3 spawnPosInWorldSpace = selectedFloor.transform.TransformPoint(spawnPosLocalNoScaling); 
                Vector3 spawnPosInWorldSpaceCorrected = new Vector3(spawnPosInWorldSpace.x, selectedFloor.transform.position.y + 0.05f, spawnPosInWorldSpace.z);
                spawnPos = spawnPosInWorldSpaceCorrected;

                canSpawnHere = PreventSpawnOverlap(spawnPos, objectsToSpawn[i]);

                if (canSpawnHere)
                {
                  
                    GameObject newDecoration = Instantiate(objectsToSpawn[i], spawnPos, Quaternion.identity) as GameObject;
                    
                    Vector3 targetToLook = new Vector3(Camera.main.transform.position.x, spawnPos.y, Camera.main.transform.position.z);
                    newDecoration.transform.LookAt(targetToLook);

                    newDecoration.transform.parent = gameObject.transform;

                    spawnedFloorObjects.Add(newDecoration);
                    break;
                }

                spawnAttempts++;

                if (spawnAttempts > 70)
                {
                    if (objectsToSpawn[i].name.Contains("Wooden Chest") || objectsToSpawn[i].name.Contains("Key"))
                    {
                        Vector3 spawnPosInFront = (objectsToSpawn[i].name.Contains("Wooden Chest"))? Camera.main.transform.TransformPoint(Vector3.forward * 2) : Camera.main.transform.position;
                        spawnPosInFront = new Vector3(spawnPosInFront.x, selectedFloor.transform.position.y + 0.05f, spawnPosInFront.z);
                        GameObject newDecoration = Instantiate(objectsToSpawn[i], spawnPosInFront, Quaternion.identity) as GameObject;
                        Vector3 targetToLook = new Vector3(Camera.main.transform.position.x, selectedFloor.transform.position.y + 0.05f, Camera.main.transform.position.z);
                        newDecoration.transform.LookAt(targetToLook);
                        newDecoration.transform.parent = gameObject.transform;

                        spawnedFloorObjects.Add(newDecoration);
                    }
                    
                    break;
                }


                yield return null;

            }


        }

        //To avoid overlapping with walls. But this reduces the spawned objects very much
        //walls = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
        //foreach (GameObject wall in walls)
        //{
        //    wall.layer = 31; //Then I reset the value of the layer to the 31 (SpatialMapping)
        //}

    }


    private bool PreventSpawnOverlap(Vector3 spawnPosition, GameObject objectToSpawn)
    {
        Collider[] collidersTouched;
        Collider colliderObjToSpawn = objectToSpawn.GetComponent<Collider>();

        float radiusObjToSpawn = (colliderObjToSpawn.bounds.extents.x > colliderObjToSpawn.bounds.extents.z) ?
                                  colliderObjToSpawn.bounds.extents.x : colliderObjToSpawn.bounds.extents.z;

        

        collidersTouched = Physics.OverlapSphere(spawnPosition, 1 * 13, maskToConsiderOverlappingObjects) as Collider[];

        for (int j = 0; j < collidersTouched.Length; j++)
        {
            Vector3 centerPointObj = collidersTouched[j].bounds.center;
            float halfWidthObjX = collidersTouched[j].bounds.extents.x;
            float halfLengthObjZ = collidersTouched[j].bounds.extents.z;

            float leftExtent = centerPointObj.x - halfWidthObjX;
            float rightExtent = centerPointObj.x + halfWidthObjX;
            float lowerExtent = centerPointObj.z - halfLengthObjZ;
            float upperExtent = centerPointObj.z + halfLengthObjZ;

            if ((spawnPosition.x + radiusObjToSpawn + 0.5) >= leftExtent && (spawnPosition.x - radiusObjToSpawn - 0.5) <= rightExtent)
            {
                if ((spawnPosition.z + radiusObjToSpawn + 0.5) >= lowerExtent && (spawnPosition.z - radiusObjToSpawn - 0.5) <= upperExtent)
                {
                    return false; //In this case the spawnPosition is inside another object and we won't generate in this position, we want to avoid overlappings.
                }
            }

        }
        return true;
    }

    public void AddTablesToFloorObjectsLayerMask(bool activate)
    {
        List<GameObject> tables = new List<GameObject>();
        tables = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table);

        if (tables.Count <= 0)
            return;

        if (activate)
        {
            foreach (GameObject table in tables)
            {
                table.layer = 8; //I'm temporarily changing the layer mask of the walls in order to avoid overlapping between spawned objects and walls
            }
        }
        else if (!activate)
        {
            foreach (GameObject table in tables)
            {
                //Layer 31: spatial mapping. Layer 10: ignore gaze. Layer 8: floor objects
                table.layer = 31; //I'm temporarily changing the layer mask of the walls in order to avoid overlapping between spawned objects and walls
            }
        }
    }

    public void CleanFloorObjects()
    {
        if (spawnedFloorObjects.Count != 0)
        {
            foreach (GameObject obj in spawnedFloorObjects)
            {
                Destroy(obj);
            }
            spawnedFloorObjects.Clear();
        }
    }

    public void CleanWallsObjects()
    {
        if (spawnedWallsObjects.Count != 0)
        {
            foreach (GameObject obj in spawnedWallsObjects)
            {
                Destroy(obj);
            }
            spawnedWallsObjects.Clear();
        }
    }

    public void DecorationAtStart()
    {
        CleanFloorObjects();
        CleanWallsObjects();
        StartCoroutine(WaterAnimation());
        SpawnWallsDecorationObjects(gifWallsDecorationObjectPrefabs, true);
        StartCoroutine(SpawnFloorDecorationObjects(floorDecorationObjectPrefabs, true));
    }

   

    private IEnumerator WaterAnimation()
    {
        if (water == null)
        {
            yield break;
        }

        //Just in case the developer want to change the waterDaytime with the more advanced water.
        if (water.name.Contains("Water4Advanced") && waterCamera != null)
            waterCamera.SetActive(true);

        float smoothSpeed = 0.2f;

        List<GameObject> floors = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor);
        List<GameObject> ceilings = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Ceiling);
        if (floors.Count <= 0 || ceilings.Count <= 0)
            yield break;

        GameObject floor = floors[0];
        GameObject ceiling = ceilings[0];
        ceiling.GetComponent<MeshRenderer>().enabled = false; //Should be already deactivated but we make it again to be sure (in this way the player will see seagulls flying away)

        if (floor == null || ceiling == null)
            yield break;

        if (ceiling.activeInHierarchy == false)
            ceiling.SetActive(true);

        water.transform.position = new Vector3(floor.transform.position.x, floor.transform.position.y + 0.03f, floor.transform.position.x);
        seagullsGroup.transform.position = new Vector3(ceiling.transform.position.x, ceiling.transform.position.y - 0.1f, ceiling.transform.position.z);

        water.SetActive(true);
        seagullsGroup.SetActive(true);
        yield return new WaitForSeconds(2f);

        while (water.transform.position.y != ceiling.transform.position.y)
        {
            float yCeilingTarget = ceiling.transform.position.y;

            Vector3 target = new Vector3(water.transform.position.x, yCeilingTarget, water.transform.position.z);

            float step = Time.deltaTime * smoothSpeed;

            water.transform.position = Vector3.MoveTowards(water.transform.position, target, step);
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        water.SetActive(false);
        seagullsGroup.SetActive(false);
        ceiling.SetActive(false);

        if (water.name.Contains("Water4Advanced") && waterCamera != null)
            waterCamera.SetActive(false);
    }

}

//CODE TO SPAWN RANDOMLY FLOOR OBJECTS USING COLLIDER BOUNDS (imprecise if floor rotated): 
/*
Collider selectedFloorCollider = selectedFloor.GetComponent<Collider>();
Vector3 centerPointFloor = selectedFloorCollider.bounds.center;
float halfWidthFloorX = selectedFloorCollider.bounds.extents.x;
float halfLengthFloorZ = selectedFloorCollider.bounds.extents.z;

float leftExtent = centerPointFloor.x - halfWidthFloorX;
float rightExtent = centerPointFloor.x + halfWidthFloorX;
float lowerExtent = centerPointFloor.z - halfLengthFloorZ;
float upperExtent = centerPointFloor.z + halfLengthFloorZ;

float spawnPosX = Random.Range(leftExtent + 1f, rightExtent - 1f);
float spawnPosZ = Random.Range(lowerExtent + 1f, upperExtent - 1f);

spawnPos = new Vector3(spawnPosX, selectedFloor.transform.position.y + 0.05f, spawnPosZ);
*/

/* CODE TO SEE COLLIDERS DIMENSIONS
        List<GameObject> floors = new List<GameObject>();
        floors = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor);
        GameObject selectedFloor = floors[0]; //Dovrei perfezionare non considerando solo il primo floor

        //Fetch the Collider from the GameObject
        Collider m_Collider = selectedFloor.GetComponent<Collider>();
        //Fetch the center of the Collider volume
        Vector3 m_Center = m_Collider.bounds.center;
        //Fetch the size of the Collider volume
        Vector3 m_Size = m_Collider.bounds.size;
        //Fetch the minimum and maximum bounds of the Collider volume
        Vector3 m_Min = m_Collider.bounds.min;
        Vector3 m_Max = m_Collider.bounds.max;
        Vector3 m_Extents = m_Collider.bounds.extents;

        Debug.Log("Collider Center : " + m_Center);
        Debug.Log("Collider Size : " + m_Size);
        Debug.Log("Collider bound Minimum : " + m_Min);
        Debug.Log("Collider bound Maximum : " + m_Max);
        Debug.Log("Collider bound EXTENTS : " + m_Extents);
*/

