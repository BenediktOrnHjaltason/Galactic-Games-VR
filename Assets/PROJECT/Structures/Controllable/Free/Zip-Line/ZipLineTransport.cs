using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZipLineTransport : MonoBehaviour
{
    public event Action OnBeamTouchesObstacle;

    Transform startPointTransform;
    public Transform StartPointTransform { set => startPointTransform = value; get => startPointTransform; }

    Transform endPointTransform;
    public Transform EndPointTransform { set => endPointTransform = value; get => endPointTransform; }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(16) || other.gameObject.layer.Equals(10))
        {
            OnBeamTouchesObstacle?.Invoke();
        }
    }
}
