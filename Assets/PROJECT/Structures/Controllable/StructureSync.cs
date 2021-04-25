using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Types;
using System;

/*
 This class handles networked state for controllable structures (allowing control with GravityForce and replicator, 
 enforcing no-movement while players are on structure etc)
 */

[System.Serializable]
struct SelfAxisToRotation
{
    public Vector3 Roll;
    public Vector3 Yaw;
    public Vector3 Pitch;
}

public class StructureSync : RealtimeComponent<StructureSync_Model>
{
    [SerializeField]
    bool allowDuplicationByPlayer;

    public bool AllowDuplicationByPlayer { get => allowDuplicationByPlayer; set => allowDuplicationByPlayer = value; }

    [SerializeField]
    bool allowGravityForceByPlayer = true;

    public bool AllowGravityForceByPlayer { get => allowGravityForceByPlayer; }

    protected Rigidbody rb;

    protected RealtimeTransform rtt;
    public RealtimeTransform Rtt { get => rtt; }

    //----

    [SerializeField]
    bool allowRotationForceByPlayer = true;
    public bool AllowRotationForceByPlayer { get => allowRotationForceByPlayer; set => allowRotationForceByPlayer = value; }

    [SerializeField]
    float pushPullMultiplier = 1;

    public float PushPullMultiplier { get => pushPullMultiplier; }

    bool ownedByPlayer = false;
    public bool OwnedByPlayer { get => ownedByPlayer; set => ownedByPlayer = value; }

    GameObject mainStructure;

    public event Action OnBreakControl;

    /// <summary>
    /// Used for structures with rotation restricted to one world axis to set correct rotation force independent of which side player is on
    /// </summary>
    /// <param name="controllingHandPosition"></param>
    public virtual void CalculatePlayerAngleModifier(Vector3 controllingHandPosition)
    {

    }


    protected virtual void Awake()
    {
        mainStructure = transform.GetChild(0).gameObject;

        rb = GetComponent<Rigidbody>();

        rtt = GetComponent<RealtimeTransform>();
    }

   
    protected override void OnRealtimeModelReplaced(StructureSync_Model previousModel, StructureSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playersOccupyingDidChange -= PlayersOccupyingDidChange;
            previousModel.availableToManipulateDidChange -= AvailableToManipulateDidChange;
            previousModel.collisionEnabledDidChange -= CollisionEnabledDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.playersOccupying = playersOccupying;
                currentModel.availableToManipulate = availableToManipulate;
                currentModel.collisionEnabled = true;
            }

            else
            {
                //Update data to match the new model
                UpdatePlayersOccupying();
                UpdateAvailableToManipulate();
                UpdateCollisionEnabled();
            }

            // Register for events so we'll know if data changes later
            currentModel.playersOccupyingDidChange += PlayersOccupyingDidChange;
            currentModel.availableToManipulateDidChange += AvailableToManipulateDidChange;
            currentModel.collisionEnabledDidChange += CollisionEnabledDidChange;
        }
    }

    /// <summary>
    /// Players standing on or climbing on this structre (Platform, climbing wall etc) 
    /// </summary>
    int playersOccupying = 0;

    public int PlayersOccupying
    {
        get => playersOccupying;

        set
        {
            if (value < 0) model.playersOccupying = 0;
            else model.playersOccupying = value;
        }
    }

    private void PlayersOccupyingDidChange(StructureSync_Model model, int playersUsingStructure)
    {
        UpdatePlayersOccupying();
    }
    private void UpdatePlayersOccupying()
    {
        playersOccupying = model.playersOccupying;
    }
    //-----------------

    /// <summary>
    /// Is structure available for being moved/rotated/replicated with OmniDevice? Used to guarantee that only one player
    /// at a time can manipulate this structure
    /// </summary>
    protected bool availableToManipulate = true;

    public bool AvailableToManipulate { get => availableToManipulate; set => model.availableToManipulate = value; }

    public event Action OnControlTaken;
    public event Action OnControlReleased;

    private void AvailableToManipulateDidChange(StructureSync_Model model, bool available)
    {
        UpdateAvailableToManipulate();
    }
    private void UpdateAvailableToManipulate()
    {
        availableToManipulate = model.availableToManipulate;

        if (!availableToManipulate) OnControlTaken?.Invoke();
        else OnControlReleased?.Invoke();
    }
    //------------------

    public bool CollisionEnabled 
    { 
        get => mainStructure.layer == 10;

        set 
        { 
            model.collisionEnabled = value;
        }
    }

    private void CollisionEnabledDidChange(StructureSync_Model model, bool enabled)
    {
        
        UpdateCollisionEnabled();
    }
    private void UpdateCollisionEnabled()
    {
        if (!mainStructure) mainStructure = transform.GetChild(0).gameObject;

        mainStructure.layer = (model.collisionEnabled) ? 10 : 9;
    }
    //-------------------

    protected float sideGlowOpacity = 0.2f;

    public float SideGlowOpacity { get => sideGlowOpacity; set => model.sideGlowOpacity = value; }

    //------**** General functionality ****------//

    public event Action<float, float, float> OnExternalPiggybacking;

    public virtual void Rotate(Vector3 playerForward, float rollForce, float yawForce, Vector3 playerRight, float pitchForce)
    {
        OnExternalPiggybacking?.Invoke(rollForce, yawForce, pitchForce);

        //Roll
        rb.AddTorque(playerForward * rollForce, ForceMode.Acceleration);

        //Yaw
        rb.AddTorque(Vector3.up * yawForce, ForceMode.Acceleration);

        //Pitch
        rb.AddTorque(playerRight * pitchForce, ForceMode.Acceleration);
    }

    public void AddGravityForce(Vector3 force)
    {
        rb.AddForce(force);
    }

    public void BreakControl()
    {
        OnBreakControl?.Invoke();
    }

    protected virtual void FixedUpdate()
    {
        if (playersOccupying > 0) BreakControl();

        //Prevent structure becoming unavailable forever if game loses input focus of controller
        if (!availableToManipulate && rb.velocity == Vector3.zero) AvailableToManipulate = true;
    }

    public void ResetLinearVelocity()
    {
        rb.velocity = Vector3.zero;
    }
}
