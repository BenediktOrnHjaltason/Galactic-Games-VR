using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using System;
using Normal.Realtime;

public class StructureOnRails : MonoBehaviour
{
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
            case EPlatformMoveGlobal.Y_Positive:
                moveForce = new Vector3(0, 100, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezeRotation;
                break;

            case EPlatformMoveGlobal.Y_Negative:
                moveForce = new Vector3(0, -100, 0);
                RB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezeRotation;
                break;
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (realtime.connected && realtimeRtt.ownerIDSelf == realtime.clientID)
        RB.AddForce(moveForce);
    }

}
