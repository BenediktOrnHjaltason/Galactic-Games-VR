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


    /// <summary>
    /// Prefab to spawn
    /// </summary>
    [SerializeField]
    GameObject PF_TransportLine;

    [SerializeField]
    GameObject otherPoint;

    GameObject transportLine;

    ZipLineTransport zipLineTransport;

    //Vector3 startToEnd;
    Vector3 selfToOther;


    RealtimeTransform rtt;
    


    // Start is called before the first frame update
    void Start()
    {
        if (point == EZipLine.START)
        {
            transportLine = Instantiate<GameObject>(PF_TransportLine, transform);

            zipLineTransport = transportLine.GetComponent<ZipLineTransport>();

            if (transportLine && zipLineTransport) Debug.Log("ZipLinePoint::Start: We have reference to zipline object and script");
        }

        rtt = transform.root.GetComponent<RealtimeTransform>();
    }

    void FixedUpdate()
    {
        selfToOther =  otherPoint.transform.root.position - transform.root.position;

        if (transportLine)
        {
            //startToEnd = otherPoint.transform.position - transform.position;
            zipLineTransport.TransportDirection = selfToOther;

            //if (Vector3.Dot(startToEnd.normalized, transform.root.forward) > 0.96f && Vector3.Dot(-startToEnd.normalized, otherPoint.transform.forward) > 0.96f)
            //{
                transportLine.transform.SetPositionAndRotation(transform.position + (selfToOther / 2), Quaternion.LookRotation(selfToOther));
                transportLine.transform.localScale = new Vector3(0.25f, 0.25f, selfToOther.magnitude);
            //}

            //else
            //{
            //transportLine.transform.position = transform.position;
            //transportLine.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            //}

            
        }

        if (rtt.realtime.connected && rtt.isUnownedSelf) rtt.RequestOwnership();

        if (rtt.realtime.connected && rtt.isOwnedLocallySelf) transform.root.rotation = Quaternion.LookRotation(selfToOther);

    }
}
