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

    EHandDeviceState mode = EHandDeviceState.IDLE;

    Vector3 controlForce;

    OVRInput.Button grabbingControllerIndexTrigger;

    string structureSceneName = "";
    string structurePrefabName = "";

    /// <summary>
    /// All the devices in the GravityController
    /// </summary>
    List<HandDevice> devices;


    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponentInChildren<ControllingBeam>();
        //beam.SetLines(mode);

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
            mode = EHandDeviceState.SCANNING;

            beam.SetVisuals(mode);
        }


        else if (OVRInput.GetUp(grabbingControllerIndexTrigger))
        {
            mode = EHandDeviceState.IDLE;
            //beam.SetLines(mode);

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

        else if (mode == EHandDeviceState.IDLE) return false;

        //************ Operation logic **************//

        if (mode == EHandDeviceState.SCANNING)
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

                //beam.SetStructureTransform(structureDuplicate.transform);

                mode = EHandDeviceState.CONTROLLING;
                beam.SetVisuals(mode);

                structureSceneName = structurePrefabName = "";
            }

            return true;
        }

        else if (mode == EHandDeviceState.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            beam.SetControlForce(controlForce);
            //beam.SetLines(mode);

            //Movement
            structureDuplicateRB.AddForce(controlForce.normalized * 2);

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
