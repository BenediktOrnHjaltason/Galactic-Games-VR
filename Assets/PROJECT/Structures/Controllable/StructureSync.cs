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
[System.Serializable]
struct SelfAxisConstraints
{
    public bool constrainRoll;
    public bool constrainYaw;
    public bool constrainPitch;
}

public class StructureSync : RealtimeComponent<StructureSync_Model>
{
    //----Variables that are replicated on network clients but never change
    [SerializeField]
    bool allowDuplicationByDevice;

    public bool AllowDuplicationByDevice { get => allowDuplicationByDevice; }

    [SerializeField]
    bool allowGravityForceByDevice = true;

    public bool AllowGravityForceByDevice { get => allowGravityForceByDevice; }

    protected Rigidbody rb;

    protected RealtimeTransform rtt;

    //----

    [SerializeField]
    bool allowRotationForces = true;
    public bool AllowRotationForces { get => allowRotationForces; set => allowRotationForces = value; }    


    GameObject mainStructure;

    [SerializeField]
    ERotationForceAxis rotationAxis = ERotationForceAxis.PLAYER;

    /// <summary>
    /// Settings for structures rotating in local space
    /// </summary>
    [Header("Define which self direction represent which rotation")]
    [SerializeField]
    SelfAxisToRotation selfAxisToRotation;

    [Header("Define which rotations to constrain")]
    [SerializeField]
    SelfAxisConstraints selfAxisConstraints;

    [SerializeField]
    float selfRotateMultiplier = 1;

    public event Action OnBreakControl;

    protected Realtime worldRealtime;


    protected virtual void Start()
    {
        mainStructure = transform.GetChild(0).gameObject;

        rb = GetComponent<Rigidbody>();

        rtt = GetComponent<RealtimeTransform>();

        worldRealtime = GameObject.Find("Realtime").GetComponent<Realtime>();
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

    public void Rotate(Vector3 playerForward, float rollForce, float yawForce, Vector3 playerRight, float pitchForce)
    {
        switch (rotationAxis)
        {
            case ERotationForceAxis.PLAYER:
                {

                    OnExternalPiggybacking?.Invoke(rollForce, yawForce, pitchForce);

                    //Roll
                    rb.AddTorque(playerForward * rollForce, ForceMode.Acceleration);

                    //Yaw
                    rb.AddTorque(Vector3.up * yawForce, ForceMode.Acceleration);

                    //Pitch
                    rb.AddTorque(playerRight * pitchForce, ForceMode.Acceleration);
                    break;
                }

            case ERotationForceAxis.SELF:
                {

                    //Roll (Only use case for now)
                    if (!selfAxisConstraints.constrainRoll)    rb.AddRelativeTorque(selfAxisToRotation.Roll * rollForce * selfRotateMultiplier, ForceMode.Acceleration);

                    //Yaw
                    //if (!localRotationConstraints.constrainYaw)     RB.AddRelativeTorque(transform.up * yawForce);

                    //Pitch
                    //if (!localRotationConstraints.constrainPitch)   RB.AddRelativeTorque(transform.right * pitchForce);

                    break;
                }           
        }
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
    }
}
