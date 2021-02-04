using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VerticalMazeHazard : MonoBehaviour
{
    public event Action<GameObject> OnPlatformHitHazard;


    private void OnTriggerEnter(Collider other)
    {
        OnPlatformHitHazard?.Invoke(other.gameObject);
    }
}
