﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class OmniDevice : HandDevice
{
    [SerializeField]
    GameObject playerRoot;

    public GameObject PlayerRoot { get => playerRoot; }

    public EHandDeviceState OperationState { set => deviceSync.OperationState = value; get => deviceSync.OperationState; }


    /// <summary>
    /// All the devices belonging to the OmniDevice
    /// </summary>
    List<HandDevice> devices = new List<HandDevice>();

    [SerializeField]
    GravityForce gravityForce;

    int activeDeviceIndex;

    EOmniDeviceMode mode = EOmniDeviceMode.NONE;

    EOmniDeviceMode Mode
    {
        set
        {
            mode = value;
            activeDeviceIndex = (int)mode;
        }
    }

    private void Awake()
    {
        gravityForce = GetComponent<GravityForce>();
        gravityForce.Owner = this;

        //Just so we don't get a nullreference in Using() before hands are spawned when client connects to server.
        //Allows for no if-testing in Using()
        devices.Add(new DummyDevice());
    }

    //Must be initialized after spawning hands on network, because deviceSync is located there
    //GravityForce and other OmniDevice device uses deviceSync in their operations.
    //deviceSync is located there so it can control it's own mesh and beam.
    //Maybe it could be located locally and reference mesh and beam from spawned hand. 
    public void Initialize(OmniDeviceSync deviceSync)
    {
        this.deviceSync = deviceSync; 

        devices.Add(gravityForce);
        Mode = EOmniDeviceMode.GRAVITYFORCE;
    }

    //Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override bool Using()
    {
        return devices[activeDeviceIndex].Using();
    }


    public void ReleaseStructureFromControl()
    {
        if (structureSync && OperationState == EHandDeviceState.CONTROLLING) structureSync.AvailableToManipulate = true;
    }

    private void FixedUpdate()
    {

        if (deviceSync && deviceSync.OperationState == EHandDeviceState.CONTROLLING && devices[activeDeviceIndex].StructureSync.PlayersOccupying > 0)
            ReleaseStructureFromControl();
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class
    }

    protected override bool ValidateStructureState(GameObject target)
    {
        //Nothing necessary here for this class
        //Possible confusions here: Both this OmniDevice class, and the devices it will be owning by composition inherit from HandDevice.
        //At the moment this feels like the most flexible in regards to having devices inside OmniDevice and also have externally equippable devices.
        return true;
    }

}
