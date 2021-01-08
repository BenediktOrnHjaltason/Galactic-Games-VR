using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalState : MonoBehaviour
{

    /*
    The purpose of this script is to hold variables that will never be neccessary to sync to other clients
    */

    /// <summary>
    /// referenses to networked objects that belong to a parent,
    /// but is not a direct part of its hierarchy, e.g. the transport beam of the zip line
    /// is controlled by the start point cube but is not directly affected by its transform when bridging the points.
    /// Used to take network ownership of subobjects.
    /// </summary>
    List<GameObject> subObjects = new List<GameObject>();

    public List<GameObject> GetSubObjects()
    {
        return subObjects;
    }

    public void AddSubObject(GameObject objectToAdd)
    {
        subObjects.Add(objectToAdd);
    }

    /// <summary>
    /// Determines if object is allowed to be duplicated by Replicator gun
    /// </summary>

    public bool allowReplicationByGun;

}
