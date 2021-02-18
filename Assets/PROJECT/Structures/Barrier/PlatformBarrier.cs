using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class PlatformBarrier : MonoBehaviour
{
    [SerializeField]
    MeshRenderer effectPlane;

    [SerializeField]
    float pushForce;

    Realtime realtime;

    Vector3 thisToPlatform;

    /// <summary>
    /// Vector representing the horizontal plane
    /// </summary>
    Vector3 projectedOnUp;

    RaycastHit hit;

    bool showEffectPlane = false;
    float showTimeLimit = 2;
    float runningTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }


    void FixedUpdate()
    {
        if (showEffectPlane)
        {
            runningTime += 0.1f;


            if (runningTime > showTimeLimit)
            {
                showEffectPlane = false;
                effectPlane.enabled = false;
                runningTime = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {

            RealtimeTransform rtt = other.gameObject.transform.parent.GetComponent<RealtimeTransform>();

            if (rtt && rtt.ownerIDSelf == realtime.clientID)
            {
                Rigidbody RB = other.gameObject.transform.parent.GetComponent<Rigidbody>();
                StructureSync ss = other.gameObject.transform.parent.GetComponent<StructureSync>();


                if (RB && ss)
                {
                    thisToPlatform = other.transform.position - transform.position;

                    projectedOnUp = thisToPlatform - (Vector3.up * (Vector3.Dot(thisToPlatform, Vector3.up)));


                    Vector3 hitNormal = Vector3.zero; ;
                    Vector3 hitPoint = Vector3.zero;
                    if (Physics.Raycast(other.transform.position, -thisToPlatform, out hit, 20.0f, 1 << 15))
                    {
                        hitNormal = hit.normal;
                        hitPoint = hit.point;

                        RB.velocity = Vector3.zero;

                        RB.AddForce(hitNormal * pushForce);
                    }

                    //ss.BreakControl();

                    effectPlane.transform.position = hitPoint + hitNormal * 0.1f;
                    effectPlane.transform.rotation = Quaternion.LookRotation(hitNormal) * Quaternion.Euler(90, 0, 0);

                    effectPlane.enabled = true;
                    showEffectPlane = true;
                }
            }
        }
    }
}
