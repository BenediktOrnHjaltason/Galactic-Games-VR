using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityForce : HandDevice
{

    GameObject playerRoot;

    public GameObject PlayerRoot { set => playerRoot = value; }

    bool pushingForward;
    bool pushingBackward;

    Vector2 stickInput;

    Rigidbody targetRB;
    Transform targetTransform;


    float distanceToStructure;

    Vector3 controlForce;
    Vector3 Up = new Vector3(0, 1, 0);

    float joltForce = 150.0f;
    Vector3 controllerVelocity;

    float controllerRollOnControlling;
    float rollMultiplier;

    bool targetRestrictedRotation;

    int layer_Structures = 10;
    int layer_UI = 5;


    public override bool Using()
    {

        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            //Update state locally and on networked deviceSync
            owner.OperationState = EHandDeviceState.SCANNING;
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


        //************ Operation logic **************//

        if (owner.OperationState == EHandDeviceState.SCANNING)
        {
            //Network: update state on clients. OmniSyncDevice manages its own beam based on its on transform on client

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << layer_Structures | 1 << layer_UI))
            {
                GameObject target = structureHit.collider.gameObject;



                if (target.layer.Equals(layer_Structures))
                {

                    if (!ValidateStructureState(structureHit.collider.transform.root.gameObject)) return true;

                    structureSync.AvailableToManipulate = false;

                    targetRB = targetStructure.GetComponent<Rigidbody>();
                    targetTransform = targetStructure.transform;

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

                    targetRestrictedRotation = (targetStructure.GetComponent<StructureOnRails>());

                    controllerRollOnControlling = transform.rotation.eulerAngles.z;

                    //Update state on deviceSync
                    owner.OperationState = EHandDeviceState.CONTROLLING;

                    //----//

                    return true;
                }

                else
                {
                    ((OmniDevice)owner).HandleUIButtons(target);
                }
            }

            return true;
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            //Network: Update controlForce and structurePosition in deviceSync, so all clients can update their own visual beam
            owner.DeviceSync.ControlForce = controlForce;
            owner.DeviceSync.StructurePosition = targetTransform.position;

            controllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            if (controllerVelocity.z > 1.5) targetRB.AddForce(transform.forward * joltForce);
            else if (controllerVelocity.z < -1.5) targetRB.AddForce(-transform.forward * joltForce);

            //Movement
            targetRB.AddForce(controlForce);

            //Rotation
            stickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            if (transform.rotation.eulerAngles.z < 340 && transform.rotation.eulerAngles.z > 200) rollMultiplier = -0.5f;
            else if (transform.rotation.eulerAngles.z > 30 && transform.rotation.eulerAngles.z < 140) rollMultiplier = 0.5f;
            else rollMultiplier = 0;

            if (!targetRestrictedRotation)
            {
                //Roll
                targetStructure.transform.Rotate(playerRoot.transform.forward, rollMultiplier, Space.World);
                //Yaw
                targetStructure.transform.Rotate(playerRoot.transform.up, ((stickInput.x * -1) / 2), Space.World);
                //Pitch
                targetStructure.transform.Rotate(playerRoot.transform.right, stickInput.y / 2, Space.World);
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
