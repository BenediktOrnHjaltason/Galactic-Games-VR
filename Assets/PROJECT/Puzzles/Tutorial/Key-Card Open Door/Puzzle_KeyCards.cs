﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;


public enum EKeycardAction
{
    INSERT,
    REMOVE
}

public class Puzzle_KeyCards : RealtimeComponent<KeycardPuzzle_Model>
{
    [SerializeField]
    Door door;


    [SerializeField]
    List<KeycardPort> ports = new List<KeycardPort>();

    private void Awake()
    {
        foreach (KeycardPort port in ports)
            port.OnKeycardAction += RegisterKeycardAction;
    }

    void RegisterKeycardAction(KeycardPort port, EKeycardAction action)
    {
        foreach (KeycardPort p in ports)
        {
            if (p == port)
            {
                if (action == EKeycardAction.INSERT && !p.Occupied)
                {
                    p.Occupied = true;
                    InsertedKeys++;

                    VerifyCondition();
                }

                else if (action == EKeycardAction.REMOVE && p.Occupied)
                {
                    p.Occupied = false;
                    InsertedKeys--;

                    VerifyCondition();
                }

                break;
            }
        }
    }

    void VerifyCondition()
    {
        if (InsertedKeys == ports.Count && door.State == EDoorState.Closed)
        {
            door.Operate();
        }
        else if (InsertedKeys < ports.Count && door.State == EDoorState.Open)
        {
            door.Operate();
        }
    }

    //--------Networking

    protected override void OnRealtimeModelReplaced(KeycardPuzzle_Model previousModel, KeycardPuzzle_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.insertedKeysDidChange -= InsertedKeysDidChange;
            previousModel.leftPortOccupiedDidChange -= LeftPortOccupiedDidChange;
            previousModel.rightPortOccupiedDidChange -= RightPortOccupiedDidChange;
            //previousModel.previousActionOpenedDoorDidChange -= PreviousActionOpenedDoorDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.insertedKeys = insertedKeys;
                currentModel.leftPortOccupied = leftPortOccupied;
                currentModel.rightPortOccupied = rightPortOccupied;
                //currentModel.previousActionOpenedDoor = (door.StartState == EDoorState.Closed) ? false : true;
            }

            
            // Update data to match the new model
            UpdateInsertedKeys();
            UpdateLeftPortOccupied();
            UpdateRightPortOccupied();
            UpdatePreviousActionOpenedDoor();
            

            // Register for events so we'll know if data changes later
            currentModel.insertedKeysDidChange += InsertedKeysDidChange;
            currentModel.leftPortOccupiedDidChange += LeftPortOccupiedDidChange;
            currentModel.rightPortOccupiedDidChange += RightPortOccupiedDidChange;
            //currentModel.previousActionOpenedDoorDidChange += PreviousActionOpenedDoorDidChange;
        }
    }

    int insertedKeys = 0;
    int InsertedKeys { get => insertedKeys; set => model.insertedKeys = value; }

    void InsertedKeysDidChange(KeycardPuzzle_Model model, int keys)
    {
        UpdateInsertedKeys();
    }

    void UpdateInsertedKeys()
    {
        insertedKeys = model.insertedKeys;
    }

    //----

    bool leftPortOccupied = false;
    bool LeftPortOccupied { get => leftPortOccupied; set => model.leftPortOccupied = value;   }

    void LeftPortOccupiedDidChange(KeycardPuzzle_Model model, bool state)
    {
        UpdateLeftPortOccupied();
    }

    void UpdateLeftPortOccupied()
    {
        leftPortOccupied = model.leftPortOccupied;
    }

    //----

    bool rightPortOccupied = false;
    bool RightPortOccupied { get => rightPortOccupied; set => model.rightPortOccupied = value; }

    void RightPortOccupiedDidChange(KeycardPuzzle_Model model, bool state)
    {
        UpdateRightPortOccupied();
    }

    void UpdateRightPortOccupied()
    {
        rightPortOccupied = model.rightPortOccupied;
    }

    //----

    bool previousActionOpenedDoor = false;

    bool PreviousActionOpenedDoor { get => previousActionOpenedDoor; set => model.previousActionOpenedDoor = value; }

    void PreviousActionOpenedDoorDidChange(KeycardPuzzle_Model model, bool opened)
    {
        UpdatePreviousActionOpenedDoor();
    }

    void UpdatePreviousActionOpenedDoor()
    {
        previousActionOpenedDoor = model.previousActionOpenedDoor;
    }
}
