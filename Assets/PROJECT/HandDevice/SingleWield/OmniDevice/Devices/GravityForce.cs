using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityForce : HandDevice
{

    GameObject playerRoot;
    Realtime realtime;

    public GameObject PlayerRoot { set => playerRoot = value; }

    bool pushingForward;
    bool pushingBackward;

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

    //Buttons depending on HandSide

    OVRInput.Button indexTrigger;
    OVRInput.Axis1D handTrigger;
    OVRInput.Button platformForward;
    OVRInput.Button platformBackward;
    OVRInput.Axis2D thumbStick;

    

    public void Initialize(EHandSide handSide)
    {
        if (handSide == EHandSide.RIGHT)
        {
            indexTrigger = OVRInput.Button.SecondaryIndexTrigger;
            handTrigger = OVRInput.Axis1D.SecondaryHandTrigger;
            platformBackward = OVRInput.Button.One;
            platformForward = OVRInput.Button.Two;
            thumbStick = OVRInput.Axis2D.SecondaryThumbstick;
        }

        else if (handSide == EHandSide.LEFT)
        {
            indexTrigger = OVRInput.Button.PrimaryIndexTrigger;
            handTrigger = OVRInput.Axis1D.PrimaryHandTrigger;
            platformBackward = OVRInput.Button.Three;
            platformForward = OVRInput.Button.Four;
            thumbStick = OVRInput.Axis2D.PrimaryThumbstick;
        }
    }

    private void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    public override void Using(ref HandDeviceData handDeviceData)
    {

        //************ Manage input **************//

        if (OVRInput.Get(handTrigger) > 0.8f) replicating = true;

        else replicating = false;


        if (OVRInput.GetDown(indexTrigger))
        {
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

            return;
        }


        if (owner.OperationState == EHandDeviceState.IDLE)
        {
            if (handDeviceData.controllingStructure) handDeviceData.controllingStructure = false;
            return;
        }


        if (OVRInput.GetDown(platformBackward))
            pushingBackward = true;
        else if (OVRInput.GetUp(platformBackward))
            pushingBackward = false;

        if (OVRInput.GetDown(platformForward))
            pushingForward = true;
        else if (OVRInput.GetUp(platformForward))
            pushingForward = false;


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
                        structureSync.AvailableToManipulate = false;
                        structureSync.OnBreakControl += ReleaseStructureFromControl;

                        handDeviceData.targetStructureAllowsRotation = structureSync.AllowRotationForces;
                        targetTransform = targetStructure.transform;

                        //---- Networking
                        structureRtt = targetStructure.GetComponent<RealtimeTransform>();
                        if (structureRtt)
                        {
                            //If player holds structure sufficiently still while controlling it, it may register as sleeping and we could loose ownership
                            structureRtt.maintainOwnershipWhileSleeping = true;
                            structureRtt.RequestOwnership();
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

                        handDeviceData.targetStructureAllowsRotation = structureSync.AllowRotationForces;


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
                    HandleUIButtons(target, platformBackward);
                }
            }
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {
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

            if (structureSync.AllowRotationForces)
            {
                float atan = Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg;


                //Debug.Log("Atan Rad2Deg = " + atan);
                //Debug.Log("Stick input: " + stickInput);

                if (atan > -45f && atan < 45f )
                {
                    //Debug.Log("RIGHT");
                    structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     /*Yaw*/((stickInput.x * -1) / 2) * rotationMultiplier * Time.deltaTime,
                                     /*Pitch*/Vector3.zero,0);

                }
                else if (atan > 45f && atan < 135f)
                {
                    //Debug.Log("UP");
                    structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     /*Yaw*/0,
                                     /*Pitch*/playerRoot.transform.right, (stickInput.y / 2) * rotationMultiplier * Time.deltaTime);
                }
                else if (atan > 135f || atan < -135)
                {
                    //Debug.Log("LEFT");
                    structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     /*Yaw*/((stickInput.x * -1) / 2) * rotationMultiplier * Time.deltaTime,
                                     /*Pitch*/Vector3.zero, 0);
                }
                else if (atan > -135 && atan < -45)
                {
                    //Debug.Log("DOWN");
                    structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     /*Yaw*/0,
                                     /*Pitch*/playerRoot.transform.right, (stickInput.y / 2) * rotationMultiplier * Time.deltaTime);
                }
                

                
                //structureSync.Rotate(/*Roll*/playerRoot.transform.forward, rollMultiplier * rotationMultiplier * Time.deltaTime,
                                     ///*Yaw*/((stickInput.x * -1) / 2) * rotationMultiplier * Time.deltaTime,
                                     ///*Pitch*/playerRoot.transform.right, (stickInput.y / 2) * rotationMultiplier * Time.deltaTime);
                
            }
        }
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - structureSync.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structureSync.transform.position;

        float forwardMultiplier = (pushingForward) ? 7.0f : 0.0f;


        if (distanceToStructure > 7) forwardMultiplier += (pushingBackward) ? -7.0f : 0.0f;

        else forwardMultiplier += (pushingBackward) ? -7.0f + (7 - distanceToStructure) : 0.0f;

        return (structureToAdjustedForward + transform.forward * forwardMultiplier);
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

            if (structureSync && (structureSync.PlayersOccupying > 0 || !structureSync.AvailableToManipulate))
            {
                Debug.Log("GravityForce: Not allowed to control structure. Reason: ");
                if (structureSync.PlayersOccupying > 0) Debug.Log("GravityForce: PlayersOccupying is more than 0 ");
                if (!structureSync.AvailableToManipulate) Debug.Log("GravityForce: AvailableToManipulate is false");

                return false;
            }

            else return true;
        }

        else
        {
            if (!structureSync ||
            (structureSync && (!structureSync.AllowDuplicationByDevice || !structureSync.AvailableToManipulate || structureSync.PlayersOccupying > 0)))
            {
                Debug.Log("Replicator: Not allowed to replicate structure! Reason: ");

                if (!structureSync) Debug.Log("The is no StructureSync object");
                if (structureSync && !structureSync.AllowDuplicationByDevice) Debug.Log("AllowDuplicationByDevice is false");
                if (structureSync && !structureSync.AvailableToManipulate) Debug.Log("AvailableToManipulate is false");
                if (structureSync && structureSync.PlayersOccupying > 0) Debug.Log("PlayersOccupying is more than 0");

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
