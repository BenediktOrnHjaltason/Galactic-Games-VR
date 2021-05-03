using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This script is to manage turning on collisions for gameplay prefabs that have more collisions than the standard
    one that the user interacts with to take control of objects, e.g. Rotate bridge has a green ball that the player
    targets to take control, but also other bridge pieces that need collisions. 

    Because we are spawning multiple instances of gameplay objects in the same position per team, gameplay object 
    prefabs have all collisions turned off to avoid misallignments caused by collisions, and the game manager turns on 
    collisions after non team objects are filtered out. Scene instances have collisions turned on as an override so that 
    players can go through the game without forming teams.
 */

public class CollisionAfterTeamFiltering : MonoBehaviour
{
    [SerializeField]
    int collisionLayer;

    [SerializeField]
    List<GameObject> objectsWithCollision;

    public void TurnOnCollisions()
    {
        foreach (GameObject gameObject in objectsWithCollision)
            gameObject.layer = collisionLayer;
    }

}
