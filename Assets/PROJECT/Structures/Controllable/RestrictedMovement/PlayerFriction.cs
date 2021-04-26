using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFriction : MonoBehaviour
{
    Rigidbody platformRB;

    OVRPlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        platformRB = transform.root.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //Was an issue with triggering swing jump in playercontroller while calling Move here, because controller velocities was
        //being picked up in many passes of FixedUpdate, hence checking for IsSwingJumping

        if (player)
        {
            if (!player.Controller.enabled)
            {
                Debug.Log("PlayerFriction: Player started climbing from playform. Setting reference to null");
                player = null;
            }

            else if (!player.IsSwingJumping)
                player.Controller.Move(platformRB.velocity / 50);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            player = other.GetComponent<OVRPlayerController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController ovrpc = other.GetComponent<OVRPlayerController>();

            if (ovrpc = player)
            {
                player = null;
                Debug.Log("PlayerFriction: Releasing player from friction");

            }

            
        }
    }
}
