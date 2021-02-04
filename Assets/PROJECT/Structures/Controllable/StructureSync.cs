using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Types;
using System;

/*
 This class handles networking state for controllable structures (enforcing no-movement while players are on structure etc),
 but also contains general non-synced variables that control behaviour in relation to physics manipulation,
 allowed rotation forces etc. May want to factor non-synced stuff out to separate, but it's convenient to have
 to add one script to create controllable structures.
 */

[System.Serializable]
struct WorldAxisToRotation
{
    public Vector3 Roll;
    public Vector3 Yaw;
    public Vector3 Pitch;
}
[System.Serializable]
struct WorldAxisConstraints
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
    //----

    [SerializeField]
    bool allowRotationForces = true;
    public bool AllowRotationForces { get => allowRotationForces; }
    
    GameObject mainStructure;

    [SerializeField]
    ERotationForceAxis rotationAxis = ERotationForceAxis.PLAYER;

    /// <summary>
    /// Settings for structures rotating in local space
    /// </summary>
    [Header("Define which world direction represent which rotation in this case, i.e. (0,0,1) represents Roll")]
    [SerializeField]
    WorldAxisToRotation worldAxisToRotation;

    [Header("Define which rotations to constrain")]
    [SerializeField]
    WorldAxisConstraints worldAxisConstraints;

    float compensationForConstrainedRBImpactRotation = 220;

    Rigidbody RB;

    public event Action OnBreakControl;


    private void Awake()
    {
        mainStructure = transform.GetChild(0).gameObject;

        RB = GetComponent<Rigidbody>();
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

            // Update data to match the new model
            UpdatePlayersOccupying();
            UpdateAvailableToManipulate();
            UpdateCollisionEnabled();

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
    /// Is structure available for being moved/rotated/replicated with gun etc? Used to guarantee that only one player
    /// at a time can manipulate this structure
    /// </summary>
    bool availableToManipulate = true;

    public bool AvailableToManipulate { get => availableToManipulate; set => model.availableToManipulate = value; }

    private void AvailableToManipulateDidChange(StructureSync_Model model, bool available)
    {
        UpdateAvailableToManipulate();
    }
    private void UpdateAvailableToManipulate()
    {
        availableToManipulate = model.availableToManipulate;
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
        mainStructure.layer = (model.collisionEnabled) ? 10 : 9;
    }
    //-------------------

    //------**** General functionality ****------//

    public void Rotate(Vector3 playerForward, float rollForce, float yawForce, Vector3 playerRight, float pitchForce)
    {
        switch (rotationAxis)
        {
            case ERotationForceAxis.PLAYER:
                {
                    //Roll
                    RB.AddTorque(playerForward * rollForce, ForceMode.Acceleration);

                    //Yaw
                    RB.AddTorque(Vector3.up * yawForce, ForceMode.Acceleration);

                    //Pitch
                    RB.AddTorque(playerRight * pitchForce, ForceMode.Acceleration);
                    break;
                }

            case ERotationForceAxis.WORLD:
                {

                    //Roll (Only use case for now)
                    if (!worldAxisConstraints.constrainRoll)    RB.AddRelativeTorque(worldAxisToRotation.Roll * rollForce * compensationForConstrainedRBImpactRotation * Time.deltaTime, ForceMode.Acceleration);

                    //Yaw
                    //if (!localRotationConstraints.constrainYaw)     RB.AddRelativeTorque(transform.up * yawForce);

                    //Pitch
                    //if (!localRotationConstraints.constrainPitch)   RB.AddRelativeTorque(transform.right * pitchForce);

                    break;
                }           
        }
    }

    public void BreakControl()
    {
        OnBreakControl?.Invoke();
    }

    private void FixedUpdate()
    {
        if (playersOccupying > 0) BreakControl(); 
    }
}
