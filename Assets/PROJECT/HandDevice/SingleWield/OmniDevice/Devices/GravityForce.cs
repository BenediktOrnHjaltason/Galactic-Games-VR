﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityForce : HandDevice
{

    GameObject playerRoot;
    Realtime realtime;

    public GameObject PlayerRoot { set => playerRoot = value; }

    bool pushingStructure;
    bool pullingStructure;

    Vector2 stickInput;

    Rigidbody targetRB;
    Transform targetTransform;

    //----If replicating
    GameObject duplicate;
    //Rigidbody duplicateRB;

    StructureSync duplicateStructureSync;

    StructureSync sourceStructureSync;

    RealtimeTransform duplicateRealtimeTransform;
    RealtimeTransform sourceRtt;

    string structureSceneName = "";
    string structurePrefabName = "";
    //----


    float distanceToStructure;

    Vector3 controlForce;
    Vector3 Up = new Vector3(0, 1, 0);

    float joltForce = 380.0f;
    Vector3 controllerVelocity;

    float rollMultiplier;

    [SerializeField]
    float movementMultiplier = 100;

    [SerializeField]
    float rotationMultiplier = 2600;

    bool replicating = false;

    

    private void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    public override void Operate(ref HandDeviceData handDeviceData)
    {

        //************ Manage input **************//

        if (OVRInput.Get(handTrigger) > 0.8f) replicating = true;

        else replicating = false;


        if (OVRInput.GetDown(indexTrigger))
        {
            if (structureSync) ReleaseStructureFromControl();

            //Update state locally and on networked deviceSync
            owner.OperationState = EHandDeviceState.SCANNING;
        }


        else if (OVRInput.GetUp(indexTrigger))
        {
            ReleaseStructureFromControl();

            if (duplicate)
            {
                structureSync.CollisionEnabled = true;
                structureSync.AvailableToManipulate = true;

                sourceStructureSync.CollisionEnabled = true;
                sourceStructureSync.AvailableToManipulate = true;

                duplicateRealtimeTransform.maintainOwnershipWhileSleeping = false;

                duplicate = null;
                duplicateRealtimeTransform = null;
            }

            owner.OperationState = EHandDeviceState.IDLE;
            handDeviceData.controllingStructure = false;

            pullingStructure = false;
            pushingStructure = false;

            return;
        }


        if (owner.OperationState == EHandDeviceState.IDLE)
        {
            if (handDeviceData.controllingStructure) handDeviceData.controllingStructure = false;
            return;
        }


        //************ Operation logic **************//

        if (owner.OperationState == EHandDeviceState.SCANNING)
        {
            //Network: update state on clients. OmniDeviceSync manages its own beam based on its on transform on client

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << layer_Structures | 1 << layer_UI | 1 << layer_GeneralBlock))
            {
                GameObject target = structureHit.collider.gameObject;

                if (target.layer.Equals(layer_Structures))
                {

                    if (!ValidateStructureState(structureHit.collider.transform.parent.gameObject))
                    {
                        handDeviceData.controllingStructure = false;
                        return;
                    }

                    handDeviceData.controllingStructure = true;

                    if (!replicating)
                    {
                        structureSync.CalculatePlayerAngleModifier(transform.position);

                        structureSync.OwnedByPlayer = true;

                        structureSync.AvailableToManipulate = false;
                        structureSync.OnBreakControl += ReleaseStructureFromControl;

                        

                        handDeviceData.targetStructureAllowsRotation = structureSync.AllowRotationForceByPlayer;
                        targetTransform = targetStructure.transform;

                        //---- Networking
                        structureRtt = targetStructure.GetComponent<RealtimeTransform>();
                        if (structureRtt)
                        {
                            //If player holds structure sufficiently still while controlling it, it may register as sleeping and we could loose ownership
                            structureRtt.maintainOwnershipWhileSleeping = true;
                            structureRtt.SetOwnership(realtime.clientID);
                        }

                        //Update state on deviceSync
                        owner.OperationState = EHandDeviceState.CONTROLLING;

                    }

                    else
                    {
                        sourceStructureSync = structureSync;

                        structureSceneName = structureHit.collider.gameObject.transform.root.name;

                        //Extract prefab name 
                        for (int i = 0; i < structureSceneName.Length; ++i)
                        {
                            if (structureSceneName[i] > 47) structurePrefabName += structureSceneName[i];
                            else break;
                        }

                        //The source target
                        targetStructure.GetComponent<RealtimeTransform>().RequestOwnership();

                        structureSync.AvailableToManipulate = false;
                        structureSync.CollisionEnabled = false;

                        duplicate = Realtime.Instantiate(structurePrefabName,
                                                              ownedByClient: false,
                                                              preventOwnershipTakeover: false,
                                                              destroyWhenOwnerOrLastClientLeaves: true,
                                                              useInstance: realtime);

                        //The duplicate target
                        structureSync = duplicate.GetComponent<StructureSync>();

                        structureSync.AvailableToManipulate = false;
                        structureSync.CollisionEnabled = false;

                        handDeviceData.targetStructureAllowsRotation = structureSync.AllowRotationForceByPlayer;


                        targetStructure = duplicate;
                        targetTransform = structureSync.transform;

                        //Place newly created duplicate in same position and rotation as source
                        duplicate.transform.position = structureHit.collider.gameObject.transform.root.position;
                        duplicate.transform.rotation = structureHit.collider.gameObject.transform.root.rotation;

                        //duplicateRB = duplicate.GetComponent<Rigidbody>();

                        duplicateRealtimeTransform = duplicate.GetComponent<RealtimeTransform>();
                        if (duplicateRealtimeTransform)
                        {
                            duplicateRealtimeTransform.RequestOwnership();
                            duplicateRealtimeTransform.maintainOwnershipWhileSleeping = true;
                        }


                        owner.OperationState = EHandDeviceState.CONTROLLING;

                        structureSceneName = structurePrefabName = "";
                    }
                }

                else
                {
                    handDeviceData.controllingStructure = false;
                    HandleUIButtons(target);
                }
            }
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {

            if (OVRInput.GetDown(structurePull))
                pullingStructure = true;
            else if (OVRInput.GetUp(structurePull))
                pullingStructure = false;

            if (OVRInput.GetDown(structurePush))
                pushingStructure = true;
            else if (OVRInput.GetUp(structurePush))
                pushingStructure = false;



            controlForce = CalculateControlForce();

            //Network: Update controlForce and structurePosition in deviceSync, so all clients can update their own visual beam
            owner.DeviceSync.ControlForce = controlForce;
            owner.DeviceSync.StructurePosition = targetTransform.position;

            controllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            if (controllerVelocity.z > 1.5) structureSync.AddGravityForce(transform.forward * joltForce);
            else if (controllerVelocity.z < -1.5) structureSync.AddGravityForce(-transform.forward * joltForce);

            
            //Movement
            structureSync.AddGravityForce(controlForce * Time.deltaTime * movementMultiplier);

            //Rotation
            stickInput = OVRInput.Get(thumbStick);

            float z = transform.rotation.eulerAngles.z;

            if (z < 330 && z > 220)
            {
                rollMultiplier = -1.5f * ((110 - (z - 220))/110);
                //Debug.Log("GravityForce: Rolling to the right. coefficient = " + ((110 - (z - 220)) / 110));
            }
            else if (z > 30 && z < 140)
            {
                rollMultiplier = 1.5f * ((z - 30) / 110);
                //Debug.Log("GravityForce: Rolling to the left. coefficient = " + ((z - 30) / 110));
            }
            else rollMultiplier = 0;

            if (structureSync.AllowRotationForceByPlayer)
            {
                structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     /*Yaw*/((stickInput.x * -1) / 2) * rotationMultiplier * Time.deltaTime,
                                     /*Pitch*/playerRoot.transform.right, (stickInput.y / 2) * rotationMultiplier * Time.deltaTime);
            }
        }
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - structureSync.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structureSync.transform.position;

        float pushPullForce = (pushingStructure) ? 7.0f : 0.0f;


        if (distanceToStructure > 7) pushPullForce += (pullingStructure) ? -7.0f : 0.0f;

        else pushPullForce += (pullingStructure) ? -7.0f + (7 - distanceToStructure) : 0.0f;

        return (structureToAdjustedForward + transform.forward * pushPullForce * structureSync.PushPullMultiplier);
    }

    //Validate state relevant to GravityForce
    protected override bool ValidateStructureState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!replicating)
        {

            if (!structureSync)
            {
                Debug.LogWarning(targetStructure.name + " does not have a structureSync component, and you're trying to use the GravityController on it");
                return false;
            }

            if (structureSync && 
                (!structureSync.AllowGravityForceByPlayer && !structureSync.AllowRotationForceByPlayer) || 
                structureSync.PlayersOccupying > 0 || 
                !structureSync.AvailableToManipulate)
            {
                
                Debug.Log("GravityForce: Not allowed to control structure. Reason: ");
                if (!structureSync.AllowGravityForceByPlayer && !structureSync.AllowRotationForceByPlayer) 
                    Debug.Log("GravityForce: Neither GravityForce nor rotation forces are allowed ");
                if (structureSync.PlayersOccupying > 0) Debug.Log("GravityForce: PlayersOccupying is more than 0 ");
                if (!structureSync.AvailableToManipulate) Debug.Log("GravityForce: AvailableToManipulate is false");
                
                return false;
            }

            else return true;
        }

        else
        {
            if (!structureSync ||
            (structureSync && (!structureSync.AllowDuplicationByPlayer || !structureSync.AvailableToManipulate || structureSync.PlayersOccupying > 0)))
            {
                /*
                Debug.Log("Replicator: Not allowed to replicate structure! Reason: ");

                if (!structureSync) Debug.Log("The is no StructureSync object");
                if (structureSync && !structureSync.AllowDuplicationByDevice) Debug.Log("AllowDuplicationByDevice is false");
                if (structureSync && !structureSync.AvailableToManipulate) Debug.Log("AvailableToManipulate is false");
                if (structureSync && structureSync.PlayersOccupying > 0) Debug.Log("PlayersOccupying is more than 0");
                */
                return false;
            }


            else return true;
        }
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class. Only for devices that are picked up and dropped
    }

}
