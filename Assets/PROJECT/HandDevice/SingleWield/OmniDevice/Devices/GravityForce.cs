using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityForce : HandDevice
{

    bool rotating_Pitch; //Relative to player right
    bool rotating_Roll; //Relative to player forward
    bool rotating_Yaw = false;

    bool pushingForward;
    bool pushingBackward;

    Vector2 stickInput;

    Rigidbody targetStructureRB;
    Transform targetStructureTransform;


    float distanceToStructure;

    Vector3 controlForce;
    Vector3 Up = new Vector3(0, 1, 0);


    


    public override bool Using()
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            //Update state locally and on networked deviceSync
            owner.OperationState = EHandDeviceState.SCANNING;
            rotating_Yaw = false;
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            ReleaseStructureFromControl(owner);

            owner.OperationState = EHandDeviceState.IDLE;

            return false;
        }

        else if (owner.OperationState == EHandDeviceState.IDLE) return false;


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

        if (owner.OperationState == EHandDeviceState.SCANNING)
        {
            //Network: update state on clients. OmniSyncDevice manages its own beam based on its on transform on client

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {

                if (!ValidateStructureState(structureHit.collider.transform.root.gameObject)) return true;

                structureSync.AvailableToManipulate = false;

                targetStructureRB = targetStructure.GetComponent<Rigidbody>();
                targetStructureTransform = targetStructure.transform;

                //---- Networking
                RealtimeTransform rtt = targetStructure.GetComponent<RealtimeTransform>();

                rtt.RequestOwnership();

                //Update state locally and on deviceSync
                owner.OperationState = EHandDeviceState.CONTROLLING;

                //----//


                return true;
            }

            return true;
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            //Network: Update controlForce and structurePosition in deviceSync, so all clients can update their own visual beam
            owner.DeviceSync.ControlForce = controlForce;
            owner.DeviceSync.StructurePosition = targetStructureTransform.position;


            //Movement
            targetStructureRB.AddForce(controlForce);

            //Rotation
            if (rotating_Yaw) targetStructure.transform.Rotate(Up, (stickInput.x * -1) / 2, Space.World);

            else
            {
                if (rotating_Roll) targetStructure.transform.Rotate(owner.PlayerRoot.transform.forward, ((stickInput.x * -1) / 2), Space.World);

                if (rotating_Pitch) targetStructure.transform.Rotate(owner.PlayerRoot.transform.right, stickInput.y / 2, Space.World);
            }

            return true;
        }

        return false;
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

    //Validate state relevant to GravityForce
    protected override bool ValidateStructureState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!structureSync)
        {
            Debug.LogWarning(targetStructure.name + " does not have a structureSync component, and you're trying to use the GravityController on it");
            return false;
        }

        if (structureSync && ( structureSync.PlayersOccupying > 0 || !structureSync.AvailableToManipulate)) return false;

        return true;
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class. Only for devices that are picked up and dropped
    }

}
