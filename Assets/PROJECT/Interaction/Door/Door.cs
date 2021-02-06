using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Door : RealtimeComponent<DoorSyncModel>
{

    enum EState
    {
        Open,
        Closed
    }

    RealtimeTransform doorRealtimeTransform;

    GameObject doorPivot;

    Realtime worldRealtime;

    //Network synced
    bool operateDoor = false;

    [SerializeField]
    EState startState = EState.Closed;

    [SerializeField]
    InteractButton button;

    [SerializeField]
    AnimationCurve fastInEaseOut;

    [SerializeField]
    AnimationCurve easeInFastOut;

    EState currentState;

    Vector3 openScale = new Vector3(1, 0, 1);
    Vector3 closedScale = Vector3.one;

    float increment = 0;




    // Start is called before the first frame update
    void Start()
    {
        worldRealtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        doorRealtimeTransform = GetComponentInChildren<RealtimeTransform>();

        doorPivot = transform.GetChild(0).gameObject;

        currentState = startState;

        if (startState == EState.Open) doorPivot.transform.localScale = openScale;

        button.OnExecute += OperateDoor;
    }

    void OperateDoor()
    {
        model.operateDoor = true;
    }

    private void FixedUpdate()
    {
        if (!operateDoor || !worldRealtime.connected) return;
        else if (doorRealtimeTransform.ownerIDSelf == -1) doorRealtimeTransform.RequestOwnership();

        if (doorRealtimeTransform.ownerIDSelf == worldRealtime.clientID)
        {
            switch(currentState)
            {
                //Closing
                case EState.Open:

                    doorPivot.transform.localScale = Vector3.Lerp(openScale, closedScale, fastInEaseOut.Evaluate(increment));
                    break;


                //Opening
                case EState.Closed:
                    doorPivot.transform.localScale = Vector3.Lerp(closedScale, openScale, easeInFastOut.Evaluate(increment));

                    break;
            }

            if (increment < 1) increment += 0.1f;

            else
            {
                increment = 0;
                currentState = (currentState == EState.Open) ? EState.Closed : EState.Open;

                model.operateDoor = false;
            }
        }
    }

    //************** Networking *************//

    protected override void OnRealtimeModelReplaced(DoorSyncModel previousModel, DoorSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.operateDoorDidChange -= OperatingDoorDidChange;

        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.operateDoor = operateDoor;
            }

            // Update data to match the new model
            UpdateOperateDoor();

            // Register for events so we'll know if data changes later
            currentModel.operateDoorDidChange += OperatingDoorDidChange;

        }
    }

    public bool OperatingDoor { get => operateDoor; set => model.operateDoor = value; }

    void OperatingDoorDidChange(DoorSyncModel model, bool operating)
    {
        UpdateOperateDoor();
    }

    void UpdateOperateDoor()
    {
        operateDoor = model.operateDoor;
    }
}
