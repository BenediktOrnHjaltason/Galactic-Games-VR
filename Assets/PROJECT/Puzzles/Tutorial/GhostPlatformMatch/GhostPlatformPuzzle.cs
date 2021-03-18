using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;
using System;

enum EOperationPhase
{
    DETECTALLIGNMENT,
    AKNOWLEDGE,
    MOVE
}

public class GhostPlatformPuzzle : RealtimeComponent<GhostPlatformPuzzle_Model>
{
    Vector3 leftHalfCenter;

    Vector3 rightHalfCenter;


    [SerializeField]
    RealtimeTransform platformRtt;

    RealtimeTransform thisRtt;

    [SerializeField]
    AnimationCurve easeInFastOut;

    [SerializeField]
    AnimationCurve detectionBump;

    [SerializeField]
    TextMeshPro text1;

    [SerializeField]
    TextMeshPro text2;

    [SerializeField]
    Transform rollGizmoBase;

    MeshRenderer rollGizmoMesh;

    [SerializeField]
    Transform pitchGizmoBase;

    MeshRenderer pitchGizmoMesh;

    [SerializeField]
    Transform yawGizmoBase;

    MeshRenderer yawGizmoMesh;

    [SerializeField]
    Transform playerHeadAnchor;


    [SerializeField]
    Animatic helperCraftAnimatic;

    [SerializeField]
    Vehicle_TicTac helperCraft;


    Realtime realTime;

    Vector3 platformToGhost;

    float forwardAllignment;
    float rightAllignment;


    float increment = 0;

    Vector3 baseLocalScale;

    //false for left, true for right
    bool leftOrRight = false;

    Quaternion oldRotation;
    Quaternion newRotation;

    //----- Operation
    EOperationPhase phase = EOperationPhase.DETECTALLIGNMENT;


    void Start()
    {
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();

        thisRtt = GetComponent<RealtimeTransform>();

        leftHalfCenter = platformRtt.transform.localPosition;
        rightHalfCenter = transform.localPosition;

        baseLocalScale = transform.localScale;

        helperCraftAnimatic.TestEvent += ProvideProgressPlatforms;

        helperCraftAnimatic.OnAnimaticEnds += SetHelperCraftInvisible;

        rollGizmoMesh = rollGizmoBase.GetComponentInChildren<MeshRenderer>();
        pitchGizmoMesh = pitchGizmoBase.GetComponentInChildren<MeshRenderer>();
        yawGizmoMesh = yawGizmoBase.GetComponentInChildren<MeshRenderer>();

        StructureSync platformSS = platformRtt.GetComponent<StructureSync>();

        if (platformSS)
        {
            platformSS.OnControlTaken += EnableRotateGizmo;
            platformSS.OnControlReleased += DisableRotateGizmo;
        }
    }

    float platformRollForce;
    float platformYawForce;
    float platformPitchForce;

    void SamplePlatformRotationForces(float roll, float yaw, float pitch)
    {
        platformRollForce = roll; platformYawForce = yaw; platformPitchForce = pitch;
    }

    private void Update()
    {
        if (animateRotateGizmo)
        {
            Vector3 playerHeadToPlatform = platformRtt.transform.position - playerHeadAnchor.transform.position;
            rollGizmoBase.rotation = pitchGizmoBase.rotation = Quaternion.LookRotation(playerHeadToPlatform);
            yawGizmoBase.rotation = Quaternion.LookRotation(Vector3.up);

            rollGizmoMesh.transform.localRotation *= Quaternion.Euler(0.0f, 0.0f, platformRollForce / 30);
            yawGizmoMesh.transform.localRotation *= Quaternion.Euler(0.0f, 0.0f, platformYawForce / 30);
            pitchGizmoMesh.transform.localRotation *= Quaternion.Euler(0.0f, 0.0f, platformPitchForce / 30);
        }
    }

    private void FixedUpdate()
    {
        
        //If we are not connected to server we cannot request ownerships
        if (!realTime.connected) return;


        //Someone needs to have ownership of ghost platform to animate it, and first dibs rules
        if (thisRtt.ownerIDSelf == -1) thisRtt.RequestOwnership();


        else if (thisRtt.ownerIDSelf == realTime.clientID)
        {
            switch (phase)
            {
                case EOperationPhase.DETECTALLIGNMENT:

                    transform.localScale = baseLocalScale + (Vector3.one * Mathf.Abs(Mathf.Sin(Time.time * 5)) / 5);

                    platformToGhost = transform.position - platformRtt.transform.position;

                    //Position match
                    if (platformToGhost.sqrMagnitude < 1.6f)
                    {
                        //Rotation match
                        forwardAllignment = Vector3.Dot(transform.forward, platformRtt.transform.forward);
                        rightAllignment = Vector3.Dot(transform.right, platformRtt.transform.right);

                        if ((forwardAllignment < -0.8f || forwardAllignment > 0.8f) && (rightAllignment < -0.8f || rightAllignment > 0.8f))
                        {
                            phase = EOperationPhase.AKNOWLEDGE;
                            MatchesLeftToWin--;
                        }
                    }
                    break;

                case EOperationPhase.AKNOWLEDGE:

                    if (increment < 1)
                    {
                        increment += 0.05f;

                        transform.localScale = baseLocalScale + (Vector3.one * detectionBump.Evaluate(increment));
                    }

                    else if (increment > 1)
                    {
                        increment = 0;

                        transform.localScale = baseLocalScale;
                        oldRotation = transform.localRotation;
                        newRotation =
                            Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0.0f, 360.0f),
                                                         UnityEngine.Random.Range(0.0f, 360.0f),
                                                         UnityEngine.Random.Range(0.0f, 360.0f)));

                        phase = EOperationPhase.MOVE;
                    }

