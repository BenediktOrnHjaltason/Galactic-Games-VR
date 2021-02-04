using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VerticalMaze : MonoBehaviour
{

    [SerializeField]
    GameObject startPosition;

    // Start is called before the first frame update
    void Start()
    {
        VerticalMazeHazard[] hazards = GetComponentsInChildren<VerticalMazeHazard>();

        foreach (VerticalMazeHazard hz in hazards) hz.OnPlatformHitHazard += SendPlatformBackToStart;
    }


    void SendPlatformBackToStart(GameObject platform)
    {
        StructureSync ss = platform.transform.root.GetComponent<StructureSync>();

        if (ss) ss.BreakControl();

        else Debug.Log("VerticalMaze: Did not find structureSync");

        platform.transform.root.position = startPosition.transform.position;
    }
}
