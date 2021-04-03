using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using System;


//Possible confusions here: Both this OmniDevice, and the devices it will be owning by composition inherit from HandDevice.
//At the moment this feels like the most flexible in regards to having devices inside OmniDevice and also have externally equippable devices,
//letting Hand use the Using() function of all HandDevice-types

public class OmniDevice : HandDevice
{
    [SerializeField]
    GameObject playerRoot;

    public GameObject PlayerRoot { get => playerRoot; }


    /// <summary>
    /// All the devices belonging to the OmniDevice
    /// </summary>
    List<HandDevice> devices = new List<HandDevice>();

    public int NumberOfDevices { get => devices.Count; }

    GravityForce gravityForce;
    Replicator replicator;
    HandDevice dummyDevice;

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
        gravityForce.PlayerRoot = playerRoot;

        replicator = GetComponent<Replicator>();
        replicator.Owner = this;

        dummyDevice = GetComponent<DummyDevice>();


        //Just so we don't get a nullreference in Using() before hands are spawned when client connects to server.
        //Allows for no if-testing in Using()
        devices.Add(dummyDevice);
    }

    //Must be initialized after spawning hands on network, because deviceSync is located there
    //GravityForce and other OmniDevice device uses deviceSync in their operations.

    public void Initialize(GameObject spawnedHand, EHandSide handSide)
    {
        deviceSync = spawnedHand.GetComponentInChildren<OmniDeviceSync>();

        gravityForce.Initialize(handSide);

        devices.Add(gravityForce);

        //Not used for anything atm. Added replication as mode for gravity force
        devices.Add(replicator);

        Mode = EOmniDeviceMode.GRAVITYFORCE;
    }

    public void SetDeviceMode(int index)
    {
        Mode = (EOmniDeviceMode)index;

        Debug.Log("OmniDevice: OmniDevice set to " + mode.ToString());
    }

    //Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override void Operate(ref HandDeviceData data)
    {
        devices[activeDeviceIndex].Operate(ref data);
    }
}
