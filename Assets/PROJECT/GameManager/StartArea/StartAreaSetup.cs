using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAreaSetup : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            Debug.Log("StartAreaSetup: Head entered trigger");

            AudioSource audioSource = other.GetComponent<AudioSource>();

            if (audioSource && audioSource.spatialBlend > 0)
            {
                audioSource.spatialBlend = 0.0f;
            }
        }
    }

}
