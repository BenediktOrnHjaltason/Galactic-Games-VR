using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class GameManagerSync : RealtimeComponent<GameManagerSync_Model>
{
    protected override void OnRealtimeModelReplaced(GameManagerSync_Model previousModel, GameManagerSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.clientsDoneSpawningDidChange -= ClientsDoneSpawningDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.clientsDoneSpawning = 0;
            }

            // Update data to match the new model
            UpdateClientsDoneSpawning();


            //Register for events so we'll know if data changes later
            currentModel.clientsDoneSpawningDidChange += ClientsDoneSpawningDidChange;
        }
    }

    int clientsDoneSpawning = 0;

    public int ClientsDoneSpawning { get => clientsDoneSpawning; set => model.clientsDoneSpawning = value; }

    void ClientsDoneSpawningDidChange(GameManagerSync_Model model, int amount)
    {
        UpdateClientsDoneSpawning();
    }

    void UpdateClientsDoneSpawning()
    {
        clientsDoneSpawning = model.clientsDoneSpawning;
    }
}
