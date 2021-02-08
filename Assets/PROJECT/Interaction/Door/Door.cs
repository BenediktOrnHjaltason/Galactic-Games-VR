using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Door : RealtimeComponent<DoorSyncModel>
{
    RealtimeTransform doorPivotRtt;

    GameObject doorPivot;

    Realtime worldRealtime;

    //Network synced
    bool operateDoor = false;

    [SerializeField]
    EDoorState startState = EDoorState.Closed;

    [SerializeField]
    InteractButton button;

    [SerializeField]
    AnimationCurve fastInEaseOut;

    [SerializeField]
    AnimationCurve easeInFastOut;



    Vector3 openScale = new Vector3(1, 0, 1);
    Vector3 closedScale = Vector3.one;

    float increment = 0;

    // Start is called before the first frame update
    void Start()
    {
        worldRealtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        doorPivotRtt = GetComponentInChildren<RealtimeTransform>();

        doorPivot = transform.GetChild(0).gameObject;

        button.OnExecute += OperateDoor;
    }

    void OperateDoor()
    {
        if (!doorPivotRtt.isOwnedLocallySelf) doorPivotRtt.RequestOwnership();
        model.operateDoor = true;
    }

    private void FixedUpdate()
    {
        if (!doorPivotRtt.isOwnedLocallySelf) return;

        if (OperatingDoor)
        {
            switch (openOrClosed)
            {
                //Closing
                case EDoorState.Open:

                    doorPivot.transform.localScale = Vector3.Lerp(openScale, closedScale, fastInEaseOut.Evaluate(increment));
                    break;


                //Opening
                case EDoorState.Closed:
                    doorPivot.transform.localScale = Vector3.Lerp(closedScale, openScale, easeInFastOut.Evaluate(increment));

                    break;
            }

            if (increment < 1) increment += 0.1f;

            else
            {
                increment = 0;
                openOrClosed = (openOrClosed == EDoorState.Open) ? EDoorState.Closed : EDoorState.Open;

                OperatingDoor = false;
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
            previousModel.openOrClosedDidChange -= OpenOrClosedDidChange;

        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.operateDoor = false;
                currentModel.openOrClosed = startState;
            }

            // Update data to match the new model
            UpdateOperateDoor();
            UpdateOpenOrClosed();

            // Register for events so we'll know if data changes later
            currentModel.operateDoorDidChange += OperatingDoorDidChange;
            currentModel.openOrClosedDidChange += OpenOrClosedDidChange;

        }
    }

    private void OnConnectedToServer()
    {
        Initialize();
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

    //-------

    EDoorState openOrClosed;

    public EDoorState OpenOrClosed { get => openOrClosed; set => model.openOrClosed = value; }
    
    void OpenOrClosedDidChange (DoorSyncModel model, EDoorState openOrClosed)
    {
        UpdateOpenOrClosed();
    }

    void UpdateOpenOrClosed()
    {
        openOrClosed = model.openOrClosed;
    }

    private void Initialize()
    {
        doorPivot.transform.localScale = (openOrClosed == EDoorState.Open) ? openScale : closedScale;
    }
}
