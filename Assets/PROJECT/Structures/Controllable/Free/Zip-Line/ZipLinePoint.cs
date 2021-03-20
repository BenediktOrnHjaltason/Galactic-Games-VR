using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using UnityEditor;
using Normal.Realtime;

public class ZipLinePoint : MonoBehaviour
{
    //TODO?: Split up into two scripts inheriting from base?


    [SerializeField]
    EZipLine point;

    public EZipLine Point { get => point; }


    /// <summary>
    /// Prefab to spawn
    /// </summary>
    [SerializeField]
    GameObject PF_TransportLine;

    [SerializeField]
    GameObject otherPoint;

    public GameObject OtherPoint 
    {
        set
        {
            otherPoint = value;

            if (point == EZipLine.START)
            {
                transportLineActive = (value != null) ? true : false;

                if (value == null) transportLine.transform.localScale = Vector3.zero; 
            }
        }

        get => otherPoint;
    }

    GameObject transportLine;

    ZipLineTransport transportBeam;

    Vector3 selfToOther;


    RealtimeTransform rtt;
    


    // Start is called before the first frame update
    void Start()
    {
        if (point == EZipLine.START)
        {
            transportLine = Instantiate<GameObject>(PF_TransportLine, transform);

            transportBeam = transportLine.GetComponent<ZipLineTransport>();
            transportBeam.OnBeamTouchesObstacle += BreakConnection;

            transportBeam.StartPointTransform = transform.root;

            if (otherPoint)
            {
                transportBeam.EndPointTransform = otherPoint.transform;
                transportLineActive = true;
            }
        }

        rtt = transform.root.GetComponent<RealtimeTransform>();
    }

    void FixedUpdate()
    {
        if (otherPoint) selfToOther = otherPoint.transform.root.position - transform.root.position;

        //Start-points handles transport line
        if (transportLine && otherPoint)
        {
            if (transportLineActive)
            {
                transportBeam.TransportDirection = selfToOther;


                transportLine.transform.SetPositionAndRotation(transform.position + (selfToOther / 2), Quaternion.LookRotation(selfToOther));
                transportLine.transform.localScale = new Vector3(0.25f, 0.25f, selfToOther.magnitude);
            }
            else
            {
                transportLine.transform.position = transform.position;
            }
        }

        if (rtt.realtime.connected && rtt.ownerIDSelf == -1) rtt.RequestOwnership();

        if (otherPoint && rtt.realtime.connected && rtt.isOwnedLocallySelf) transform.root.rotation = Quaternion.LookRotation(selfToOther);

    }


    //Only for Start-points

    bool transportLineActive = false;

    void BreakConnection(GameObject otherCollidingObject)
    {

        //if (otherCollidingObject == this) return;

        

        ZipLinePoint zp = otherPoint.transform.root.GetComponentInChildren<ZipLinePoint>();

        if (zp)
        {
            zp.OtherPoint = null;
            OtherPoint = null;

            transportLineActive = false;
        }
    }
}
