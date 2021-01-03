using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using TMPro;

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

    ControllingBeam beam;

    RaycastHit structureHit;
    GameObject structureDuplicate;

    Rigidbody structureDuplicateRB;

    float distanceToStructure;

    EControlBeamMode mode = EControlBeamMode.IDLE;

    Vector3 controlForce;

    OVRInput.Button grabbingControllerIndexTrigger;


    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponentInChildren<ControllingBeam>();
        beam.SetMaterialReferences(ActiveMaterial, InactiveMaterial);
        beam.SetLines(mode);

        RB = GetComponent<Rigidbody>();

        UICounter = GetComponentInChildren<TextMeshPro>();
        UICounter.text = allowedReplicates.ToString();
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
                structureDuplicate.GetComponent<Collider>().enabled = true;

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

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10) &&
                structureHit.collider.gameObject.GetComponent<Availability>().Available)
            {
                structureDuplicate = Instantiate<GameObject>(structureHit.collider.gameObject, 
                                                             structureHit.collider.gameObject.transform.position,
                                                             structureHit.collider.gameObject.transform.rotation);

                structureHit.collider.gameObject.GetComponent<Availability>().Available = false;
                structureDuplicate.GetComponent<Availability>().Available = false;


                structureDuplicateRB = structureDuplicate.GetComponent<Rigidbody>();

                structureDuplicate.GetComponent<Collider>().enabled = false;
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
