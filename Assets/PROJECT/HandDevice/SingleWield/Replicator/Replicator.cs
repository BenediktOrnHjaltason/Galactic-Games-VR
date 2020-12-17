using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class Replicator : HandDevice
{

    [SerializeField]
    Material InactiveMaterial;

    [SerializeField]
    Material ActiveMaterial;

    ControllingBeam beam;

    RaycastHit structureHit;
    GameObject structureDuplicate;

    float distanceToStructure;

    EControlBeamMode mode = EControlBeamMode.IDLE;

    Vector3 controlForce;


    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponentInChildren<ControllingBeam>();
        beam.SetMaterialReferences(ActiveMaterial, InactiveMaterial);
    }


    public override bool Using()
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            mode = EControlBeamMode.SCANNING;

            beam.SetVisuals(mode);
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            mode = EControlBeamMode.IDLE;
            if (structureDuplicate) structureDuplicate.GetComponent<BoxCollider>().enabled = true;
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
                structureDuplicate = Instantiate<GameObject>(structureHit.collider.gameObject, 
                                                             structureHit.collider.gameObject.transform.position,
                                                             structureHit.collider.gameObject.transform.rotation);

                structureDuplicate.GetComponent<BoxCollider>().enabled = false;
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
            structureDuplicate.transform.position += controlForce.normalized * 0.02f;

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
}
