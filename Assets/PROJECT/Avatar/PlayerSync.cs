using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

public class PlayerSync : RealtimeComponent<PlayerSync_Model>
{
    [SerializeField]
    TextMeshPro playerNameTMPro;

    
    protected override void OnRealtimeModelReplaced(PlayerSync_Model previousModel, PlayerSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playerNameDidChange -= PlayerNameDidChange;


        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.playerName = PlayerPrefs.GetString("playerName");
            }

            // Update data to match the new model
            UpdatePlayerName();


            //Register for events so we'll know if data changes later
            currentModel.playerNameDidChange += PlayerNameDidChange;
        }
    }
    

    public string PlayerName { set => model.playerName = value; }

    void PlayerNameDidChange(PlayerSync_Model model, string name)
    {
        UpdatePlayerName();
    }

    void UpdatePlayerName()
    {
        playerNameTMPro.text = model.playerName;
    }

    public void SetPlayerNameFromPlayerPrefs(Realtime realtime)
    {
        PlayerName = PlayerPrefs.GetString("playerName");
    }

}
