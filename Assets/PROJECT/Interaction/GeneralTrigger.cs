using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneralTrigger : MonoBehaviour
{
    public event Action<Collider> OnEnteredTrigger;
    public event Action<Collider> OnExitedTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnEnteredTrigger?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnExitedTrigger?.Invoke(other);
    }
}
