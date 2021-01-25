using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using TMPro;
using Normal.Realtime;

public class Replicator : HandDevice
{
    //[SerializeField]
    //int allowedDuplicates;

    //[SerializeField]
    //TextMeshPro UICounter;

    Realtime realtime;

    //References specific for Replicator (Common references are in base class)
    GameObject structureDuplicate;
    Rigidbody structureDuplicateRB;

    StructureSync duplicateStructureSync;

    float distanceToStructure;

    Vector3 controlForce;

    string structureSceneName = "";
    string structurePrefabName = "";

    OmniDevice owner;
    public OmniDevice Owner { set => owner = value; }


    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        //UICounter = GetComponentInChildren<TextMeshPro>();
        //UICounter.text = allowedDuplicates.ToString();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    ///Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override bool Using()
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            owner.OperationState = EHandDeviceState.SCANNING;
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            owner.OperationState = EHandDeviceState.IDLE;

            if (structureDuplicate)
            {
                //Networking, making structures available again
                structureSync.CollisionEnabled = true;
                structureSync.AvailableToManipulate = true;

                duplicateStructureSync.CollisionEnabled = true;
                duplicateStructureSync.AvailableToManipulate = true;

                //duplicateStructureSync.CollisionEnabled = true;

                structureDuplicate = null;
                structureDuplicateRB = null;
                //UICounter.text = allowedDuplicates.ToString();
            }

            return false;
        }

        else if (owner.OperationState == EHandDeviceState.IDLE) return false;

        //************ Operation logic **************//

        if (owner.OperationState == EHandDeviceState.SCANNING)
        {

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                if (!ValidateStructureState(structureHit.collider.transform.root.gameObject)) return true;

                

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

                structureDuplicate = Realtime.Instantiate(structurePrefabName,
                                                      ownedByClient: true,
                                                      preventOwnershipTakeover: false,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: realtime);


                duplicateStructureSync = structureDuplicate.GetComponent<StructureSync>();

                duplicateStructureSync.AvailableToManipulate = false;
                duplicateStructureSync.CollisionEnabled = false;

                structureDuplicate.transform.position = structureHit.collider.gameObject.transform.root.position;
                structureDuplicate.transform.rotation = structureHit.collider.gameObject.transform.root.rotation;
                
                structureDuplicateRB = structureDuplicate.GetComponent<Rigidbody>();

                structureDuplicate.GetComponent<RealtimeTransform>().RequestOwnership();

                owner.OperationState = EHandDeviceState.CONTROLLING;

                structureSceneName = structurePrefabName = "";
            }

            return true;
        }

        else if (owner.OperationState == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            owner.DeviceSync.ControlForce = controlForce;
            owner.DeviceSync.StructurePosition = structureDuplicate.transform.position;

            //Movement
            structureDuplicateRB.AddForce(controlForce.normalized * 3);

            return true;
        }

        return false;
    }

    //Validate state relevant to Replicator
    protected override bool ValidateStructureState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!structureSync || 
            (structureSync && (!structureSync.AllowDuplicationByDevice || !structureSync.AvailableToManipulate || structureSync.PlayersOccupying > 0))) 
            return false;

        else return true;
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - structureDuplicate.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structureDuplicate.transform.position;

        return (structureToAdjustedForward);
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary for this class since part of OmniDevice
    }
}
