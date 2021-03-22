using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Star_Pulsar : MonoBehaviour
{
    [SerializeField]
    Transform basePivot;

    public event Action OnTriggerEntered;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        basePivot.localRotation = Quaternion.Euler(basePivot.localRotation.eulerAngles + new Vector3(0, 10, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke();
    }
}
