using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Puzzle_DysonSphere : MonoBehaviour
{
    [SerializeField]
    Star_Pulsar star;

    [SerializeField]
    float starAttractionForce = 30000.0f;


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


    // Start is called before the first frame update
    void Start()
    {
        leftPieceStartLocalPos = leftDysonPiece.transform.localPosition;
        leftPieceStartLocalRot = leftDysonPiece.transform.localRotation;

        rightPieceStartLocalPos = rightDysonPiece.transform.localPosition;
        rightPieceStartLocalRot = rightDysonPiece.transform.localRotation;

        if (star)
        {
            star.OnDysonPieceTouches += ResetDysonPieceToStart;
        }

        if (magneticFieldTrigger)
        {
            magneticFieldTrigger.OnEnteredTrigger += RegisterDysonPieceEnteredMagneticField;
            magneticFieldTrigger.OnExitedTrigger += RegisterDysonPieceExitedMagneticField;
        }
    }

    private void FixedUpdate()
    {
        foreach (StructureSync ss in piecesInsideMagneticField)
        {
            ss.AddGravityForce((star.transform.position - ss.transform.position).normalized * starAttractionForce);
        }
    }

    void ResetDysonPieceToStart(DysonSpherePiece dysonPiece)
    {
        if (dysonPiece.Rtt.realtime.clientID == dysonPiece.Rtt.ownerIDSelf)
        {
            StructureSync ss = dysonPiece.GetComponent<StructureSync>();

            if (ss)
            {
                if (piecesInsideMagneticField.Contains(ss)) piecesInsideMagneticField.Remove(ss);
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

            if (ss && ss.Rtt.realtime.clientID == ss.Rtt.ownerIDSelf && !piecesInsideMagneticField.Contains(ss))
                piecesInsideMagneticField.Add(ss);
        }
    }

    void RegisterDysonPieceExitedMagneticField(Collider other)
    {
        DysonSpherePiece dysonPiece = other.gameObject.GetComponentInParent<DysonSpherePiece>();

        if (dysonPiece)
        {
            StructureSync ss = dysonPiece.GetComponent<StructureSync>();

            if (ss && ss.Rtt.realtime.clientID == ss.Rtt.ownerIDSelf && piecesInsideMagneticField.Contains(ss))
                piecesInsideMagneticField.Remove(ss);
        }
    }
}
