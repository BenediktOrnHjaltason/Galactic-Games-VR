using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class DysonSpherePiece : MonoBehaviour
{
    [SerializeField]
    LineRenderer beam;

    [SerializeField]
    RealtimeTransform rtt;

    public RealtimeTransform Rtt { get => rtt; }

    float beamLength = 1000;


    private void FixedUpdate()
    {
        //beam.SetPositions
    }

}
