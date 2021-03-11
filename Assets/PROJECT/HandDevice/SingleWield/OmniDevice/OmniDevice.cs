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



    //---------MOVING
    /*
    List<GameObject> floaties = new List<GameObject>();
    List<Vector3> scalesLocalPositionsBase = new List<Vector3>();


    Transform floatiesBase;
    Vector3 floatiesLocalPositionStart = new Vector3(-0.0002236087f, 0.09533767f, -0.04992739f);

    List<Vector3> scalesLocalPositionsEnd = new List<Vector3>();

    float operationEffectMultiplier;
    bool scalesReset = false;
    float timeWaveOffsett = (Mathf.PI * 2) / 6;
    */
    //---------/MOVING



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
    //deviceSync is located there so it can control it's own mesh and beam. (+ animating floaties?)
    //Maybe it could be located on deviceSync/OmniDeviceSync which references the synced hand object
    public void Initialize(GameObject spawnedHand, EHandSide handSide)
    {
        deviceSync = spawnedHand.GetComponentInChildren<OmniDeviceSync>();

        gravityForce.Initialize(handSide);

        devices.Add(gravityForce);

        //Not used for anything atm. Added replication as mode for gravity force
        devices.Add(replicator);

        Mode = EOmniDeviceMode.GRAVITYFORCE;


        //MOVING
        /*
        floatiesBase = spawnedHand.transform.GetChild(1);

        for (int i = 0; i < 8; i++)
        {
            floaties.Add(floatiesBase.GetChild(i).gameObject);
            scalesLocalPositionsBase.Add(floaties[i].transform.localPosition);
        }

        scalesLocalPositionsEnd.Add(new Vector3(-0.0002236087f, 0.201f, -0.04992739f));
        scalesLocalPositionsEnd.Add(new Vector3(0.073f, 0.172f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(0.106f, 0.09533767f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(0.075f, 0.022f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.0002236087f, -0.009f, -0.04992739f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.074f, 0.019f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.107f, 0.09533767f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.073f, 0.172f, -0.05f));
        */
        //-/MOVING
    }

    public void SetDeviceMode(int index)
    {
        Mode = (EOmniDeviceMode)index;

        Debug.Log("OmniDevice: OmniDevice set to " + mode.ToString());
    }

    //Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override void Using(ref HandDeviceData data)
    {
        devices[activeDeviceIndex].Using(ref data);
    }


    private void FixedUpdate()
    {
        //----MOVING
        //if (deviceSync) AnimateDevice();
        //---/MOVING
    }

    //----MOVING
    /*
    void AnimateDevice()
    {
        if (operationEffectMultiplier < 0.0f && !scalesReset)
        {
            for (int i = 0; i < floaties.Count; i++) floaties[i].transform.localPosition = scalesLocalPositionsBase[i];
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

            for (int i = 0; i < floaties.Count; i++)
            {
                floaties[i].transform.localPosition =

                    Vector3.Lerp(floatiesLocalPositionStart, scalesLocalPositionsEnd[i], (Mathf.Abs(Mathf.Sin(Time.time * 5))) * operationEffectMultiplier);
            }
        }


        else if (deviceSync.OperationState == EHandDeviceState.CONTROLLING)
        {
            if (operationEffectMultiplier < 1) operationEffectMultiplier += 0.2f;

            for (int i = 0; i < floaties.Count; i++)
            {
                floaties[i].transform.localPosition =

                    Vector3.Lerp(floatiesLocalPositionStart, scalesLocalPositionsEnd[i], (((Mathf.Sin(Time.time * 3 + (timeWaveOffsett * (i + 1))) + 1) / 2)) * operationEffectMultiplier);
            }
        }
    }
    //---/MOVING
    */


    /*
    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class
    }

    protected override bool ValidateStructureState(GameObject target)
    {
        //Nothing necessary here for this class

        return true;
    }
    */

}
