using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZipPointPlayerSensor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11))
        {
            Hand hand = other.GetComponent<Hand>();

            if (hand) hand.ReleaseZipLine();
        }
    }
}
