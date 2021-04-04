using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAreaSetup : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            AudioSource audioSource = other.GetComponent<AudioSource>();

            if (audioSource && audioSource.spatialize)
            {
                audioSource.spatialBlend = 0.0f;
            }
        }
    }

}
