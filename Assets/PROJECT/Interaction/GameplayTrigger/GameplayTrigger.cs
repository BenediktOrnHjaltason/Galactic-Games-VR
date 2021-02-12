using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameplayTrigger : MonoBehaviour
{

    public event Action Execute;

    List<GameObject> playersThatEnter = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            if (!playersThatEnter.Contains(other.gameObject)) playersThatEnter.Add(other.gameObject);

            Execute?.Invoke();
        }
    }
}
