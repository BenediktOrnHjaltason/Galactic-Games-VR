using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class Structure_RestrictedRotate : StructureSync
{
    [SerializeField]
    ERestrictedStructureMode mode = ERestrictedStructureMode.Free;

    [Header("NOTE: Script overrides Rigidbody constraints on Start.")]
    [SerializeField]
    EAutoForceAxis autoForceAxis;

    [SerializeField]
    float autoForcePower = 10;

    Vector3 autoRotateVector;

    Vector3 rootPositionOnStart;
    Vector3 forwardOnStart;


    protected override void Start()
    {
        base.Start();

        rtt = GetComponent<RealtimeTransform>();
        rootPositionOnStart = transform.root.position;
        forwardOnStart = transform.forward;

        switch(autoForceAxis)
        {
            case EAutoForceAxis.X_Positive:
                autoRotateVector = new Vector3(autoForcePower, 0, 0);
                rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ |
                    RigidbodyConstraints.FreezePosition;
                break;

            case EAutoForceAxis.X_Negative:
                autoRotateVector = new Vector3(-autoForcePower, 0, 0);
                rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ |
                    RigidbodyConstraints.FreezePosition;
                break;


            case EAutoForceAxis.Y_Positive:
                autoRotateVector = new Vector3(0, autoForcePower, 0);
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ |
                    RigidbodyConstraints.FreezePosition;
                break;

            case EAutoForceAxis.Y_Negative:
                autoRotateVector = new Vector3(0, -autoForcePower, 0);
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ |
                   RigidbodyConstraints.FreezePosition;
                break;

            case EAutoForceAxis.Z_Positive:
                autoRotateVector = new Vector3(0, 0, autoForcePower);
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                   RigidbodyConstraints.FreezePosition;
                break;

            case EAutoForceAxis.Z_Negative:
                autoRotateVector = new Vector3(0, 0, autoForcePower);
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                   RigidbodyConstraints.FreezePosition;
                break;
        }
    }

    protected override void FixedUpdate()
    {
        if (rtt.realtime.connected && mode == ERestrictedStructureMode.AutoForce)
        {
            if (rtt.ownerIDSelf == -1) rtt.RequestOwnership();

            if (rtt.ownerIDSelf == realtime.clientID)
            {
                rb.AddTorque(forwardOnStart * autoForcePower);
                //transform.Rotate(autoRotateVector);
                //rb.transform.position = rootPositionOnStart;
            }
        }
    }
}
