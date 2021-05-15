using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

/*
 Important to keep in mind that the game is not allowed to start if not all teams are filled with correct amount,
 set in the team creation pods.
 Only three players with same color will ever be active at the same time on the same client. And we only need to know if three individual players
 have passed throug the checkpoint 
 */
public class Checkpoint : RealtimeComponent<Checkpoint_Model>
{
    public static List<Checkpoint> instances = new List<Checkpoint>();

    [SerializeField]
    Vector3 teamIndicatorBasePos;

    [SerializeField]
    GameObject teamIndicatorPrefab;

    [SerializeField]
    Transform firstIndicatorPos;

    [SerializeField]
    Color teamIndicatorNullColor;

    [SerializeField]
    float teamIndicatorYOffsett;

    int numberOfTeamsPassedThrough = 0;

    Color teamColor;
    List<int> teamMembers = new List<int>();


    private void Awake()
    {
        if (instances.Count > 0) instances.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();
            PlayerSync ps = other.GetComponent<PlayerSync>();

            if (rv && ps)
            {
                teamColor = ps.PlayerTorso.material.GetColor("_BaseColor");

                if (!teamMembers.Contains(rv.ownerIDSelf))
                {
                    teamMembers.Add(rv.ownerIDSelf);

                    if (teamMembers.Count == TeamCreationPod.ColorToTeamSize[teamColor])
                        TeamPassedThrough = teamColor;
                }
            }
        }
    }

    //Networking

    protected override void OnRealtimeModelReplaced(Checkpoint_Model previousModel, Checkpoint_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.teamPassedThroughDidChange -= CreateTeamIndicator;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.teamPassedThrough = teamIndicatorNullColor;
            }

            //Register for events so we'll know if data changes later
            currentModel.teamPassedThroughDidChange += CreateTeamIndicator;
        }
    }

    Color TeamPassedThrough { set => model.teamPassedThrough = value; }

    void CreateTeamIndicator(Checkpoint_Model model, Color color)
    {
        if (model.teamPassedThrough == teamIndicatorNullColor) return;

        Debug.Log("CheckPoint: team color passed through network: " + model.teamPassedThrough);

        GameObject newIndicator = Instantiate<GameObject>(teamIndicatorPrefab);

        MeshRenderer mesh = newIndicator.GetComponent<MeshRenderer>();

        mesh.material.SetColor("_BaseColor", model.teamPassedThrough);

        newIndicator.transform.SetParent(transform.root);

        newIndicator.transform.position = firstIndicatorPos.position + new Vector3(0, teamIndicatorYOffsett * numberOfTeamsPassedThrough, 0);

        numberOfTeamsPassedThrough++;
    }
}
