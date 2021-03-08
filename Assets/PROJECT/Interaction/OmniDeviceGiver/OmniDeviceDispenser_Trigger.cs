using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OmniDeviceDispenser_Trigger : MonoBehaviour
{
    public event Action<Hand> OnHandEnters;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11))
        {
            Hand hand = other.GetComponent<Hand>();

            if (hand) OnHandEnters?.Invoke(hand);
        }
    }
}
