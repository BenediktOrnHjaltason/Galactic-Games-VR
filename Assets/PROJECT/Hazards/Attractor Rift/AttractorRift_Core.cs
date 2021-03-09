using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
    The core is the trigger that detects if the player has hit the inner part of the rift,
    sending him back to his respawn location
 */


public class AttractorRift_Core : MonoBehaviour
{

    public event Action<OVRPlayerController> OnPlayerReachedCore;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                OnPlayerReachedCore?.Invoke(player);
                player.ResetToRespawnPoint();
            }
        }

        if (other.gameObject.layer.Equals(11))
        {
            OVRPlayerController player = other.transform.root.GetComponent<OVRPlayerController>();

            if (player)
            {
                OnPlayerReachedCore?.Invoke(player);
                player.ResetToRespawnPoint();
            }
        }
    }
}
