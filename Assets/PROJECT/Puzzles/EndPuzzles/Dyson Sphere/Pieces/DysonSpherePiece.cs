using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using System;

public class DysonSpherePiece : RealtimeComponent<DysonSpherePiece_Model>
{
    [SerializeField]
    LineRenderer beamToStar;

    [SerializeField]
    Material searchMaterial;

    [SerializeField]
    Material connectedMaterial;

    [SerializeField]
    RealtimeTransform rtt;

    public RealtimeTransform Rtt { get => rtt; }

    float starBeamLength = 17.5f;


    static bool bothPiecesConnectedToStar = false;


    [SerializeField]
    DysonSpherePiece otherPiece;

    [SerializeField]
    LineRenderer connectorBeamToOtherPiece;

    [SerializeField]
    GameObject otherPieceConnectionReceiver;

    [SerializeField]
    Material zeroLoadMaterial;

    [SerializeField]
    Material fullLoadMaterial;

    static float energyExtractionIncrement = 0;
    static bool energyExtractionComplete = false;

    public event Action OnEnergyExtractionComplete;

    RaycastHit structureHit;

    public event Action OnConnectedToStar;


    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, starBeamLength / 2, 1 << 15))
        {
            if (!connectedToStar)
            {
                ConnectedToStar = true;
                OnConnectedToStar?.Invoke();
            }
        }

        else if (connectedToStar) ConnectedToStar = false;

        beamToStar.SetPosition(0, transform.position);
        beamToStar.SetPosition(1, transform.position + transform.forward * starBeamLength);

        if (bothPiecesConnectedToStar)
        {
            connectorBeamToOtherPiece.SetPosition(0, connectorBeamToOtherPiece.transform.position);
            connectorBeamToOtherPiece.SetPosition(1, otherPieceConnectionReceiver.transform.position);

            if (!energyExtractionComplete && energyExtractionIncrement < 1)
            {
                energyExtractionIncrement += 0.001f;

                connectorBeamToOtherPiece.material.Lerp(zeroLoadMaterial, fullLoadMaterial, energyExtractionIncrement);
            }
            else if (!energyExtractionComplete)
            {
                energyExtractionComplete = true;
                OnEnergyExtractionComplete?.Invoke();
            }
        }

        else
        {
            connectorBeamToOtherPiece.SetPosition(0, connectorBeamToOtherPiece.transform.position);
            connectorBeamToOtherPiece.SetPosition(1, connectorBeamToOtherPiece.transform.position);
        }
    }

    //Networking

    protected override void OnRealtimeModelReplaced(DysonSpherePiece_Model previousModel, DysonSpherePiece_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.connectedToStarDidChange -= ConnectedToStarDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.connectedToStar = connectedToStar;
            }

            //Update data to match the new model
            UpdateConnectedToStar();

            // Register for events so we'll know if data changes later
            currentModel.connectedToStarDidChange += ConnectedToStarDidChange;
        }
    }

    bool connectedToStar = false;

    public bool ConnectedToStar { get => connectedToStar; set => model.connectedToStar = value; }

    void ConnectedToStarDidChange(DysonSpherePiece_Model model, bool connected)
    {
        UpdateConnectedToStar();
    }

    void UpdateConnectedToStar()
    {
        connectedToStar = model.connectedToStar;

        beamToStar.material = (connectedToStar) ? connectedMaterial : searchMaterial;

        if (connectedToStar && otherPiece.ConnectedToStar)
            bothPiecesConnectedToStar = true;

        else
        {
            bothPiecesConnectedToStar = false;
            energyExtractionIncrement = 0.0f;
            connectorBeamToOtherPiece.material = zeroLoadMaterial;
        }
    }
}
