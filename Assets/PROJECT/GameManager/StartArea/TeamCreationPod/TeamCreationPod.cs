using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

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

    //public static int teamSizeInt = 0;

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

    TextMeshPro readyText;

    [SerializeField]
    TextMeshPro teamSizeText;

    [SerializeField]
    Color AvatarTorsoDefault;

    Material teamNotReadyMaterial;
    Material teamReadyMaterial;

    [SerializeField]
    Animatic disappearAnimatic;

    [SerializeField]
    Material disappearMaterial;

    [SerializeField]
    TextMeshPro teamList;


    bool teamFilledUp = false;

    public bool TeamFilledUp { get => teamFilledUp; set => teamFilledUp = value; }

    bool teamEmpty = true;
    public bool TeamEmpty { get => teamEmpty; }

    bool readyToPlay;

    public bool ReadyToPlay { get => readyToPlay; set => readyToPlay = value; }

    public static List<TeamCreationPod> instances = new List<TeamCreationPod>();

    public static Dictionary<Color, int> ColorToTeamSize = new Dictionary<Color, int>();

    //For when players enter a pod when it's already full of members, and a member leaves
    List<RealtimeView> excessPlayersInCollider = new List<RealtimeView>();

    List<int> teamMembers = new List<int>();
    public List<int> TeamMembers { get => teamMembers; }

    Dictionary<int, string> memberClientIDToName = new Dictionary<int, string>();

    public Dictionary<int, string> MemberClientIDToName { get => memberClientIDToName; }

    private void Awake()
    {
        if (instances.Count > 0) instances.Clear();
        if (ColorToTeamSize.Count > 0) ColorToTeamSize.Clear();

        //if (teamSizeInt != 0) teamSizeInt = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);

        Debug.Log("TCP: Added this to instances. Count is now: " + instances.Count);

        int teamSizeInt = (int)teamSize + 1;

        ColorToTeamSize.Add(teamColor, teamSizeInt);

        //teamSizeInt = (int)teamSize + 1;

        for (int i = 0; i < teamSizeInt; i++)
            teamMembers.Add(-1);

        teamSizeText.text = teamSizeInt.ToString() + "\nPlayer" + (teamSizeInt > 1 ? "s" : "");

        teamList.text = "";

        floor.material.SetColor("_BaseColor",teamColor);

        readyButton.OnExecute += SetReady;


        teamNotReadyMaterial = availableCapacityMaterial;
        teamReadyMaterial = fullCapacityMaterial;

        readyText = readyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();

        disappearAnimatic.InitializeSequenceDelegates();

        disappearAnimatic.onSequenceStart[1] = MakePodInvisible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv) AttemptEnterPlayerInTeam(rv);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                //Remove excess player if he didn't become part of team
                if (excessPlayersInCollider.Contains(rv))
                {
                    excessPlayersInCollider.Remove(rv);
                    Debug.Log("TCP1: Removed non team player from queue");
                    return;
                }

                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if (teamMembers[i] == rv.ownerIDSelf)
                    {
                        teamMembers[i] = -1;

                        Debug.Log("TCP: removed " + rv.ownerIDSelf + " from " + transform.root.name);

                        PlayerSync ps = other.GetComponent<PlayerSync>();
                        if (ps)
                        {
                            ps.PlayerTorso.material.SetColor("_BaseColor", AvatarTorsoDefault);
                            memberClientIDToName.Remove(i);
                            ConstructTeamList();
                        }

                        capacityIndicator.material = availableCapacityMaterial;

                        teamFilledUp = readyToPlay = false;
                        readyIndicator.material = teamNotReadyMaterial;


                        //Insert potential place holders in team
                        if (excessPlayersInCollider.Count != 0)
                        {
                            RealtimeView temp = excessPlayersInCollider[0];
                            excessPlayersInCollider.RemoveAt(0);
                            Debug.Log("TCP1: Tranfering excess player from queue to team");

                            AttemptEnterPlayerInTeam(temp);
                        }


                        teamEmpty = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] != -1) teamEmpty = false;

                        /*
                        RaycastHit[] playersHit = Physics.SphereCastAll(transform.position + new Vector3(0, 0.4571f, 0), 1.4f, Vector3.zero, 0, 14);

                        foreach(RaycastHit playerObjectHit in playersHit)
                        {
                            Debug.Log("TCP1: Player standing in collider while other player exited");

                            if (playerObjectHit.collider.gameObject.name.Contains("Head"))
                            {
                                RealtimeView rtV = playerObjectHit.collider.GetComponent<RealtimeView>();

                                if (rtV) AttemptEnterPlayerInTeam(rtV, playerObjectHit.collider);
                            }
                        }*/

                        break;
                    }
                }
            }
        }
    }

    void AttemptEnterPlayerInTeam(RealtimeView rtv)
    {
        if (teamFilledUp)
        {
            excessPlayersInCollider.Add(rtv);
            Debug.Log("TCP1: Added excess player to queue");
            return;
        }

        for (int i = 0; i < teamMembers.Count; i++)
        {
            //Add player to team
            if (teamMembers[i] == -1 && !teamMembers.Contains(rtv.ownerIDSelf))
            {
                teamMembers[i] = rtv.ownerIDSelf;


                Debug.Log("TCP1: Entered player " + rtv.ownerIDSelf + " to team in " + transform.root.name);

                teamEmpty = false;

                PlayerSync ps = rtv.GetComponent<PlayerSync>();
                if (ps)
                {
                    ps.PlayerTorso.material.SetColor("_BaseColor", teamColor);
                    memberClientIDToName.Add(rtv.ownerIDSelf, ps.PlayerName.text);
                    ConstructTeamList();
                }

                teamFilledUp = true;
                for (int j = 0; j < teamMembers.Count; j++)
                    if (teamMembers[j] == -1) teamFilledUp = false;

                if (teamFilledUp) capacityIndicator.material = fullCapacityMaterial;

                break;
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

        int readyTeams = 0;
        int emptyPods = 0;

        foreach (TeamCreationPod pod in instances)
        {
            if (pod.readyToPlay) readyTeams++;
            else if (pod.teamEmpty) emptyPods++;
        }

        bool allPodsReadyTeamsOrEmpty = (readyTeams + emptyPods) == instances.Count;

        bool allPlayersAccountedFor = GalacticGamesManager.Instance.AllPlayersAccountedFor();

        if (allPodsReadyTeamsOrEmpty && allPlayersAccountedFor)
        {
            foreach (TeamCreationPod pod in instances)
                if (pod.readyToPlay) pod.readyText.text = "GO!";


            Debug.Log("TCP: All pods have ready teams or is empty and all players accounted for. Calling GameManager::StartGame()");
            GalacticGamesManager.Instance.StartGame();
        }
        else
        {
            if (!allPodsReadyTeamsOrEmpty) Debug.Log("GGM: Cannot start game because all pods do not have ready teams or are empty");
            if (!allPlayersAccountedFor) Debug.Log("GGM: Cannot start game because not all players are accounted for");
        }
    }

    public void OnTeamMemberLeftRoom(int teamMemberIndex)
    {
        Debug.Log("TCP: OnTeamMemberLeftRoom called with team member index " + teamMemberIndex);

        if (memberClientIDToName.ContainsKey(teamMembers[teamMemberIndex]))
            Debug.Log("TCP: ClientID was found as key in dictionary and points to " + memberClientIDToName[teamMembers[teamMemberIndex]]);

        else
        {
            Debug.Log("TCP: ClientID was NOT found in dictionary. teamMembers[teamMemberIndex] contains clientID " + teamMembers[teamMemberIndex] + 
                " and memberClientIDToName contains clientID");

            foreach (KeyValuePair<int, string> pair in memberClientIDToName)
            {
                Debug.Log(pair.Key);
            }
        }

        memberClientIDToName.Remove(teamMembers[teamMemberIndex]);
        ConstructTeamList();

        teamMembers[teamMemberIndex] = -1;
        teamFilledUp = readyToPlay = false;

        readyIndicator.material = teamNotReadyMaterial;
    }

    public void DisableReadyButton()
    {
        readyButton.gameObject.layer = 9; //Ignore
    }

    //Disappear animatic

    public void StartDisappearAnimatic()
    {
        capacityIndicator.material = disappearMaterial;
        disappearAnimatic.Run();
    }

    void MakePodInvisible()
    {
        teamSizeText.gameObject.SetActive(false);
        floor.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
    }

    void ConstructTeamList()
    {
        string temp = "";

        foreach (KeyValuePair<int,string> pair in memberClientIDToName)
        {
            temp += '\n' + pair.Value; 
        }

        teamList.text = temp;
    }
}
