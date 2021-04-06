using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using TMPro;
using Normal.Realtime;

public class Replicator : HandDevice
{

    Realtime realtime;

    //References specific for Replicator (Common references are in base class)
    GameObject duplicate;
    Rigidbody duplicateRB;

    StructureSync duplicateStructureSync;

    RealtimeTransform duplicateRealtimeTransform;
    RealtimeView duplicateRealtimeView;

    float distanceToStructure;

    Vector3 controlForce;

    string structureSceneName = "";
    string structurePrefabName = "";


    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    ///Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override void Operate(ref HandDeviceData handDeviceData)
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            owner.OperationState = EHandDeviceState.SCANNING;
            handDeviceData.controllingStructure = false;
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            ReleaseStructureFromControl();
            owner.OperationState = EHandDeviceState.IDLE;
            handDeviceData.controllingStructure = false;

            if (duplicate)
            {
                structureSync.CollisionEnabled = true;
                structureSync.AvailableToManipulate = true;

                duplicateStructureSync.CollisionEnabled = true;
                duplicateStructureSync.AvailableToManipulate = true;

                duplicateRealtimeTransform.maintainOwnershipWhileSleeping = false;

                duplicate = null;
                duplicateRB = null;
                duplicateRealtimeTransform = null;
            }

            return;
        }

        else if (owner.OperationState == EHandDeviceState.IDLE)
        {
            handDeviceData.controllingStructure = false;
            return;
        }

        //************ Operation logic **************//

        if (owner.OperationState == EHandDeviceState.SCANNING)
        {

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                if (!ValidateStructureState(structureHit.collider.transform.root.gameObject))
                {
                    handDeviceData.controllingStructure = false;
                    return;
                }

                

                structureSceneName = structureHit.collider.gameObject.transform.root.name;

                //Extract prefab name 
                for (int i = 0; i < structureSceneName.Length; ++i)
                {
                    if (structureSceneName[i] > 47 ) structurePrefabName += structureSceneName[i];
                    else break;
                }
                
                targetStructure.GetComponent<RealtimeTransform>().RequestOwnership();

                structureSync.AvailableToManipulate = false;
                structureSync.CollisionEnabled = false;

                duplicate = Realtime.Instantiate(structurePrefabName,
                                                      ownedByClient: false,
                                                      preventOwnershipTakeover: false,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: realtime);


                duplicateStructureSync = duplicate.GetComponent<StructureSync>();

                duplicateStructureSync.AvailableToManipulate = false;
                duplicateStructureSync.CollisionEnabled = false;
                

                duplicate.transform.position = structureHit.collider.gameObject.transform.root.position;
                duplicate.transform.rotation = structureHit.collider.gameObject.transform.root.rotation;
                
                duplicateRB = duplicate.GetComponent<Rigidbody>();

                duplicateRealtimeTransform = duplicate.GetComponent<RealtimeTransform>();
                if (duplicateRealtimeTransform)
                {
                    duplicateRealtimeTransform.RequestOwnership();
                    duplicateRealtimeTransform.maintainOwnershipWhileSleeping = true;
                }
                

                owner.OperationState = EHandDeviceState.CONTROLLING;
                handDeviceData.controllingStructure = true;

                structureSceneName = structurePrefabName = "";
            }

            return;
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            owner.DeviceSync.ControlForce = controlForce;
            owner.DeviceSync.StructurePosition = duplicate.transform.position;

            //Movement
            duplicateRB.AddForce(controlForce.normalized * 3);

            return;
        }

        //return false;
    }

    //Validate state relevant to Replicator
    protected override bool ValidateStructureState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!structureSync || 
            (structureSync && (!structureSync.AllowDuplicationByPlayer || !structureSync.AvailableToManipulate || structureSync.PlayersOccupying > 0)))
        {
            Debug.Log("Replicator: Not allowed to replicate structure! Reason: ");

            if (!structureSync) Debug.Log("The is no StructureSync object");
            if (structureSync && !structureSync.AllowDuplicationByPlayer) Debug.Log("AllowDuplicationByDevice is false");
            if (structureSync && !structureSync.AvailableToManipulate) Debug.Log("AvailableToManipulate is false");
            if (structureSync && structureSync.PlayersOccupying > 0) Debug.Log("PlayersOccupying is more than 0");

            return false;
        }
            

        else return true;
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - duplicate.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - duplicate.transform.position;

        return (structureToAdjustedForward);
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary for this class since part of OmniDevice
    }
}
