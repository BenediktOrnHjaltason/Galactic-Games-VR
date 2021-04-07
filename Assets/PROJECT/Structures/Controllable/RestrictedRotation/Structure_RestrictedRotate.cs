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
    float autoForcePower = 10;

    Vector3 autoRotateVector;

    Vector3 rootPositionOnStart;
    Vector3 forwardOnStart;

    Vector3 worldX = new Vector3(1, 0, 0);
    Vector3 worldZ = new Vector3(0, 0, 1);

    [SerializeField]
    ERestrictedRotation exclusiveWorldRotation;

    [SerializeField]
    EAutoRotateDirection autoRotateDirection;

    [SerializeField]
    EPlayerRotationAllowed playerRotationAllowed;

    [SerializeField]
    float playerForceMultiplier = 0.4f;

    int playerAngleModifier = 1;

    public override void CalculatePlayerAngleModifier(Vector3 controllingHandPosition)
    {
        Vector3 controllingHandToThis = (transform.position - controllingHandPosition).normalized;

        if (exclusiveWorldRotation == ERestrictedRotation.X)
                playerAngleModifier = (Vector3.Dot(worldX, controllingHandToThis) > 0) ? 1 : -1;

        else 
        if (exclusiveWorldRotation == ERestrictedRotation.Z)
                playerAngleModifier = (Vector3.Dot(worldZ, controllingHandToThis) > 0) ? 1 : -1;
    }


    protected override void Awake()
    {
        base.Awake();

        rtt = GetComponent<RealtimeTransform>();
        rootPositionOnStart = transform.root.position;
        forwardOnStart = transform.forward;

        switch(exclusiveWorldRotation)
        {
            case ERestrictedRotation.X:
                {
                    autoRotateVector = new Vector3(autoRotateDirection == EAutoRotateDirection.POSITIVE ? autoForcePower : -autoForcePower, 0, 0);

                    if (!AllowGravityForceByPlayer)
                        rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePosition;
                    else rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    break;
                }

            case ERestrictedRotation.Y:
                {
                    autoRotateVector = new Vector3(0, autoRotateDirection == EAutoRotateDirection.POSITIVE ? autoForcePower : -autoForcePower, 0);

                    if (!AllowGravityForceByPlayer)
                        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePosition;

                    else rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    break;
                }
            case ERestrictedRotation.Z:
                {
                    autoRotateVector = new Vector3(0, 0, autoRotateDirection == EAutoRotateDirection.POSITIVE ? autoForcePower : -autoForcePower);

                    if (!AllowGravityForceByPlayer)
                        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePosition;
                    else rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                    break;
                }
        }
    }

    protected override void FixedUpdate()
    {
        if (rtt.realtime.connected && mode == ERestrictedStructureMode.AutoForce)
        {
            if (rtt.ownerIDSelf == -1) rtt.RequestOwnership();

            else if (rtt.ownerIDSelf == realtime.clientID) 
                rb.AddTorque(autoRotateVector);
        }
    }

    public override void Rotate(Vector3 playerForward, float rollForce, float yawForce, Vector3 playerRight, float pitchForce)
    {

        switch (exclusiveWorldRotation)
        {
            case ERestrictedRotation.X:
                {
                    if (playerRotationAllowed == EPlayerRotationAllowed.Pitch)
                        rb.AddTorque(worldX * pitchForce * playerForceMultiplier * playerAngleModifier, ForceMode.Acceleration);

                    else if (playerRotationAllowed == EPlayerRotationAllowed.Roll)
                        rb.AddTorque(worldX * rollForce * playerForceMultiplier * playerAngleModifier, ForceMode.Acceleration);

                    break;
                }

            case ERestrictedRotation.Z:
                {
                    if (playerRotationAllowed == EPlayerRotationAllowed.Pitch)
                        rb.AddTorque(worldZ * pitchForce * playerForceMultiplier * playerAngleModifier, ForceMode.Acceleration);

                    else if (playerRotationAllowed == EPlayerRotationAllowed.Roll)
                        rb.AddTorque(worldZ * rollForce * playerForceMultiplier * playerAngleModifier, ForceMode.Acceleration);

                    break;
                }

            case ERestrictedRotation.Y:
                {
                    if (playerRotationAllowed == EPlayerRotationAllowed.Yaw)
                        rb.AddTorque(Vector3.up * yawForce * playerForceMultiplier * playerAngleModifier, ForceMode.Acceleration);

                    break;
                }

        }

        /*
        if (!restrictRoll)
            rb.AddTorque(selfAxisToRotation.Roll * rollForce * rotationForceMultiplier * angleModifier, ForceMode.Acceleration);

        if (!restrictYaw)
            rb.AddTorque(selfAxisToRotation.Yaw * yawForce * rotationForceMultiplier * angleModifier, ForceMode.Acceleration);
        */
        //Pitch
        //if (!restrictPitch)   RB.AddRelativeTorque(transform.right * pitchForce);
    }
}
