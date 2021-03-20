using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZipLineEndSensor : MonoBehaviour
{
    ZipLinePoint ownerPoint;

    private void Start()
    {
        ownerPoint  = transform.root.gameObject.GetComponentInChildren<ZipLinePoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {
            ZipLinePoint endPoint = other.gameObject.GetComponentInChildren<ZipLinePoint>();

            if (endPoint.OtherPoint) endPoint.OtherPoint.GetComponentInChildren<ZipLinePoint>().BreakConnection();

            ZipLineTransport transport = transform.root.GetComponentInChildren<ZipLineTransport>();

            if (endPoint && endPoint.Point == Types.EZipLine.END && transport)
            {
                if (ownerPoint.OtherPoint) ownerPoint.OtherPoint.transform.root.GetComponentInChildren<ZipLinePoint>().OtherPoint = null;

                ownerPoint.OtherPoint = endPoint.transform.root.gameObject;
                endPoint.OtherPoint = transform.root.gameObject;

                transport.EndPointTransform = endPoint.transform;
            }
        }
    }
}
