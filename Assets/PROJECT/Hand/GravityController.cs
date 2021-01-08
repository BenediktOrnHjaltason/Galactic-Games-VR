using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class GravityController : HandDevice
{

    [SerializeField]
    GameObject playerRoot;

    [SerializeField]
    Material InactiveMaterial;

    [SerializeField]
    Material ActiveMaterial;


    MeshRenderer mesh;

    ControllingBeam beam;

    RaycastHit structureHit;
    GameObject structureRoot;
    Rigidbody structureRB;
    Availability structureAvailability;

    bool pushingForward;
    bool pushingBackward;

    bool rotating_Pitch; //Relative to player right
    bool rotating_Roll; //Relative to player forward
    bool rotating_Yaw = false;

    Vector2 stickInput;

    Vector3 controlForce;

    Vector3 Up = new Vector3(0, 1, 0);

    float distanceToStructure;


    EControlBeamMode mode = EControlBeamMode.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        beam = GetComponentInChildren<ControllingBeam>();
        beam.SetMaterialReferences(ActiveMaterial, InactiveMaterial);
    }

    public override bool Using() 
    {
        //************ Manage input **************//

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            mode = EControlBeamMode.SCANNING;
            rotating_Yaw = false;

            SetVisuals(mode);
            beam.SetVisuals(mode);
        }


        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            mode = EControlBeamMode.IDLE;

            structureRoot = null;
            structureRB = null;
            structureAvailability = null;

            SetVisuals(mode);
        }


        if (mode == EControlBeamMode.IDLE)
        {
            beam.SetLines(mode);

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

        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
            rotating_Yaw = !rotating_Yaw;


        //************ Operation logic **************//

        if (mode == EControlBeamMode.SCANNING )
        {
            beam.SetLines(mode);

            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                structureRoot = structureHit.collider.gameObject.transform.root.gameObject;

                //Networking (Making sure no-one else can manipulate structure at the same time)
                structureAvailability = structureRoot.GetComponent<Availability>();
                RealtimeTransform rtt = structureRoot.GetComponent<RealtimeTransform>();

                rtt.RequestOwnership();

                //Take ownership of subobjects, if any
                LocalState ls = structureRoot.GetComponent<LocalState>();

                if (ls) for (int i = 0; i < ls.GetSubObjects().Count; i++) ls.GetSubObjects()[i].GetComponent<RealtimeTransform>().RequestOwnership();


                beam.SetStructureTransform(structureRoot.transform);

                structureRB = structureRoot.GetComponent<Rigidbody>();

                mode = EControlBeamMode.CONTROLLING;
                SetVisuals(mode);
                beam.SetVisuals(mode);
            }
        }

        else if (mode == EControlBeamMode.CONTROLLING)
        {
            controlForce = CalculateControlForce();
            beam.SetControlForce(controlForce);
            beam.SetLines(mode);

            if (structureAvailability.Available)
            {
                //Movement
                structureRB.AddForce(controlForce);

                //Rotation
                if (rotating_Yaw) structureRoot.transform.Rotate(Up, (stickInput.x * -1) / 2, Space.World);

                else
                {
                    if (rotating_Roll) structureRoot.transform.Rotate(playerRoot.transform.forward, ((stickInput.x * -1) / 2), Space.World);

                    if (rotating_Pitch) structureRoot.transform.Rotate(playerRoot.transform.right, stickInput.y / 2, Space.World);
                }
            }
        }

        return true;
    }

    Vector3 CalculateControlForce()
    {
        distanceToStructure = (transform.position - structureRoot.transform.position).magnitude;

        Vector3 adjustedForward = transform.forward * distanceToStructure;

        Vector3 structureToAdjustedForward = (transform.position + adjustedForward) - structureRoot.transform.position;

        float forwardMultiplyer = (pushingForward) ? 7.0f : 0.0f;
        forwardMultiplyer += (pushingBackward) ? -7.0f : 0.0f;

        return (structureToAdjustedForward + transform.forward * forwardMultiplyer);
    }

    void SetVisuals(EControlBeamMode mode)
    {
        switch (mode)
        {
            case EControlBeamMode.IDLE:

                mesh.material = InactiveMaterial;
                break;

            case EControlBeamMode.SCANNING:
                mesh.material = InactiveMaterial;
                break;

            case EControlBeamMode.CONTROLLING:
                mesh.material = ActiveMaterial;
                break;
        }
    }

    public override void Equip(EHandSide hand)
    {
        //Nothing necessary here for this class
    }
}
