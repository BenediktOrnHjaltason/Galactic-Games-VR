using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

[System.Serializable]
public struct PlayerHeads
{
    public GameObject headMesh;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

public class PlayerSync : RealtimeComponent<PlayerSync_Model>
{
    [SerializeField]
    TextMeshPro playerNameTMPro;

    [SerializeField]
    List<PlayerHeads> playerHeads;

    bool isPlayerClient = false;

    
    protected override void OnRealtimeModelReplaced(PlayerSync_Model previousModel, PlayerSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.playerNameDidChange -= PlayerNameDidChange;
            previousModel.playerHeadIndexDidChange -= PlayerHeadIndexDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                isPlayerClient = true;

                currentModel.playerName = PlayerPrefs.GetString("playerName");
                currentModel.playerHeadIndex = PlayerPrefs.GetInt("currentHeadIndex");
            }

            // Update data to match the new model
            UpdatePlayerName();
            InstantiatePlayerHead();


            //Register for events so we'll know if data changes later
            currentModel.playerNameDidChange += PlayerNameDidChange;
            currentModel.playerHeadIndexDidChange += PlayerHeadIndexDidChange;
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

    //---------------------------------------------------------

    public int PlayerHead { set => model.playerHeadIndex = value; }

    void PlayerHeadIndexDidChange(PlayerSync_Model model, int index)
    {
        InstantiatePlayerHead();
    }

    void InstantiatePlayerHead()
    {
        int headIndex = model.playerHeadIndex;
        GameObject head = Instantiate<GameObject>(playerHeads[headIndex].headMesh);
        head.transform.SetParent(this.transform);

        head.transform.localPosition = playerHeads[headIndex].position;
        head.transform.localRotation = Quaternion.Euler(playerHeads[headIndex].rotation);
        head.transform.localScale = playerHeads[headIndex].scale;

        if (isPlayerClient)
        {
            head.SetActive(false);
            playerNameTMPro.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
