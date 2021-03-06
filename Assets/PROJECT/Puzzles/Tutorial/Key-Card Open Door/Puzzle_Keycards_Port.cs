using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum EKeycardPortSide
{
    LEFT,
    RIGHT
}

public class Puzzle_Keycards_Port : MonoBehaviour
{

    [SerializeField]
    EKeycardPortSide side;

    public event Action<EKeycardPortSide, EKeycardAction> OnKeycardAction;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10)) OnKeycardAction?.Invoke(side, EKeycardAction.INSERT);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(10)) OnKeycardAction?.Invoke(side, EKeycardAction.REMOVE);
    }
}