                    break;

                case EOperationPhase.MOVE:

                    if (increment < 1)
                    {
                        if (leftOrRight) transform.localPosition = Vector3.Lerp(leftHalfCenter, rightHalfCenter, easeInFastOut.Evaluate(increment));
                        else transform.localPosition = Vector3.Lerp(rightHalfCenter, leftHalfCenter, easeInFastOut.Evaluate(increment));

                        transform.localRotation = Quaternion.Lerp(oldRotation, newRotation, increment);

                        increment += 0.05f;
                    }
                    else if (increment > 1)
                    {
                        phase = EOperationPhase.DETECTALLIGNMENT;
                        increment = 0;
                        leftOrRight = !leftOrRight;
                    }

                    break;
            }
        }
        

        if (provideProgressPlatforms)
        {
            if (Time.time > nextTimeToFire && platformsGiven < numberOfPlatformsToGive)
            {
                SpawnPlatform();

                if (platformsGiven == numberOfPlatformsToGive) provideProgressPlatforms = false;
            }
        }
    }

    bool provideProgressPlatforms = false;
    float numberOfPlatformsToGive = 4;
    float fireIncrement = 0;
    int platformsGiven = 0;
    float nextTimeToFire = 0;

    void ProvideProgressPlatforms()
    {
        provideProgressPlatforms = true;
        fireIncrement = (helperCraftAnimatic.movementSequences[1].duration / numberOfPlatformsToGive) - 0.2f;

        SpawnPlatform();
    }

    void SpawnPlatform()
    { 

        nextTimeToFire = Time.time + fireIncrement;
        platformsGiven++;


        GameObject platform = Realtime.Instantiate("PF_SquarePlatform1",
                                                    ownedByClient: false,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

        platform.GetComponent<RealtimeTransform>().RequestOwnership();

        Vector3 fireDirection = helperCraft.transform.forward - helperCraft.transform.up;
        platform.transform.position = helperCraft.transform.position + fireDirection;

        platform.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0.0f, 360.0f),
                                                         UnityEngine.Random.Range(0.0f, 360.0f),
                                                         UnityEngine.Random.Range(0.0f, 360.0f)));

        Rigidbody rb = platform.GetComponent<Rigidbody>();

        rb.AddForce(fireDirection * 2500);

    }

    bool animateRotateGizmo = false;

    void EnableRotateGizmo()
    {
        if (platformRtt.ownerIDSelf == realTime.clientID)
            animateRotateGizmo = rollGizmoMesh.enabled = pitchGizmoMesh.enabled = yawGizmoMesh.enabled = true;

        StructureSync platformSS = platformRtt.GetComponent<StructureSync>();

        if (platformSS) platformSS.OnExternalPiggybacking += SamplePlatformRotationForces;
    }

    void DisableRotateGizmo()
    {
        if (platformRtt.ownerIDSelf == realTime.clientID)
            animateRotateGizmo = rollGizmoMesh.enabled = pitchGizmoMesh.enabled = yawGizmoMesh.enabled = false;

        StructureSync platformSS = platformRtt.GetComponent<StructureSync>();

        if (platformSS) platformSS.OnExternalPiggybacking -= SamplePlatformRotationForces;
    }

    //-------------Networking------------//

    protected override void OnRealtimeModelReplaced(GhostPlatformPuzzle_Model previousModel, GhostPlatformPuzzle_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.matchesLeftToWinDidChange -= MatchesLeftToWinDidChange;
            previousModel.helperCraftVisibleDidChange -= HelperCraftVisibleDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.matchesLeftToWin = matchesLeftToWin;
                currentModel.helperCraftVisible = false;
            }

            // Update data to match the new model
            UpdateMatchesLeftToWin();
            UpdateHelperCraftVisible();

            //Register for events so we'll know if data changes later
            currentModel.matchesLeftToWinDidChange += MatchesLeftToWinDidChange;
            currentModel.helperCraftVisibleDidChange += HelperCraftVisibleDidChange;
        }
    }

    int matchesLeftToWin = 5;

    int MatchesLeftToWin 
    { 
        get => matchesLeftToWin;

        set
        {
            if (value >= 0 ) model.matchesLeftToWin = value;
        }
    }

    void MatchesLeftToWinDidChange(GhostPlatformPuzzle_Model model, int number)
    {
        UpdateMatchesLeftToWin();
    }

    void UpdateMatchesLeftToWin()
    {
        matchesLeftToWin = model.matchesLeftToWin;

        if (matchesLeftToWin > 0)
        {
            text1.text = text2.text = matchesLeftToWin.ToString();
        }

        else if (matchesLeftToWin == 0 && realTime.connected)
        {
            text1.transform.GetComponent<MeshRenderer>().enabled =
            text2.transform.GetComponent<MeshRenderer>().enabled = false;

            if (thisRtt.ownerIDSelf == realTime.clientID)
            {
                HelperCraftVisible = true;

                helperCraft.GetComponent<RealtimeTransform>().SetOwnership(thisRtt.ownerIDSelf);

                helperCraftAnimatic.Run();
            }
        }
    }

    bool HelperCraftVisible { set => model.helperCraftVisible = value; }

    void HelperCraftVisibleDidChange(GhostPlatformPuzzle_Model model, bool visible)
    {
        UpdateHelperCraftVisible();
    }

    void UpdateHelperCraftVisible()
    {
        bool visible = model.helperCraftVisible;

        if (helperCraft) helperCraft.SetVisibility(visible);
    }

    void SetHelperCraftInvisible()
    {
        HelperCraftVisible = false;
    }
}
