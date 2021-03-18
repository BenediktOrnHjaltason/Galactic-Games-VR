using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle_TicTac : MonoBehaviour
{
    List<MeshRenderer> meshes = new List<MeshRenderer>();

    private void Awake()
    {
        for (int i = 0; i < 3; i++) meshes.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
    }

    public void SetVisibility(bool visibility)
    {
        foreach (MeshRenderer mr in meshes) mr.enabled = visibility;
    }
}
