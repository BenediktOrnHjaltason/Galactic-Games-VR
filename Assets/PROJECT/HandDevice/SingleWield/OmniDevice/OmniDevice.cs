using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;
using System;

public class OmniDevice : HandDevice
{
    /*
    NOTE: Will be fleshed out to multi-mode device for gameplay. 
    Only GravityController at the moment
    */
    
    public GameObject playerRoot;

    [SerializeField]
    Material InactiveMaterial;

    [SerializeField]
    Material ActiveMaterial;



    OmniDeviceSync deviceSync;
    public OmniDeviceSync DeviceSync { get => deviceSync;  set => deviceSync = value; }


    Rigidbody targetStructureRB;
    Transform targetStructureTransform;

    bool pushingForward;
    bool pushingBackward;

    bool rotating_Pitch; //Relative to player right
    bool rotating_Roll; //Relative to player forward
    bool rotating_Yaw = false;

    Vector2 stickInput;

    Vector3 controlForce;

    Vector3 Up = new Vector3(0, 1, 0);

    float distanceToStructure;

    EHandDeviceState state = EHandDeviceState.IDLE;


    //Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override bool Using() 
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            //Update state locally and on networked deviceSync
            deviceSync.OperationState = state = EHandDeviceState.SCANNING;
            rotating_Yaw = false;
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            ReleaseStructureFromControl();

            deviceSync.OperationState = state = EHandDeviceState.IDLE;

            return false;
        }

        else if (state == EHandDeviceState.IDLE) return false;


        if (OVRInput.GetDown(OVRInput.Button.One))
            pushingBackward = true;
        else if (OVRInput.GetUp(OVRInput.Button.One))
            pushingBackward = false;

        if (OVRInput.GetDown(OVRInput.Button.Two))
            pushingForward = true;
        else if (OVRInput.GetUp(OVRInput.Button.Two))
            pushingForward = false;


        stickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (stickInput.x > 0.1 || stickInput.x < -0.1) rotating_Roll = true;
        else rotating_Roll = false;

        if (stickInput.y > 0.1 || stickInput.y < -0.1) rotating_Pitch = true;
        else rotating_Pitch = false;

        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
            rotating_Yaw = !rotating_Yaw;


        //************ Operation logic **************//

        if (state == EHandDeviceState.SCANNING )
        {
            //Network: update state on clients. OmniSyncDevice manages its own beam based on its on transform on client

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {

                if (!ValidateRelevantState(structureHit.collider.transform.root.gameObject)) return true;
                
                structureSync.AvailableToManipulate = false;

                targetStructureRB = targetStructure.GetComponent<Rigidbody>();
                targetStructureTransform = targetStructure.transform;

                //---- Networking
                RealtimeTransform rtt = targetStructure.GetComponent<RealtimeTransform>();

                rtt.RequestOwnership();

                //Update state locally and on deviceSync
                deviceSync.OperationState = state = EHandDeviceState.CONTROLLING;

                //----//


                return true;
            }

            return true;
        }

        else if (state == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            //Network: Update controlForce and structurePosition in deviceSync, so all clients can update their own visual beam
            deviceSync.ControlForce = controlForce;
            deviceSync.StructurePosition = targetStructureTransform.position;


            //Movement
            targetStructureRB.AddForce(controlForce);
            
            //Rotation
            if (rotating_Yaw) targetStructure.transform.Rotate(Up, (stickInput.x * -1) / 2, Space.World);
            
            else
            {
                if (rotating_Roll) targetStructure.transform.Rotate(playerRoot.transform.forward, ((stickInput.x * -1) / 2), Space.World);
            
                if (rotating_Pitch) targetStructure.transform.Rotate(playerRoot.transform.right, stickInput.y / 2, Space.World);
            }

            return true;
        }

        return false;
    }

    //Validate state relevant to GravityController
    protected override bool ValidateRelevantState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!structureSync)
        {
            Debug.LogWarning(targetStructure.name + " does not have a structureSync component, and you're trying to use the GravityController on it");
            return false;
        }

        if (structureSync && structureSync.PlayersOccupying > 0) return false;

        return true;
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - targetStructure.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - targetStructure.transform.position;

        float forwardMultiplyer = (pushingForward) ? 7.0f : 0.0f;
        forwardMultiplyer += (pushingBackward) ? -7.0f : 0.0f;

        return (structureToAdjustedForward + transform.forward * forwardMultiplyer);
    }

    void ReleaseStructureFromControl()
    {
        if (structureSync && state == EHandDeviceState.CONTROLLING) structureSync.AvailableToManipulate = true;
    }

    private void FixedUpdate()
    {
        if (state == EHandDeviceState.CONTROLLING && structureSync.PlayersOccupying > 0) ReleaseStructureFromControl();
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class
    }
}
