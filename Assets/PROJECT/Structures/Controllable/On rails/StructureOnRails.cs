using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using System;
using Normal.Realtime;

public class StructureOnRails : MonoBehaviour
{
    [Header("NOTE: Script overrides Rigidbody constraints on Start")]
    [SerializeField]
    EPlatformMoveGlobal selfAttractDirection;

    Rigidbody RB;

    Vector3 moveForce;

    Realtime realtime;

    RealtimeTransform realtimeRtt;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
        realtimeRtt = GetComponent<RealtimeTransform>();



        switch(selfAttractDirection)
        {
            case EPlatformMoveGlobal.X_Positive:
                moveForce = new Vector3(5, 0, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation;
                break;

            case EPlatformMoveGlobal.X_Negative:
                moveForce = new Vector3(-5, 0, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation;
                break;


            case EPlatformMoveGlobal.Y_Positive:
                moveForce = new Vector3(0, 5, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezeRotation;
                break;

            case EPlatformMoveGlobal.Y_Negative:
                moveForce = new Vector3(0, -5, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezeRotation;
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (realtime.connected && realtimeRtt.ownerIDSelf == realtime.clientID)
        RB.AddForce(moveForce * 40 * Time.deltaTime);
    }

}
