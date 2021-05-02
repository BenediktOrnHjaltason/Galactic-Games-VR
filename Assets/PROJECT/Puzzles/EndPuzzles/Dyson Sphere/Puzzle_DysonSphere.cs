using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class Puzzle_DysonSphere : MonoBehaviour
{
    [SerializeField]
    Star_Pulsar star;

    [SerializeField]
    float starAttractionForce = 590000.0f;


    [SerializeField]
    DysonSpherePiece leftDysonPiece;

    Vector3 leftPieceStartLocalPos;
    Quaternion leftPieceStartLocalRot;

    [SerializeField]
    DysonSpherePiece rightDysonPiece;

    Vector3 rightPieceStartLocalPos;
    Quaternion rightPieceStartLocalRot;

    [SerializeField]
    GeneralTrigger magneticFieldTrigger;

    List<StructureSync> piecesInsideMagneticField = new List<StructureSync>();

    [SerializeField]
    GeneralTrigger portalTrigger;

    [SerializeField]
    GameObject portal;

    [SerializeField]
    Transform playerTeleportTo;

    Vector3 portalEndScale = new Vector3(2.015517f, 6.777942f, 2.015517f);


    // Start is called before the first frame update
    void Start()
    {
        if (leftDysonPiece && rightDysonPiece)
        {
            leftPieceStartLocalPos = leftDysonPiece.transform.localPosition;
            leftPieceStartLocalRot = leftDysonPiece.transform.localRotation;

            rightPieceStartLocalPos = rightDysonPiece.transform.localPosition;
            rightPieceStartLocalRot = rightDysonPiece.transform.localRotation;

            leftDysonPiece.OnEnergyExtractionComplete += ReleaseLevelEndPortal;
            rightDysonPiece.OnEnergyExtractionComplete += ReleaseLevelEndPortal;
        }

        

        if (star)
        {
            star.OnDysonPieceTouches += ResetDysonPieceToStart;
        }

        if (magneticFieldTrigger)
        {
            magneticFieldTrigger.OnEnteredTrigger += RegisterDysonPieceEnteredMagneticField;
            magneticFieldTrigger.OnExitedTrigger += RegisterDysonPieceExitedMagneticField;
        }

        if (portalTrigger)
        {
            portalTrigger.OnEnteredTrigger += TeleportPlayer;
        }
    }

    private void FixedUpdate()
    {
        foreach (StructureSync ss in piecesInsideMagneticField)
        {
            if (ss.Rtt.ownerIDSelf == ss.Rtt.realtime.clientID)
                    ss.AddGravityForce((star.transform.position - ss.transform.position).normalized * starAttractionForce);
        }

        if (levelEndPortalReleased)
        {
            if (portalIncrement < 1.0f)
            {
                portalIncrement += 0.02f;

                portal.transform.localScale = Vector3.Lerp(Vector3.zero, portalEndScale, portalIncrement);
            }
        }
    }

    void ResetDysonPieceToStart(DysonSpherePiece dysonPiece)
    {
        StructureSync ss = dysonPiece.GetComponent<StructureSync>();

        if (ss)
        {
            if (piecesInsideMagneticField.Contains(ss)) piecesInsideMagneticField.Remove(ss);

            if (dysonPiece.Rtt.realtime.clientID == dysonPiece.Rtt.ownerIDSelf)
            {
                ss.ResetLinearVelocity();
                ss.BreakControl();
                dysonPiece.transform.localPosition = (dysonPiece == leftDysonPiece) ? leftPieceStartLocalPos : rightPieceStartLocalPos;
                dysonPiece.transform.localRotation = (dysonPiece == leftDysonPiece) ? leftPieceStartLocalRot : rightPieceStartLocalRot; 
            }
        }
    }

    void RegisterDysonPieceEnteredMagneticField(Collider other)
    {
        DysonSpherePiece dysonPiece = other.gameObject.GetComponentInParent<DysonSpherePiece>();

        if (dysonPiece)
        {
            StructureSync ss = dysonPiece.GetComponent<StructureSync>();

            if (!piecesInsideMagneticField.Contains(ss))
                    piecesInsideMagneticField.Add(ss);
        }
    }

    void RegisterDysonPieceExitedMagneticField(Collider other)
    {
        DysonSpherePiece dysonPiece = other.gameObject.GetComponentInParent<DysonSpherePiece>();

        if (dysonPiece)
        {
            StructureSync ss = dysonPiece.GetComponent<StructureSync>();

            if (piecesInsideMagneticField.Contains(ss))
                    piecesInsideMagneticField.Remove(ss);
        }
    }

    bool levelEndPortalReleased = false;
    float portalIncrement = 0.0f;

    void ReleaseLevelEndPortal()
    {
        if (!levelEndPortalReleased)
        {
            levelEndPortalReleased = true;

            leftDysonPiece.OnEnergyExtractionComplete -= ReleaseLevelEndPortal;
            rightDysonPiece.OnEnergyExtractionComplete -= ReleaseLevelEndPortal;
        }
    }

    void TeleportPlayer(Collider other)
    {
        OVRPlayerController pc = other.GetComponent<OVRPlayerController>();

        if (pc)
        {
            pc.Controller.enabled = false;
            pc.transform.position = playerTeleportTo.position;
            pc.Controller.enabled = true;
        }
    }
}
