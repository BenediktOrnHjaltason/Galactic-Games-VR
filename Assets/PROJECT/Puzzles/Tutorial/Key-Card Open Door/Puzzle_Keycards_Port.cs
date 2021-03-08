using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Normal.Realtime;


public enum EKeycardPortSide
{
    LEFT,
    RIGHT
}

public class Puzzle_Keycards_Port : MonoBehaviour
{

    [SerializeField]
    EKeycardPortSide side;

    Realtime realTime;

    [SerializeField]
    MeshRenderer statusIndicator;

    [SerializeField]
    Material m_statusIdle;

    [SerializeField]
    Material m_StatusOccupied;


    public event Action<EKeycardPortSide, EKeycardAction, int> OnKeycardAction;

    private void Awake()
    {
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {
            if (statusIndicator.material != m_StatusOccupied) statusIndicator.material = m_StatusOccupied;

            OnKeycardAction?.Invoke(side, EKeycardAction.INSERT, realTime.clientID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (statusIndicator.material != m_statusIdle) statusIndicator.material = m_statusIdle;

        if (other.gameObject.layer.Equals(10)) OnKeycardAction?.Invoke(side, EKeycardAction.REMOVE, realTime.clientID);
    }
}
