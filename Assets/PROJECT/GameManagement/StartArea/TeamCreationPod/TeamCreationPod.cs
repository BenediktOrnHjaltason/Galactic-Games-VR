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

    List<int> teamMembers = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < (int)teamSize + 1; i++)
            teamMembers.Add(-1);

        floor.material.SetColor("_BaseColor",teamColor);

        GalacticGamesManager.Instance.TeamCreationPods.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.tag == "PlayerHead")
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
}
