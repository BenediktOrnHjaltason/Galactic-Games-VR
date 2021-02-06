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

    GameObject doorObject;

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

    Vector3 fullScale;

    float increment = 0;

    Vector3 closedRotation = new Vector3(0, 0, -360.0f);




    // Start is called before the first frame update
    void Start()
    {
        worldRealtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        doorRealtimeTransform = GetComponentInChildren<RealtimeTransform>();

        doorObject = transform.GetChild(0).gameObject;

        fullScale = doorObject.transform.localScale;


        if (startState == EState.Open) doorObject.transform.localScale = Vector3.zero;

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
            if (increment < 1) increment += 0.05f;
            
            switch(currentState)
            {
                case EState.Open:

                    doorObject.transform.localScale = Vector3.Lerp(fullScale, Vector3.zero, fastInEaseOut.Evaluate(increment));
                    doorObject.transform.localRotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, closedRotation, fastInEaseOut.Evaluate(increment)));
                    break;



                case EState.Closed:
                    doorObject.transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, easeInFastOut.Evaluate(increment));
                    doorObject.transform.localRotation = Quaternion.Euler(Vector3.Lerp(closedRotation, Vector3.zero, easeInFastOut.Evaluate(increment)));

                    break;
            }

            if (increment > 1)
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
