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

    [SerializeField]
    MeshRenderer mesh;

    Material[] materials;

    string graphVariableScrollDirection = "Vector2_BE6D9D07";


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

        materials = mesh.materials;

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

                materials[1].SetVector("Vector2_BE6D9D07", new Vector2(0, -1));
                 break;

            case EAutoMoveDirection.Y_Negative:
                 //resetForceVector = new Vector3(0, -resetForcePower, 0);
                 autoForceVector = new Vector3(0, -autoForcePower, 0);
                 RB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation;
                materials[1].SetVector("Vector2_BE6D9D07", new Vector2(0, 1));
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
        //Platforms in Free mode is allowed to loose ownership when sleeping because they will be static 
        if (realtime.connected && mode == ERailsMode.AutoForce)
        {
            if (realtimeTransform.ownerIDSelf == -1) realtimeTransform.RequestOwnership();

            if (realtimeTransform.ownerIDSelf == realtime.clientID)
                RB.AddForce(autoForceVector);
        }
        

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
