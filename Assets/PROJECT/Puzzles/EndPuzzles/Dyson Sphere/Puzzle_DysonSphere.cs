using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Puzzle_DysonSphere : MonoBehaviour
{
    [SerializeField]
    Star_Pulsar star;


    [SerializeField]
    DysonSpherePiece leftDysonPiece;

    Vector3 leftPieceStartLocalPos;

    [SerializeField]
    DysonSpherePiece rightDysonPiece;

    Vector3 rightPieceStartLocalPos;


    // Start is called before the first frame update
    void Start()
    {
        leftPieceStartLocalPos = leftDysonPiece.transform.localPosition;
        rightPieceStartLocalPos = rightDysonPiece.transform.localPosition;

        if (star)
        {
            star.OnDysonPieceTouches += ResetDysonPieceToStart;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetDysonPieceToStart(DysonSpherePiece dsp)
    {
        if (dsp.Rtt.realtime.clientID == dsp.Rtt.ownerIDSelf)
        {
            StructureSync ss = dsp.GetComponent<StructureSync>();

            if (ss)
            {
                ss.BreakControl();
                dsp.transform.localPosition = (dsp == leftDysonPiece) ? leftPieceStartLocalPos : rightPieceStartLocalPos;
            }
        }
    }
}
