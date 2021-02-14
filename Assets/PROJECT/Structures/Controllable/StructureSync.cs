﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Types;
using System;

/*
 This class handles networking state for controllable structures (enforcing no-movement while players are on structure etc),
 but also contains general non-synced variables that control behaviour in relation to physics manipulation,
 allowed rotation forces etc. May want to factor non-synced stuff out to separate, but it's convenient to only have
 to add one script to create controllable structures.
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


    Rigidbody rb;

    RealtimeTransform rtt;

    //----

    [SerializeField]
    bool allowRotationForces = true;
    public bool AllowRotationForces { get => allowRotationForces; }

    [SerializeField]
    MeshRenderer mesh;


    //Need to restructure stuff later
    bool isRailsPlatform = false;

    string graphVariableGlow_Free = "Vector1_A563CEE";

    string graphVariableGlow_Rails = "Vector1_CDE54C4F";

    Material[] materials;

    bool isPlatform = false;

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


    private void Awake()
    {
        mainStructure = transform.GetChild(0).gameObject;

        rb = GetComponent<Rigidbody>();

        rtt = GetComponent<RealtimeTransform>();

        

        isRailsPlatform = (GetComponent<StructureOnRails>());

        //Restructuring stuff later
        isPlatform = this.gameObject.name.Contains("Platform");

        if (isPlatform) materials = mesh.materials;

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

    float sideGlowOpacity = 0.2f;

    public float SideGlowOpacity { get => sideGlowOpacity; set => model.sideGlowOpacity = value; }

    //------**** General functionality ****------//

    public void Rotate(Vector3 playerForward, float rollForce, float yawForce, Vector3 playerRight, float pitchForce)
    {
        switch (rotationAxis)
        {
            case ERotationForceAxis.PLAYER:
                {
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

    public void BreakControl()
    {
        OnBreakControl?.Invoke();
    }

    private void FixedUpdate()
    {
        if (playersOccupying > 0) BreakControl();

        
        //Restructuring later
        if (!isPlatform) return;

        if (!availableToManipulate)
        {
            if (rtt.isOwnedLocallySelf)
            {
                //Increment float opacity
                if (sideGlowOpacity < 1) sideGlowOpacity = model.sideGlowOpacity += 0.1f;

                //Set opacity on material
                if (isRailsPlatform) materials[1].SetFloat("Vector1_CDE54C4F", sideGlowOpacity);
                else materials[0].SetFloat("Vector1_A563CEE", sideGlowOpacity);


            }
            else
            {
                sideGlowOpacity = model.sideGlowOpacity;

                //Set opcaity on material
                if (isRailsPlatform) materials[1].SetFloat("Vector1_CDE54C4F", sideGlowOpacity);
                else materials[0].SetFloat("Vector1_A563CEE", sideGlowOpacity);
            }
        }

        else if (rtt.isOwnedLocallySelf && sideGlowOpacity > 0.6f)
        {
            sideGlowOpacity =  model.sideGlowOpacity -= 0.01f;
            
            //set opacity on material
            if (isRailsPlatform) materials[1].SetFloat("Vector1_CDE54C4F", sideGlowOpacity);
            else materials[0].SetFloat("Vector1_A563CEE", sideGlowOpacity);
        }

        else if (rtt.isOwnedRemotelySelf && sideGlowOpacity > 0.6f)
        {
            //set opacity on material
            if (isRailsPlatform) materials[1].SetFloat("Vector1_CDE54C4F", sideGlowOpacity);
            else materials[0].SetFloat("Vector1_A563CEE", sideGlowOpacity);
        }
    }
}
