using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class GameManagerSync : RealtimeComponent<GameManagerSync_Model>
{
    List<int> finishedPlayers = new List<int>();

    protected override void OnRealtimeModelReplaced(GameManagerSync_Model previousModel, GameManagerSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.clientsDoneSpawningDidChange -= ClientsDoneSpawningDidChange;
            previousModel.clientCrossedFinishLineDidChange -= RegisterFinishedPlayer;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.clientsDoneSpawning = 0;
                currentModel.clientCrossedFinishLine = -1;
            }

            // Update data to match the new model
            UpdateClientsDoneSpawning();


            //Register for events so we'll know if data changes later
            currentModel.clientsDoneSpawningDidChange += ClientsDoneSpawningDidChange;
            currentModel.clientCrossedFinishLineDidChange += RegisterFinishedPlayer;
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

    public int PlayerCrossedFinishLine { set => model.clientCrossedFinishLine = value; }

    void RegisterFinishedPlayer(GameManagerSync_Model Model, int clientID)
    {
        Debug.Log("GMSync: Player " + clientID + " crossed finish line");

        if (model.clientCrossedFinishLine != -1 && !finishedPlayers.Contains(model.clientCrossedFinishLine))
        {
            Debug.Log("GMSync: Player " + clientID + " WAS NOT registered before");

            finishedPlayers.Add(model.clientCrossedFinishLine);
            GalacticGamesManager.Instance.ReactivateFinishedPlayer(model.clientCrossedFinishLine);
        }

        else Debug.Log("GMSync: Player " + clientID + " WAS registered before. Aborting");
    }
}
