using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

enum EOperationPhase
{
    DETECTALLIGNMENT,
    AKNOWLEDGE,
    MOVE
}

public class GhostPlatform : MonoBehaviour
{
    Vector3 leftHalfCenter;

    Vector3 rightHalfCenter;


    [SerializeField]
    RealtimeTransform platformRtt;

    RealtimeTransform thisRtt;

    [SerializeField]
    AnimationCurve easeInFastOut;

    [SerializeField]
    AnimationCurve detectionBump;


    Realtime realtime;

    Vector3 platformToGhost;

    float forwardAllignment;
    float rightAllignment;


    float increment = 0;

    Vector3 baseLocalScale;

    //false for left, true for right
    bool leftOrRight = false;

    Quaternion oldRotation;
    Quaternion newRotation;

    //----- Operation
    EOperationPhase phase = EOperationPhase.DETECTALLIGNMENT;



    void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        thisRtt = GetComponent<RealtimeTransform>();

        leftHalfCenter = platformRtt.transform.localPosition;
        rightHalfCenter = transform.localPosition;

        baseLocalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        
        //If we are not connected to server we cannon request ownerships
        if (!realtime.connected) return;


        //If nobody has network ownership of platform, someone needs to have ownership of ghost platform, so why not the first one to enter the room
        //Someone needs to have ownership of the Ghost Platform if we want to see the idle scaling effect
        else if (platformRtt.ownerIDSelf == -1)
        {

            //Will it break if client ID 0 leaves the room before anyone else?
            if (thisRtt.ownerIDSelf == 0) thisRtt.RequestOwnership();
        }

        //If someone DOES have ownership of platform, and we are still unowned
        else if (thisRtt.ownerIDSelf != platformRtt.ownerIDSelf) thisRtt.SetOwnership(platformRtt.ownerIDSelf);


        if (true)
        {
            switch (phase)
            {
                case EOperationPhase.DETECTALLIGNMENT:

                    transform.localScale = baseLocalScale + (Vector3.one * Mathf.Abs(Mathf.Sin(Time.time * 5)) / 5);

                    platformToGhost = transform.position - platformRtt.transform.position;

                    //Position match
                    if (platformToGhost.sqrMagnitude < 0.32f)
                    {
                        //Rotation match
                        forwardAllignment = Vector3.Dot(transform.forward, platformRtt.transform.forward);
                        rightAllignment = Vector3.Dot(transform.right, platformRtt.transform.right);

                        if ((forwardAllignment < -0.8f || forwardAllignment > 0.8f) && (rightAllignment < -0.8f || rightAllignment > 0.8f))
                        {
                            phase = EOperationPhase.AKNOWLEDGE;
                        }
                    }
                    break;

                case EOperationPhase.AKNOWLEDGE:

                    if (increment < 1)
                    {
                        increment += 0.05f;

                        transform.localScale = baseLocalScale + (Vector3.one * detectionBump.Evaluate(increment));
                    }

                    else if (increment > 1)
                    {
                        increment = 0;

                        transform.localScale = baseLocalScale;
                        oldRotation = transform.localRotation;
                        newRotation = Quaternion.Euler(new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)));

                        phase = EOperationPhase.MOVE;
                    }

                    break;

                case EOperationPhase.MOVE:

                    if (increment < 1)
                    {
                        if (leftOrRight) transform.localPosition = Vector3.Lerp(leftHalfCenter, rightHalfCenter, easeInFastOut.Evaluate(increment));
                        else transform.localPosition = Vector3.Lerp(rightHalfCenter, leftHalfCenter, easeInFastOut.Evaluate(increment));

                        transform.localRotation = Quaternion.Lerp(oldRotation, newRotation, increment);

                        increment += 0.05f;
                    }
                    else if (increment > 1)
                    {
                        phase = EOperationPhase.DETECTALLIGNMENT;
                        increment = 0;
                        leftOrRight = !leftOrRight;
                    }

                    break;
            }
        }
    }
}
