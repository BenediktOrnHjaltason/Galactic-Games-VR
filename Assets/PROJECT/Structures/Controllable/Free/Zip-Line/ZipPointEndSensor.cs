using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Types;

public class ZipPointEndSensor : MonoBehaviour
{
    ZipLinePoint ownerPoint;

    private void Start()
    {
        ownerPoint  = transform.root.gameObject.GetComponent<ZipLinePoint>();
        if (!ownerPoint) ownerPoint = transform.root.gameObject.GetComponentInChildren<ZipLinePoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10) || other.gameObject.layer.Equals(0))
        {
            ZipLinePoint pointEnteringTrigger = other.transform.root.GetComponent<ZipLinePoint>();
            if (!pointEnteringTrigger) pointEnteringTrigger = other.transform.root.GetComponentInChildren<ZipLinePoint>();

            if (pointEnteringTrigger && pointEnteringTrigger.OtherPoint != ownerPoint && pointEnteringTrigger.StartOrEnd == EZipLinePoint.END)
            {
                //Break previous connections
                if (pointEnteringTrigger.OtherPoint) pointEnteringTrigger.OtherPoint.BreakConnection();
                if (ownerPoint.OtherPoint) ownerPoint.OtherPoint.BreakConnection();

                //Make new connections
                ownerPoint.OtherPoint = pointEnteringTrigger;
                pointEnteringTrigger.OtherPoint = ownerPoint;

                if (ownerPoint.TransportBeam)
                    ownerPoint.TransportBeam.EndPointTransform = pointEnteringTrigger.transform;
            }
        }
    }
}
