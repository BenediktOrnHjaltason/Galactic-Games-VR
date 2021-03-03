using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

/*
    The player sensor is the large field detecting if players are in are of influence,
    and pushing them to the core if they are
 */


public class AttractorRift_PlayerSensor : MonoBehaviour
{

    [SerializeField]
    float autoForce;

    [SerializeField]
    AttractorRift_Core core;

    Transform baseTransform;
    RealtimeTransform rtt;
    Rigidbody rb;

    Vector3 baseToRoot = Vector3.zero;

    List<OVRPlayerController> playersInReach = new List<OVRPlayerController>();

    // Start is called before the first frame update
    void Start()
    {
        baseTransform = transform.GetComponentInParent<Transform>();
        rtt = transform.GetComponentInParent<RealtimeTransform>();
        rb = GetComponentInParent<Rigidbody>();


        if (core)
        {
            Debug.Log("Attractor Sensor HAS reference to core");
            core.OnPlayerReachedCore += RemovePlayerFromInfluence;
        }

        else Debug.Log("Attractor Sensor DOES NOT HAVE reference to core");
    }

    private void FixedUpdate()
    {
        if (!rtt.realtime.connected) return;

        if (rtt.isUnownedSelf) rtt.RequestOwnership();
        else if (rtt.isOwnedLocallySelf)
        {
            baseToRoot = transform.root.position - baseTransform.position;

            if ((transform.position - transform.root.transform.position).sqrMagnitude > 0.02)
                rb.AddForce(baseToRoot * autoForce);
        }

        foreach (OVRPlayerController player in playersInReach)
        {
            if (player.HeadRealtimeView.isOwnedLocallySelf) player.transform.position += (baseTransform.position - player.transform.position).normalized * 0.04f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                playersInReach.Add(player);
            }
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                RemovePlayerFromInfluence(player);
            }
        }
    }
    

    void RemovePlayerFromInfluence(OVRPlayerController player)
    {
        playersInReach.Remove(player);
    }
}
