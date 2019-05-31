
using UnityEngine;


public enum PlacementSurfaces
{
    // Horizontal surface with an upward pointing normal.    
    Horizontal = 1,

    // Vertical surface with a normal facing the user.
    Vertical = 2,
}

/// <summary>
/// The Placeable class determines the placement of an object if using the DecorationsSpawnerManager automatic placement algorithm (CreateGameObjects())
public class Placeable : MonoBehaviour
{

    [Tooltip("The type of surface on which the object can be placed.")]
    public PlacementSurfaces PlacementSurface = PlacementSurfaces.Horizontal;
    
}