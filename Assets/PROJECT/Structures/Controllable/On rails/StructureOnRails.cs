using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

enum EAutoMoveDirection
{
    X_Positive,
    X_Negative,
    Y_Positive,
    Y_Negative,
    Z_Positive,
    Z_Negative
}

enum ERailsMode
{
    Free,
    AutoForce
    //ResetPeriodically
}

public class StructureOnRails : MonoBehaviour
{
    //----Properties

    [SerializeField]
    ERailsMode mode = ERailsMode.Free;

    [Header("NOTE: Script overrides Rigidbody constraints on Start.")]
    [Tooltip("Use for both AUTOFORCE and FREE_RESETPERIODICALLY")]
    [SerializeField]
    EAutoMoveDirection moveDirection;

    [SerializeField]
    float autoForcePower = 10;
    //float resetForcePower = 1000;

    Vector3 autoForceVector;
    //Vector3 resetForceVector;

    //[SerializeField]
    //float resetIntervalSeconds = 120;

    //float resetIncementer = 0;
    //bool reset = false;

    //float time = 0;


    //References
    Realtime realtime;
    RealtimeTransform realtimeTransform;
    Rigidbody RB;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
        realtimeTransform = GetComponent<RealtimeTransform>();

        switch(moveDirection)
        {
            case EAutoMoveDirection.X_Positive:
                 //resetForceVector = new Vector3(resetForcePower, 0, 0);
                 autoForceVector = new Vector3(autoForcePower, 0, 0);
                 RB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;
                 break;

            case EAutoMoveDirection.X_Negative:
                 //resetForceVector = new Vector3(-resetForcePower, 0, 0);
                 autoForceVector = new Vector3(-autoForcePower, 0, 0);
                 RB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;
                 break;


            case EAutoMoveDirection.Y_Positive:
                 //resetForceVector = new Vector3(0, resetForcePower, 0);
                 autoForceVector = new Vector3(0, autoForcePower, 0);
                 RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;
                 break;

            case EAutoMoveDirection.Y_Negative:
                 //resetForceVector = new Vector3(0, -resetForcePower, 0);
                 autoForceVector = new Vector3(0, -autoForcePower, 0);
                 RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation;
                 break;

            case EAutoMoveDirection.Z_Positive:
                 //resetForceVector = new Vector3(0, 0, resetForcePower);
                 autoForceVector = new Vector3(0, 0, autoForcePower);
                 RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezeRotation;
                    break;

            case EAutoMoveDirection.Z_Negative:
                //resetForceVector = new Vector3(0, 0, resetForcePower);
                autoForceVector = new Vector3(0, 0, autoForcePower);
                RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY |
                   RigidbodyConstraints.FreezeRotation;
                break;
            }
    }


    void FixedUpdate()
    {

        if (!realtime.connected) return;

        if (realtimeTransform.ownerIDSelf == -1) realtimeTransform.RequestOwnership();

        if (mode == ERailsMode.AutoForce && realtimeTransform.ownerIDSelf == realtime.clientID)
            RB.AddForce(autoForceVector);
        

        

        /*
         time += Time.fixedDeltaTime;
        if (time > resetIntervalSeconds)
        {
            time = 0;
            reset = true;
        }
         
        else if (mode == ERailsMode.ResetPeriodically && realtime.connected)
        {
            if (realtimeTransform.ownerIDSelf == -1) realtimeTransform.SetOwnership(0);

            if (reset)
            {
                RB.AddForce(resetForceVector);
                reset = false;
            }
        }
        */
    }
}
