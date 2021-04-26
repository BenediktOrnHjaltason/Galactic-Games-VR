using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZipPointPlayerSensor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11))
        {
            Debug.Log("ZipPointPlayerSensor: Hand collided with sensor.");

            Hand hand = other.GetComponent<Hand>();

            if (hand) hand.ReleaseZipLine();
        }
    }
}
