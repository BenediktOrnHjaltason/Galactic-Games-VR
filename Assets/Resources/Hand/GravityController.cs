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

    bool pushingForward;
    bool pushingBackward;

    bool rotating_Pitch; //Relative to player right
    bool rotating_Roll; //Relative to player forward

    Vector2 stickInput;

    Vector3 controlForce;


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
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            mode = EMode.SCANNING;

        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            mode = EMode.NONE;


        if (mode == EMode.NONE)
        {
            SetLineRenderer(mode);

            return false;
        }


        if (OVRInput.GetDown(OVRInput.Button.One))
            pushingBackward = true;
        else if (OVRInput.GetUp(OVRInput.Button.One))
            pushingBackward = false;

        if (OVRInput.GetDown(OVRInput.Button.Two))
            pushingForward = true;
        else if (OVRInput.GetUp(OVRInput.Button.Two))
            pushingForward = false;


        stickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (stickInput.x > 0.1 || stickInput.x < -0.1) rotating_Roll = true;
        else rotating_Roll = false;

        if (stickInput.y > 0.1 || stickInput.y < -0.1) rotating_Pitch = true;
        else rotating_Pitch = false;


        //************ Operation logic **************//

        if (mode == EMode.SCANNING )
        {
            SetLineRenderer(mode);

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                structure = structureHit.collider.gameObject;

                structureRB = structure.GetComponent<Rigidbody>();

                mode = EMode.CONTROLLING;
            }
        }

        else if (mode == EMode.CONTROLLING)
        {
            controlForce = CalculateControlForce();

            SetLineRenderer(mode);

            //Movement
            structureRB.AddForce(controlForce);

            //Rotation
            if (rotating_Roll) structure.transform.rotation *= Quaternion.AngleAxis((stickInput.x * -1) / 2, playerRoot.transform.forward);

            if (rotating_Pitch) structure.transform.rotation *= Quaternion.AngleAxis(stickInput.y / 2, playerRoot.transform.right);

        }

        return true;
    }

    Vector3 CalculateControlForce()
    {
        float distanceToStructure = (transform.position - structure.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structure.transform.position;

        float forwardMultiplyer = (pushingForward) ? 7.0f : 0.0f;
        forwardMultiplyer += (pushingBackward) ? -7.0f : 0.0f;

        //structureToAdjustedForward projected on plane made up of avatar right and up vectors, represented by avatar forward vector
        return (structureToAdjustedForward - ((Vector3.Dot(structureToAdjustedForward, playerRoot.transform.forward)) * playerRoot.transform.forward))
                                + transform.forward * forwardMultiplyer;
    }

    void SetLineRenderer(EMode mode)
    {
        switch (mode) 
        {
            case EMode.NONE:

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position);
                break;

            case EMode.SCANNING:

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position + transform.forward * 1000);
                break;

            case EMode.CONTROLLING:

                line.SetPosition(0, transform.position);
                line.SetPosition(1, structure.transform.position + controlForce);
                line.SetPosition(2, structure.transform.position);
                break;
        }
    }
}
