using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class StructureSync : RealtimeComponent<StructureSync_Model>
{

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

    /// <summary>
    /// Is structure available for being moved/rotated/replicated with gun etc? Used to guarantee that only one player
    /// at a time can manipulate this structure
    /// </summary>
    bool availableToManipulate = true;

    public bool AvailableToManipulate { get => availableToManipulate; set => model.availableToManipulate = value; }


    private void PlayersOccupyingDidChange(StructureSync_Model model, int playersUsingStructure)
    {
        UpdatePlayersOccupying();
    }

    private void AvailableToManipulateDidChange(StructureSync_Model model, bool available)
    {
        UpdateAvailableToManipulate();
    }

    protected override void OnRealtimeModelReplaced(StructureSync_Model previousModel, StructureSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playersOccupyingDidChange -= PlayersOccupyingDidChange;
            previousModel.availableToManipulateDidChange -= AvailableToManipulateDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.playersOccupying = playersOccupying;
                currentModel.availableToManipulate = availableToManipulate;
            }

            // Update data to match the new model
            UpdatePlayersOccupying();
            UpdateAvailableToManipulate();

            // Register for events so we'll know if data changes later
            currentModel.playersOccupyingDidChange += PlayersOccupyingDidChange;
            currentModel.availableToManipulateDidChange += AvailableToManipulateDidChange;
        }
    }

    private void UpdatePlayersOccupying()
    {
        playersOccupying = model.playersOccupying;
    }

    private void UpdateAvailableToManipulate()
    {
        availableToManipulate = model.availableToManipulate;
    }
}
