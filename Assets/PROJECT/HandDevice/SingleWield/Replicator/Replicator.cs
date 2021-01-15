using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using TMPro;
using Normal.Realtime;

public class Replicator : HandDevice
{

    [SerializeField]
    Material InactiveMaterial;

    [SerializeField]
    Material ActiveMaterial;

    [SerializeField]
    int allowedReplicates;

    [SerializeField]
    TextMeshPro UICounter;

    Realtime realtime;

    ControllingBeam beam;

    //References specific for Replicator (Common references are in base class)
    GameObject structureDuplicate;
    Rigidbody structureDuplicateRB;

    StructureSync duplicateStructureSync;

    float distanceToStructure;

    EControlBeamMode mode = EControlBeamMode.IDLE;

    Vector3 controlForce;

    OVRInput.Button grabbingControllerIndexTrigger;

    string structureSceneName = "";
    string structurePrefabName = "";


    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponentInChildren<ControllingBeam>();
        beam.SetMaterialReferences(ActiveMaterial, InactiveMaterial);
        beam.SetLines(mode);

        RB = GetComponent<Rigidbody>();

        UICounter = GetComponentInChildren<TextMeshPro>();
        UICounter.text = allowedReplicates.ToString();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    ///Operates the HandDevice. Returns true or false so Hand.cs can restrict grabbing/climbing while operating it
    public override bool Using()
    {
        //************ Manage input **************//

        if (allowedReplicates > 0 && OVRInput.GetDown(grabbingControllerIndexTrigger))
        {
            mode = EControlBeamMode.SCANNING;

            beam.SetVisuals(mode);
        }


        else if (OVRInput.GetUp(grabbingControllerIndexTrigger))
        {
            mode = EControlBeamMode.IDLE;
            beam.SetLines(mode);

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

                allowedReplicates--;
                UICounter.text = allowedReplicates.ToString();
            }

            return false;
        }

        else if (mode == EControlBeamMode.IDLE) return false;

        //************ Operation logic **************//

        if (mode == EControlBeamMode.SCANNING)
        {
            beam.SetLines(mode);

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                if (!ValidateRelevantState(structureHit.collider.transform.root.gameObject)) return true;

                

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

                beam.SetStructureTransform(structureDuplicate.transform);

                mode = EControlBeamMode.CONTROLLING;
                beam.SetVisuals(mode);

                structureSceneName = structurePrefabName = "";
            }

            return true;
        }

        else if (mode == EControlBeamMode.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            beam.SetControlForce(controlForce);
            beam.SetLines(mode);

            //Movement
            structureDuplicateRB.AddForce(controlForce.normalized * 2);

            return true;
        }

        return false;
    }

    //Validate state relevant to Replicator
    protected override bool ValidateRelevantState(GameObject target)
    {
        GetStateReferencesFromTarget(target);

        if (!structureLocal || (structureLocal && !structureLocal.AllowReplicationByRGun)) return false;

        if (!structureSync || (structureSync && !structureSync.AvailableToManipulate) ||
            structureSync && structureSync.PlayersOccupying > 0) return false;

        return true;
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
        switch(hand)
        {
            case EHandSide.LEFT:

                grabbingControllerIndexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                break;

            case EHandSide.RIGHT:
                grabbingControllerIndexTrigger = OVRInput.Button.SecondaryIndexTrigger;
                break;
        }
    }
}
