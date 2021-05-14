using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;
using System;

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

    public TextMeshPro PlayerName { get => playerNameTMPro; }

    [SerializeField]
    List<PlayerHeads> playerHeads;

    [SerializeField]
    MeshRenderer playerTorso;

    public MeshRenderer PlayerTorso { get => playerTorso; }

    bool isPlayerClient = false;

    int clientID;


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
        GameObject headMesh = Instantiate<GameObject>(playerHeads[headIndex].headMesh);

        headMesh.layer = 9;

        headMesh.transform.SetParent(this.transform);

        headMesh.transform.localPosition = playerHeads[headIndex].position;
        headMesh.transform.localRotation = Quaternion.Euler(playerHeads[headIndex].rotation);
        headMesh.transform.localScale = playerHeads[headIndex].scale;

        if (isPlayerClient)
        {
            headMesh.SetActive(false);
            playerNameTMPro.GetComponent<MeshRenderer>().enabled = false;
        }
        else StartCoroutine(SetVoiceAudioAttenuation());


        RealtimeView rtv = GetComponent<RealtimeView>();

        if (rtv)
        {
            clientID = rtv.ownerIDSelf;

            if (rtv) GalacticGamesManager.Instance.RegisterClientArrival(clientID);
        }

        

        
        //realtime.didDisconnectFromRoom += NotifyGameManagerOnLeavingRoom;
    }

    void NotifyGameManagerOnLeavingRoom(Realtime realtime)
    {
        //GalacticGamesManager.Instance.NetworkNotifyClientLeftRoom(clientID);
    }

    private void OnDisable()
    {
        Debug.Log("PS: OnDisable called when player quit game");
        GalacticGamesManager.Instance.NetworkNotifyClientLeftRoom(clientID);
    }

    IEnumerator SetVoiceAudioAttenuation()
    {
        while (GetComponent<AudioSource>() == null)
        {
            //Debug.Log("PlayerSync: Did NOT find AudioSource this iteration");
            yield return null;
        }

        AudioSource auS = GetComponent<AudioSource>();

        if (auS)
        {
            //Debug.Log("PlayerSync: Found audio source in coroutine. Setting attenuation settings");

            auS.spatialBlend = 1f;
            auS.volume = 0.6f;
            auS.rolloffMode = AudioRolloffMode.Linear;
            auS.maxDistance = 10000;
        }
    }
}
