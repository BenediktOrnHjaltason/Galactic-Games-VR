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

    RaycastHit structureHit;
    GameObject structureDuplicate;

    Rigidbody structureDuplicateRB;

    Collider structureCollider;

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

            if (structureDuplicate)
            {
                structureCollider.enabled = true;
                structureCollider = null;

                //Networking, making structures available again
                structureDuplicate.GetComponent<Availability>().Available = true;
                structureHit.collider.gameObject.GetComponent<Availability>().Available = true;

                structureDuplicate = null;

                allowedReplicates--;
                UICounter.text = allowedReplicates.ToString();
            }
        }


        if (mode == EControlBeamMode.IDLE)
        {
            beam.SetLines(mode);

            return false;
        }


        //************ Operation logic **************//

        if (mode == EControlBeamMode.SCANNING)
        {
            beam.SetLines(mode);

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                //structureDuplicate = Instantiate<GameObject>(structureHit.collider.gameObject, 
                //structureHit.collider.gameObject.transform.position,
                //structureHit.collider.gameObject.transform.rotation);


                string structureSceneName = structureHit.collider.gameObject.transform.root.name;

                Debug.Log("Replicator: Raw gameObject name: " + structureSceneName);

                for (int i = 0; i < structureSceneName.Length; ++i)
                {
                    if (structureSceneName[i] != ' ') structurePrefabName += structureSceneName[i];
                    else break;
                }

                    structureDuplicate = Realtime.Instantiate(structurePrefabName,
                                                          ownedByClient: true,
                                                          preventOwnershipTakeover: false,
                                                          destroyWhenOwnerOrLastClientLeaves: true,
                                                          useInstance: realtime);

                structureSceneName = structurePrefabName = "";

                structureDuplicate.GetComponent<RealtimeTransform>().RequestOwnership();

                structureCollider = structureDuplicate.GetComponent<Collider>();

                if (structureCollider) structureCollider.enabled = false;

                else
                {
                    structureCollider = structureDuplicate.GetComponentInChildren<Collider>();

                    if (structureCollider) structureCollider.enabled = false;
                }


                structureDuplicate.transform.position = structureHit.collider.gameObject.transform.position;
                structureDuplicate.transform.rotation = structureHit.collider.gameObject.transform.rotation;
                

                structureDuplicateRB = structureDuplicate.GetComponent<Rigidbody>();

                
                beam.SetStructureTransform(structureDuplicate.transform);


                mode = EControlBeamMode.CONTROLLING;
                beam.SetVisuals(mode);
                
            }
        }

        else if (mode == EControlBeamMode.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            beam.SetControlForce(controlForce);
            beam.SetLines(mode);

            //Movement
            structureDuplicateRB.AddForce(controlForce.normalized * 2);

        }

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
