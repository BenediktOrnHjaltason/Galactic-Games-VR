using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class MovingGrabHandle : GrabHandle
{
    [SerializeField]
    Vector3 startPosition;

    [SerializeField]
    Vector3 endPosition;

    [SerializeField]
    float speed = 1;

    RealtimeTransform rtt;

    // Start is called before the first frame update
    void Start()
    {
        rtt = GetComponent<RealtimeTransform>();
    }

    private void FixedUpdate()
    {
        if (!rtt.realtime.connected) return;

        if (rtt.ownerIDSelf == -1) rtt.SetOwnership(rtt.realtime.clientID);
        else if (rtt.ownerIDSelf == rtt.realtime.clientID)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, (Mathf.Sin(Time.time * speed) +1) /2);
        }
    }
}
