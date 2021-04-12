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


    List<int> teamMembers = new List<int>();
    public List<int> TeamMembers { get => teamMembers; }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < (int)teamSize + 1; i++)
            teamMembers.Add(-1);

        floor.material.SetColor("_BaseColor",teamColor);

        readyButton.OnExecute += HandleReadyness;

        GalacticGamesManager.Instance.TeamCreationPods.Add(this);
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

                        if (i == teamMembers.Count -1) capacityIndicator.material = fullCapacityMaterial;
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
                    }
                }
            }
        }
    }

    void HandleReadyness()
    {
        bool thisTeamReady = true;

        foreach (int teamMember in teamMembers)
            if (teamMember == -1) thisTeamReady = false;

        if (thisTeamReady) readyIndicator.material = fullCapacityMaterial; //Just reusing materials for prototyping

        bool allTeamsAreReady = true;

        foreach (TeamCreationPod pod in GalacticGamesManager.Instance.TeamCreationPods)
            if (pod.readyIndicator.material == availableCapacityMaterial) allTeamsAreReady = false;

        if (allTeamsAreReady) GalacticGamesManager.Instance.StartGame();
    }

}
