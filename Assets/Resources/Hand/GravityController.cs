using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{

    [SerializeField]
    GameObject playerRoot;

    LineRenderer line;

    RaycastHit structureHit;
    GameObject structure;
    Rigidbody structureRB;

    float distanceToStructure;

    bool pushingForward;
    bool pushingBackward;

    Vector3 controlForceVector;


    enum EMode
    {
        NONE,
        SCANNING,
        CONTROLLING
    } EMode mode = EMode.NONE;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    public bool Operating()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            mode = EMode.SCANNING;

        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            mode = EMode.NONE;


        if (OVRInput.GetDown(OVRInput.Button.One))
            pushingBackward = true;
        else if (OVRInput.GetUp(OVRInput.Button.One))
            pushingBackward = false;

        if (OVRInput.GetDown(OVRInput.Button.Two))
            pushingForward = true;
        else if (OVRInput.GetUp(OVRInput.Button.Two))
            pushingForward = false;



        //************ Operation logic **************//

        if (mode == EMode.NONE)
        {
            SetLineRendererInactive();
            return false;
        }

        else SetLineRendererActive(mode);


        if (mode == EMode.SCANNING )
        {
            
            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                structure = structureHit.collider.gameObject;

                structureRB = structure.GetComponent<Rigidbody>();

                mode = EMode.CONTROLLING;
            }
        }

        else if (mode == EMode.CONTROLLING)
        {
            structureRB.AddForce(controlForceVector);
        }

        return true;
    }

    void SetLineRendererActive(EMode mode)
    {
        switch (mode)
        {
            case EMode.SCANNING:
                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position + transform.forward * 1000);
                break;

            case EMode.CONTROLLING:

                distanceToStructure = (transform.position - structure.transform.position).magnitude;

                Vector3 adjustedForward = transform.forward * distanceToStructure;

                Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structure.transform.position;

                float forwardMultiplyer = (pushingForward) ? 7.0f : 0.0f;
                forwardMultiplyer += (pushingBackward) ? -7.0f : 0.0f;

                //structureToAdjustedForward projected on plane made up of avatar right and up vectors, represented by avatar forward vector
                controlForceVector = (structureToAdjustedForward - ((Vector3.Dot(structureToAdjustedForward, playerRoot.transform.forward)) * playerRoot.transform.forward))
                                        + transform.forward * forwardMultiplyer;


                line.SetPosition(0, transform.position);
                line.SetPosition(1, structure.transform.position + controlForceVector);
                line.SetPosition(2, structure.transform.position);
                break;
        }
    }

    void SetLineRendererInactive()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
        line.SetPosition(2, transform.position);
    }
}
