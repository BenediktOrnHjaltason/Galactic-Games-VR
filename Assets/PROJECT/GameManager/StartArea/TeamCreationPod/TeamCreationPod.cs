using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

enum ETeamCreationPod
{
    OnePlayer,
    TwoPlayers,
    ThreePlayers
}

public class TeamCreationPod : MonoBehaviour
{
    [SerializeField]
    ETeamCreationPod teamSize;

    [SerializeField]
    Color teamColor;

    [SerializeField]
    MeshRenderer capacityIndicator;

    [SerializeField]
    Material availableCapacityMaterial;

    [SerializeField]
    Material fullCapacityMaterial;

    [SerializeField]
    MeshRenderer floor;

    [SerializeField]
    MeshRenderer readyIndicator;

    [SerializeField]
    InteractButton readyButton;

    Material teamNotReadyMaterial;
    Material teamReadyMaterial;

    bool teamFilledUp = false;

    public bool TeamFilledUp { get => teamFilledUp; }

    bool teamEmpty = true;

    bool readyToPlay;


    List<int> teamMembers = new List<int>();
    public List<int> TeamMembers { get => teamMembers; }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < (int)teamSize + 1; i++)
            teamMembers.Add(-1);

        floor.material.SetColor("_BaseColor",teamColor);

        readyButton.OnExecute += SetReady;

        GalacticGamesManager.Instance.TeamCreationPods.Add(this);

        Debug.Log("TCP: Added this to game managers collection");

        Material teamNotReadyMaterial = availableCapacityMaterial;
        Material teamReadyMaterial = fullCapacityMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head"))
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    //Add player to team
                    if (teamMembers[i] == -1)
                    {
                        teamMembers[i] = rv.ownerIDSelf;
                        teamEmpty = false;

                        teamFilledUp = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] == -1) teamFilledUp = false;

                        if (teamFilledUp) capacityIndicator.material = fullCapacityMaterial;
                        
                        break;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head"))
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if (teamMembers[i] == rv.ownerIDSelf)
                    {
                        teamMembers[i] = -1;

                        capacityIndicator.material = availableCapacityMaterial;

                        teamFilledUp = readyToPlay = false;
                        readyIndicator.material = teamNotReadyMaterial;

                        teamEmpty = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] != -1) teamEmpty = false;

                            break;
                    }
                }
            }
        }
    }

    void SetReady()
    {
        Debug.Log("TCP: SetReady() called");

        readyToPlay = teamFilledUp;

        if (readyToPlay) readyIndicator.material = teamReadyMaterial;
        else
        {
            Debug.Log("TCP: Team in " + name + " is not ready to play. Aborting");
            return;
        }

        bool allTeamsReadyOrEmpty = true;

        foreach (TeamCreationPod pod in GalacticGamesManager.Instance.TeamCreationPods)
        {
            if (!pod.readyToPlay) allTeamsReadyOrEmpty = false;
            if (pod.teamEmpty) allTeamsReadyOrEmpty = true;
        }

        bool allPlayersAccountedFor = GalacticGamesManager.Instance.AllPlayersAccountedFor();

        if (allTeamsReadyOrEmpty && allPlayersAccountedFor)
        {
            Debug.Log("TCP: All pods have ready teams or is empty and all players accouted for. Calling GameManager::StartGame()");
            GalacticGamesManager.Instance.StartGame();
        }
        else
        {
            if (!allTeamsReadyOrEmpty) Debug.Log("GGM: Cannot start game because all teams are not ready or empty");
            if (!allPlayersAccountedFor) Debug.Log("GGM: Cannot start game because not all players are accounted for");
        }
    }
}
