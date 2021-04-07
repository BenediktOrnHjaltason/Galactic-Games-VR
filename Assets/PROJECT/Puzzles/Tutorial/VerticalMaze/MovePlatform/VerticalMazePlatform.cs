using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VerticalMazePlatform : MonoBehaviour
{

    [SerializeField]
    GameObject startPosition;

    [SerializeField]
    Rigidbody leftWallMazePlatformRB;

    [SerializeField]
    Rigidbody frontMazePlatformRB;

    // Start is called before the first frame update
    void Start()
    {
        VerticalMazeHazard[] hazards = GetComponentsInChildren<VerticalMazeHazard>();

        foreach (VerticalMazeHazard hz in hazards) hz.OnPlatformHitHazard += SendPlatformBackToStart;

        //Overriding rigidbody constraints for platform in this maze. Edge case.
        leftWallMazePlatformRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                                 RigidbodyConstraints.FreezePositionZ;

        frontMazePlatformRB.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ |
                                RigidbodyConstraints.FreezePositionX;
    }


    void SendPlatformBackToStart(GameObject platform)
    {
        StructureSync ss = platform.transform.root.GetComponent<StructureSync>();

        if (ss) ss.BreakControl();

        else Debug.Log("VerticalMaze: Did not find structureSync");

        platform.transform.root.position = startPosition.transform.position;
    }
}
