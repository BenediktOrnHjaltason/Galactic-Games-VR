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

            if (endPoint && endPoint.Point == Types.EZipLine.END)
            {
                ownerPoint.OtherPoint = endPoint.transform.root.gameObject;
                endPoint.OtherPoint = transform.root.gameObject;
            }
        }
    }


}
