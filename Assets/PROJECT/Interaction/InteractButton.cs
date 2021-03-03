using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct ButtonMeshes
{
    public string State1Name;
    public MeshRenderer State1_Idle;
    public MeshRenderer State1_Highlighted;

    public string State2Name;
    public MeshRenderer State2_Idle;
    public MeshRenderer State2_Highlighted;
}


public class InteractButton : MonoBehaviour
{
    public enum EButtonState
    {
        State1,
        State2
    }

    public enum EButtonVisualType
    {
        Meshes,
        Plane
    }


    [SerializeField]
    bool changeMeshOnExecute = false;

    [SerializeField]
    ButtonMeshes buttonSet;


    [SerializeField]
    MeshRenderer plane;

    [SerializeField]
    Material materialIdle;

    [SerializeField]
    Material materialHighlighted;

    

    bool beingHighlighted = false;

    public bool BeingHighlighted { set => beingHighlighted = value; }


    EButtonState state;
    public EButtonState State { set => state = value; }


    public event Action OnExecute;



    private void FixedUpdate()
    {

        if (plane)
        {
            if (beingHighlighted && plane.material != materialHighlighted) plane.material = materialHighlighted;
            else if (!beingHighlighted && plane.material != materialIdle) plane.material = materialIdle;
        }

        else
        {
            if (!changeMeshOnExecute) return;

            //Handle Highlighting
            switch (state)
            {
                case EButtonState.State1:

                    if (!buttonSet.State1_Idle) Debug.Log("InteractButton: buttonSet.State1_Idle MeshRenderer reference not set");

                    if (!beingHighlighted && !buttonSet.State1_Idle.enabled)
                    {
                        buttonSet.State1_Idle.enabled = true;
                        buttonSet.State1_Highlighted.enabled = false;
                    }
                    else if (beingHighlighted && !buttonSet.State1_Highlighted.enabled)
                    {
                        buttonSet.State1_Highlighted.enabled = true;
                        buttonSet.State1_Idle.enabled = false;

                    }
                    break;

                case EButtonState.State2:

                    if (!beingHighlighted && !buttonSet.State2_Idle.enabled)
                    {
                        buttonSet.State2_Idle.enabled = true;
                        buttonSet.State2_Highlighted.enabled = false;
                    }
                    else if (beingHighlighted && !buttonSet.State2_Highlighted.enabled)
                    {
                        buttonSet.State2_Highlighted.enabled = true;
                        buttonSet.State2_Idle.enabled = false;

                    }
                    break;
            }
        }

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

    public void ToggleMeshes()
    {
        if (state == EButtonState.State1)
        {
            buttonSet.State1_Idle.enabled = false;
            buttonSet.State1_Highlighted.enabled = false;

            state = EButtonState.State2;
        }

        else
        {
            buttonSet.State2_Idle.enabled = false;
            buttonSet.State2_Highlighted.enabled = false;

            state = EButtonState.State1;
        }
    }

    public void InitializeState(string stateName)
    {
        if (buttonSet.State1Name == stateName)
        {
            state = EButtonState.State1;

            buttonSet.State1_Idle.enabled = true;
            buttonSet.State1_Highlighted.enabled = false;

            buttonSet.State2_Idle.enabled = false;
            buttonSet.State2_Highlighted.enabled = false;
        }

        else if (buttonSet.State2Name == stateName)
        {
            state = EButtonState.State2;

            buttonSet.State2_Idle.enabled = true;
            buttonSet.State2_Highlighted.enabled = false;

            buttonSet.State1_Idle.enabled = false;
            buttonSet.State1_Highlighted.enabled = false;
        }
    }
}
