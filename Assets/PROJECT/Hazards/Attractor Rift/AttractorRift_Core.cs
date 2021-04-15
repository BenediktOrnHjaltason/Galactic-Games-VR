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
                                    //bool for deciding to remove from influence of all rifts
    public event Action<OVRPlayerController, bool> OnPlayerReachedCore;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) || other.gameObject.layer.Equals(11))
        {
            OVRPlayerController player = other.transform.root.GetComponent<OVRPlayerController>();

            if (player)
            {
                OnPlayerReachedCore?.Invoke(player, true);
                player.RespondToEncounteredHazard();
                player.ResetToRespawnPoint();
            }
        }
    }
}
