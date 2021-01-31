using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityForce : HandDevice
{

    GameObject playerRoot;

    public GameObject PlayerRoot { set => playerRoot = value; }

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

    float joltForce = 150.0f;
    Vector3 controllerVelocity;


    


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
            ReleaseStructureFromControl();

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
                structureRtt = targetStructure.GetComponent<RealtimeTransform>();
                if (structureRtt)
                {
                    Debug.Log("GravityForce: Structure ownership before request: " + structureRtt.ownerIDSelf);

                    //If player holds structure sufficiently still while controlling it, it may register as sleeping and we could loose ownership
                    structureRtt.maintainOwnershipWhileSleeping = true;
                    structureRtt.RequestOwnership();

                    Debug.Log("GravityForce: Structure ownership after request: " + structureRtt.ownerIDSelf);
                }

                structureRtw = targetStructure.GetComponent<RealtimeView>();
                if (structureRtw)
                {
                    structureRtw.preventOwnershipTakeover = true;
                }
                

                //Update state on deviceSync
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

            controllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            if (controllerVelocity.z > 1.5) targetStructureRB.AddForce(transform.forward * joltForce);
            else if (controllerVelocity.z < -1.5) targetStructureRB.AddForce(-transform.forward * joltForce);

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

        if (structureSync && (structureSync.PlayersOccupying > 0 || !structureSync.AvailableToManipulate))
        {
            Debug.Log("GravityForce: Not allowed to control structure. Reason: ");
            if (structureSync.PlayersOccupying > 0) Debug.Log("GravityForce: PlayersOccupying is more than 0 ");
            if (!structureSync.AvailableToManipulate) Debug.Log("GravityForce: AvailableToManipulate is false");

            return false;
        }

        return true;
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class. Only for devices that are picked up and dropped
    }

}
