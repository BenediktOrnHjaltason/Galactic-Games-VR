using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractButton : MonoBehaviour
{
    [SerializeField]
    Material inactiveMaterial;

    [SerializeField]
    Material activeMaterial;

    MeshRenderer mesh;

    bool beingHighlighted = false;
    public bool BeingHighlighted { set => beingHighlighted = value; }


    public event Action OnExecute;

    bool isHighLighted = false;
    public bool IsHighLighted 
    {
        set
        {
            if (value == true) isHighLighted = value;
        }
    }

    void Awake()
    {
        //For some reason GetComponentInChildren() didn't work here
        mesh = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        //HandleHighLighting
        if (beingHighlighted && mesh.material != activeMaterial) mesh.material = activeMaterial;
        else if (!beingHighlighted && mesh.material != inactiveMaterial) mesh.material = inactiveMaterial;

        beingHighlighted = false;
    }

    public void Execute()
    {
        OnExecute?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11)) Execute();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer.Equals(11)) beingHighlighted = true;
    }
}
