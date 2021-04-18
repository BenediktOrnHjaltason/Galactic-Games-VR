using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Unity.Profiling;

/*
    The player sensor is the large field detecting if players are in area of influence,
    and pulling them towards the core if they are
 */


public class AttractorRift_PlayerSensor : MonoBehaviour
{

    [SerializeField]
    float autoForce;

    [SerializeField]
    AttractorRift_Core core;

    GameObject anchor;

    RealtimeTransform rtt;
    Rigidbody rb;

    
    //Only one instance of OVRPlayerController exists per client, which is used to apply pulling towards the core.
    OVRPlayerController playerControllerInReach = null;


    //Rifts need access to each other to cancel the pull from other rifts if in the influence of other rifts when
    //player touches the core of this rift
    static List<AttractorRift_PlayerSensor> allSensors = new List<AttractorRift_PlayerSensor>();

    //Since OVRPlayerController is local only, we need another way of displaying the beam that pulls them
    List<GameObject> playerHeads = new List<GameObject>();

    //---- beams
    float changeInterval = 0.466f;
    float nextTimeToChange = 0;
    GameObject dummyObject;

    //--------- Default beams
    List<LineRenderer> defaultBeams = new List<LineRenderer>();

    Vector3 pathToEdge;
    Vector3 randomDirection;
    Vector3 directionToPlayerTorso;

    Vector3 offsettToPlayerHead = new Vector3(0, -0.3f, 0);

    Vector3[] offsetRight = new Vector3[4];
    Vector3[] offsetUp = new Vector3[4];

    Vector3[] endPointsInitial = new Vector3[4];
    Vector3[] endPointsMoveTo = new Vector3[4];

    //--------- PlayerLines
    List<LineRenderer> playerBeams = new List<LineRenderer>();

    //GalacticGamesManager gm;


    // Start is called before the first frame update
    void Start()
    {
        dummyObject = new GameObject("ARiftBeamDirectionsReference");
        dummyObject.layer = 9; //Ignore

        anchor = transform.root.gameObject;

        rtt = transform.GetComponentInParent<RealtimeTransform>();

        rb = GetComponentInParent<Rigidbody>();

        if (core)
        {
            core.OnPlayerReachedCore += RemovePlayerFromInfluence;
        }

        defaultBeams.Add(transform.GetChild(0).transform.GetChild(0).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(1).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(2).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(3).GetComponent<LineRenderer>());

        playerBeams.Add(transform.GetChild(1).transform.GetChild(0).GetComponent<LineRenderer>());
        playerBeams.Add(transform.GetChild(1).transform.GetChild(1).GetComponent<LineRenderer>());
        playerBeams.Add(transform.GetChild(1).transform.GetChild(2).GetComponent<LineRenderer>());

        allSensors.Add(this);

        GameObject temp = new GameObject("ARiftAnchor");
        temp.transform.position = rb.position;
        anchor = temp;

        //gm = GalacticGamesManager.Instance;
    }


    Vector3 riftPosToAnchorPos = Vector3.zero;

    private void FixedUpdate()
    {
        //Debug.Log("AttractorRift: Root position: " + transform.root.position);


        //Handle beams
        
        if (Time.time > nextTimeToChange)
        {
            nextTimeToChange += changeInterval;

            //Default beams 
            for (int i = 0; i < defaultBeams.Count; i++)
            {
                randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                pathToEdge = randomDirection.normalized * (transform.localScale.x / 15.0f);


                dummyObject.transform.rotation = Quaternion.LookRotation(pathToEdge * 10);

                offsetUp[i] = dummyObject.transform.up * (randomDirection.x / 10);
                offsetRight[i] = dummyObject.transform.right * (randomDirection.y / 10);

                defaultBeams[i].SetPosition(1, (pathToEdge / 2) +
                    offsetUp[i] + offsetRight[i]);

                endPointsInitial[i] = pathToEdge + offsetUp[i] + offsetRight[i];
                endPointsMoveTo[i] = pathToEdge - offsetUp[i] * 1.5f - offsetRight[i] * 1.5f;

                defaultBeams[i].SetPosition(2, endPointsInitial[i]);
            }

            //Beams to players
            for (int i = 0; i < playerHeads.Count; i++)
            {
                playerBeams[i].SetPosition(0, transform.position);

                directionToPlayerTorso = (playerHeads[i].transform.position - transform.position) + offsettToPlayerHead;

                playerBeams[i].SetPosition(1, transform.position + (directionToPlayerTorso / 2) +
                dummyObject.transform.up * (directionToPlayerTorso.x /5) +
                dummyObject.transform.right * (directionToPlayerTorso.y /5));

                playerBeams[i].SetPosition(2, transform.position + directionToPlayerTorso);
            }

            
        }

        else
        {
            //Move tip of beams along edges
            for (int i = 0; i < defaultBeams.Count; i++)
            {
                defaultBeams[i].SetPosition(2, Vector3.Lerp(endPointsInitial[i], endPointsMoveTo[i], (nextTimeToChange - Time.time) / changeInterval));
            }

            //Place playerBeams inside core when not used
            for (int i = 0; i < 3; i++)
            {
                playerBeams[i].SetPosition(0, transform.position);

                if (i > playerHeads.Count -1)
                {
                    playerBeams[i].SetPosition(1, transform.position);
                    playerBeams[i].SetPosition(2, transform.position);
                }
            }
        }

        if (!rtt.realtime.connected) return;


        //Place self
        if (GalacticGamesManager.Instance.CompetitionStarted && rtt.ownerIDSelf == -1) rtt.RequestOwnership();

        else if (rtt.isOwnedLocallySelf && anchor)
        {
            riftPosToAnchorPos = anchor.transform.position - transform.position;

            //if ((transform.position - anchor.transform.position).sqrMagnitude > 0.02)
            rb.AddForce(riftPosToAnchorPos * autoForce);
        }

        

        //Attract player
        if (playerControllerInReach && !playerControllerInReach.GrabbingAnything)
            playerControllerInReach.Controller.Move((transform.position - playerControllerInReach.transform.position).normalized * 0.04f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController playerController = other.GetComponent<OVRPlayerController>();

            if (playerController)
            {
                playerController.GravityModifier = 0.0f;
                playerControllerInReach = playerController;
            }

            else if (other.gameObject.name.Contains("Head"))
            {
                playerHeads.Add(other.gameObject);
            }
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController playerController = other.GetComponent<OVRPlayerController>();

            if (playerController)
            {
                RemovePlayerFromInfluence(false);
            }

            else if (other.gameObject.name.Contains("Head"))
            {
                playerHeads.Remove(other.gameObject);
            }
        }
    }
    

    void RemovePlayerFromInfluence(bool removeFromAll)
    {
        if (removeFromAll)
        {
            foreach (AttractorRift_PlayerSensor playerSensor in allSensors)
            {
                if (playerSensor.playerControllerInReach)
                {
                    playerSensor.playerControllerInReach.GravityModifier = 0.04f;
                    playerSensor.playerControllerInReach = null;
                }
            }
        }

        else
        {
            bool playerInfluencedByOtherRift = false;

            foreach (AttractorRift_PlayerSensor playerSensor in allSensors)
                if (playerSensor != this && playerSensor.playerControllerInReach) playerInfluencedByOtherRift = true;

            if (!playerInfluencedByOtherRift) playerControllerInReach.GravityModifier = 0.04f;

            playerControllerInReach = null;
        }
    }
}
