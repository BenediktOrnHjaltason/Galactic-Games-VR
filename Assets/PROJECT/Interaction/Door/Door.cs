using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Door : RealtimeComponent<DoorSyncModel>
{
    GameObject doorPivot;

    //Network synced
    bool operateDoor = false;

    bool doorOperationTriggered = false;

    [SerializeField]
    EDoorState state;

    public EDoorState State { get => state; }

    public EDoorState nextState;



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
        button.OnExecute += Operate;
    }

    public void Operate()
    {
        model.operateDoor = true;
    }

    private void FixedUpdate()
    {
        if (doorOperationTriggered)
        {
            switch (nextState)
            {

                case EDoorState.Closed:

                    doorPivot.transform.localScale = Vector3.Lerp(openScale, closedScale, fastInEaseOut.Evaluate(increment));
                    break;



                case EDoorState.Open:
                    doorPivot.transform.localScale = Vector3.Lerp(closedScale, openScale, easeInFastOut.Evaluate(increment));
                    break;
            }

            if (increment < 1) increment += 0.1f;
            else
            {
                increment = 0;

                doorOperationTriggered = false;
                OperatingDoor = false;

                OpenOrClosed = nextState;
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
                //Debug.Log("Door: isFreshModel");

                currentModel.operateDoor = false;
                currentModel.openOrClosed = state;
            }

            //else Debug.Log("Door: Model was not fresh");


            // Update data to match the new model
            UpdateOperateDoor();
            UpdateOpenOrClosed();


            Initialize();

            // Register for events so we'll know if data changes later
            currentModel.operateDoorDidChange += OperatingDoorDidChange;
            currentModel.openOrClosedDidChange += OpenOrClosedDidChange;
        }
    }

    private void Initialize()
    {
        doorPivot = transform.GetChild(0).gameObject;

        //Debug.Log("Door: Initialized called with state " + state.ToString());

        doorPivot.transform.localScale = (state == EDoorState.Open) ? openScale : closedScale;
    }


    public bool OperatingDoor { get => operateDoor; set => model.operateDoor = value; }

    void OperatingDoorDidChange(DoorSyncModel model, bool operating)
    {
        UpdateOperateDoor();
    }

    void UpdateOperateDoor()
    {
        if (model.operateDoor)
        {
            doorOperationTriggered = true;
            nextState = (nextState == EDoorState.Open) ? EDoorState.Closed : EDoorState.Open;

        }
    }

    //-------

    EDoorState openOrClosed;

    public EDoorState OpenOrClosed { get => openOrClosed; set => model.openOrClosed = value; }
    
    void OpenOrClosedDidChange (DoorSyncModel model, EDoorState openOrClosed)
    {
        UpdateOpenOrClosed();
    }

    //NOTE! Start state. Rename.
    void UpdateOpenOrClosed()
    {
        state = nextState = model.openOrClosed;
    }
}
