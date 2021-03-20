using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZipLineTransport : MonoBehaviour
{
    public event Action<GameObject> OnBeamTouchesObstacle;

    Transform startPointTransform;
    public Transform StartPointTransform { set => startPointTransform = value; get => startPointTransform; }

    Transform endPointTransform;
    public Transform EndPointTransform { set => endPointTransform = value; get => endPointTransform; }

    Vector3 transportDirection;
    public Vector3 TransportDirection { get => transportDirection;  set => transportDirection = value; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(16) || other.gameObject.layer.Equals(0) || other.gameObject.layer.Equals(10))
        {
            OnBeamTouchesObstacle?.Invoke(other.gameObject);
        }
    }
}
