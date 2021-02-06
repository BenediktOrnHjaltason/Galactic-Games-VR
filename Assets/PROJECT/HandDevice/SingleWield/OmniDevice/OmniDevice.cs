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

    List<GameObject> scales = new List<GameObject>();
    List<Vector3> scalesLocalPositionsBase = new List<Vector3>();


    Transform scalesBase;

    [SerializeField]
    List<Vector3> scalesLocalPositionsEnd;

    float operationEffectMultiplier;
    bool scalesReset = false;
    float timeWaveOffsett = (Mathf.PI * 2) / 6;

    GameObject buttonObjectPointedAtPreviously = null;
    InteractButton button;
    InteractButton previousButton;

    
     


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
    //deviceSync is located there so it can control it's own mesh and beam.
    //Maybe it could be located locally and reference mesh and beam from spawned hand. 
    public void Initialize(GameObject spawnedHand)
    {
        this.deviceSync = spawnedHand.GetComponentInChildren<HandDeviceSync>(); 

        devices.Add(gravityForce);
        devices.Add(replicator);

        Mode = EOmniDeviceMode.GRAVITYFORCE;

        scalesBase = spawnedHand.transform.GetChild(3);

        for (int i = 0; i < 6; i++)
        {
            scales.Add(scalesBase.transform.GetChild(i).gameObject);
            scalesLocalPositionsBase.Add(scales[i].transform.localPosition);
        }

        scalesLocalPositionsEnd.Add(new Vector3(0.0f, 0.04994f, -0.0282f));
        scalesLocalPositionsEnd.Add(new Vector3(0.03878f, 0.02724f, -0.0282f));
        scalesLocalPositionsEnd.Add(new Vector3(0.03859f, -0.01703f, -0.0282f));
        scalesLocalPositionsEnd.Add(new Vector3(0.0007f, -0.03938f, -0.0282f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.03842f, -0.01737f, -0.0282f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.0385f, 0.0279f, -0.0282f));

    }

    public void SetDeviceMode(int index)
    {
        Mode = (EOmniDeviceMode)index;

        Debug.Log("OmniDevice: OmniDevice set to " + mode.ToString());
    }

    //Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override bool Using()
    {
        //Using operates the active device
        return devices[activeDeviceIndex].Using();
    }


    private void FixedUpdate()
    {
        if (deviceSync) AnimateDevice();
    }

    void AnimateDevice()
    {
        if (operationEffectMultiplier < 0.0f && !scalesReset)
        {
            for (int i = 0; i < scales.Count; i++) scales[i].transform.localPosition = scalesLocalPositionsBase[i];
            scalesReset = true;
        }

        if (deviceSync.OperationState == EHandDeviceState.IDLE && operationEffectMultiplier > 0)
        {
            operationEffectMultiplier -= 0.2f;

            if (scalesReset) scalesReset = false;
        }

        else if (deviceSync.OperationState == EHandDeviceState.SCANNING)
        {
            if (operationEffectMultiplier < 1) operationEffectMultiplier += 0.2f;

            for (int i = 0; i < scales.Count; i++)
            {
                scales[i].transform.localPosition =

                    Vector3.Lerp(scalesLocalPositionsBase[i], scalesLocalPositionsEnd[i], (Mathf.Abs(Mathf.Sin(Time.time * 5))) * operationEffectMultiplier);
            }
        }


        else if (deviceSync.OperationState == EHandDeviceState.CONTROLLING)
        {
            if (operationEffectMultiplier < 1) operationEffectMultiplier += 0.2f;

            for (int i = 0; i < scales.Count; i++)
            {
                scales[i].transform.localPosition =

                    Vector3.Lerp(scalesLocalPositionsBase[i], scalesLocalPositionsEnd[i], (((Mathf.Sin(Time.time * 3 + (timeWaveOffsett * (i + 1))) + 1) / 2)) * operationEffectMultiplier);
            }
        }
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class
    }

    protected override bool ValidateStructureState(GameObject target)
    {
        //Nothing necessary here for this class

        return true;
    }

    //Called only during Raytracing of buttons
    public void HandleUIButtons(GameObject buttonPointedAt)
    {
        if (buttonPointedAt != buttonObjectPointedAtPreviously)
        {

            buttonObjectPointedAtPreviously = buttonPointedAt;

            button = buttonObjectPointedAtPreviously.GetComponent<InteractButton>();
        }
        if (button) button.BeingHighlighted = true;



        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (button) button.Execute();
        }
    }
}
