using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Button_InteractiveScreen : MonoBehaviour
{
    [SerializeField]
    Material inactiveMaterial;

    [SerializeField]
    Material activeMaterial;


    MeshRenderer mesh;

    public event Action OnExecute;

    void Awake()
    {
        //For some reason GetComponentInChildren() didn't work here
        mesh = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaterial(GameObject highlightedButton)
    {
        //Debug.Log("Button_InfoScreen::SetMaterial called. Name of highLightedButton: " + highlightedButton.name + ". GameObject this script is attached to: " + gameObject.name);

        mesh.material = (highlightedButton == gameObject) ? activeMaterial : inactiveMaterial;

        //Debug.Log("Button_InfoScreen::Material set to" + mesh.material.name);
    }

    public void Execute()
    {
        OnExecute?.Invoke();
    }
}
