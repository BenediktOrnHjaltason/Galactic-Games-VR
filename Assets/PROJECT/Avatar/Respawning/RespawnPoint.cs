using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController pc = other.GetComponent<OVRPlayerController>();

            if (pc) pc.RespawnPoint = transform.parent.position;
        }
    }
}
