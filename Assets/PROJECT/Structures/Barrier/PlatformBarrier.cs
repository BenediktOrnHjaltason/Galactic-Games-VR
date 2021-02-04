using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class PlatformBarrier : MonoBehaviour
{

    [SerializeField]
    float pushForce;

    Realtime realtime;

    Vector3 thisToPlatform;

    /// <summary>
    /// Vector representing the horizontal plane
    /// </summary>
    Vector3 projectedOnUp;

    // Start is called before the first frame update
    void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        RealtimeTransform rtt = other.gameObject.transform.parent.GetComponent<RealtimeTransform>();

        if (rtt && rtt.ownerIDSelf == realtime.clientID)
        {
            Rigidbody RB = other.gameObject.transform.parent.GetComponent<Rigidbody>();

            if (RB)
            {
                thisToPlatform = other.transform.position - transform.position;

                projectedOnUp = thisToPlatform - (Vector3.up * (Vector3.Dot(thisToPlatform, Vector3.up)));

                RB.AddForce(projectedOnUp.normalized * pushForce);
            }
        }
    }
}
