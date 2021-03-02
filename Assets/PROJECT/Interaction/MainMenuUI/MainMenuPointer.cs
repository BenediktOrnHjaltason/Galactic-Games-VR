﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public class MainMenuPointer : HandDevice
{
    [SerializeField]
    EHandSide handSide;

    OVRInput.Button executeButton;

    LineRenderer line;


    private void Start()
    {
        executeButton = (handSide == EHandSide.LEFT) ? OVRInput.Button.Three : OVRInput.Button.One;
        line = GetComponent<LineRenderer>();
    }


    private void Update()
    {

        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + transform.forward * 10);


        if (Physics.Raycast(transform.position, transform.forward, out structureHit, 10, 1 << layer_Structures | 1 << layer_UI))
        {
            HandleUIButtons(structureHit.transform.gameObject, executeButton);
        }
    }
}
