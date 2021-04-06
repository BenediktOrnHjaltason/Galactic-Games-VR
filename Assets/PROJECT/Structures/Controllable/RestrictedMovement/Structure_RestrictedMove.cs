using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Types;



public class Structure_RestrictedMove : StructureSync
{
    [SerializeField]
    ERestrictedStructureMode mode = ERestrictedStructureMode.Free;

    [Header("NOTE: Script overrides Rigidbody constraints on Start.")]
    [SerializeField]
    EAutoForceAxis moveDirection;

    [SerializeField]
    float autoForcePower = 10;

    Vector3 autoMoveVector;

    [SerializeField]
    MeshRenderer mesh;

    Material[] materials;

    string graphVariableScrollGlow = "Vector1_CDE54C4F";
    int scrollGlowId;

    string graphVariableScrollDirection = "Vector2_BE6D9D07";

    //References
    //Realtime realtime;
    RealtimeTransform realtimeTransform;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        realtimeTransform = GetComponent<RealtimeTransform>();

        materials = mesh.materials;

        scrollGlowId = materials[1].shader.FindPropertyIndex(graphVariableScrollGlow);

        switch (moveDirection)
        {
            case EAutoForceAxis.X_Positive:
                 autoMoveVector = new Vector3(autoForcePower, 0, 0);
                 rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;
                 break;

            case EAutoForceAxis.X_Negative:
                 autoMoveVector = new Vector3(-autoForcePower, 0, 0);
                 rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;
                 break;


            case EAutoForceAxis.Y_Positive:
                 autoMoveVector = new Vector3(0, autoForcePower, 0);
                 rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;

                materials[1].SetVector(graphVariableScrollDirection, new Vector2(0, -1));
                 break;

            case EAutoForceAxis.Y_Negative:
                 autoMoveVector = new Vector3(0, -autoForcePower, 0);
                 rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation;
                materials[1].SetVector(graphVariableScrollDirection, new Vector2(0, 1));
                break;

            case EAutoForceAxis.Z_Positive:
                 autoMoveVector = new Vector3(0, 0, autoForcePower);
                 rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezeRotation;
                    break;

            case EAutoForceAxis.Z_Negative:
                autoMoveVector = new Vector3(0, 0, autoForcePower);
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY |
                   RigidbodyConstraints.FreezeRotation;
                break;
        }
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleSideGlow();

        //Platforms in Free mode is allowed to loose ownership when sleeping because they will be static 
        if (rtt.realtime.connected && mode == ERestrictedStructureMode.AutoForce)
        {
            if (realtimeTransform.ownerIDSelf == -1) realtimeTransform.RequestOwnership();

            if (realtimeTransform.ownerIDSelf == realtime.clientID && availableToManipulate)
                rb.AddForce(autoMoveVector);
        }
    }

    void HandleSideGlow()
    {
        if (!availableToManipulate)
        {
            if (rtt.isOwnedLocallySelf)
            {
                //Increment float opacity
                if (sideGlowOpacity < 1) sideGlowOpacity = model.sideGlowOpacity += 0.1f;

                //set glow opacity
                materials[1].SetFloat(scrollGlowId, sideGlowOpacity);
            }
            else
            {
                sideGlowOpacity = model.sideGlowOpacity;

                //set glow opacity
                materials[1].SetFloat(scrollGlowId, sideGlowOpacity);
            }
        }

        else if (rtt.isOwnedLocallySelf && sideGlowOpacity > 0.6f)
        {
            sideGlowOpacity = model.sideGlowOpacity -= 0.01f;

            //set glow opacity
            materials[1].SetFloat(scrollGlowId, sideGlowOpacity);

        }

        else if (rtt.isOwnedRemotelySelf && sideGlowOpacity > 0.6f)
        {
            //set glow opacity
            materials[1].SetFloat(scrollGlowId, model.sideGlowOpacity);
        }
    }
}
