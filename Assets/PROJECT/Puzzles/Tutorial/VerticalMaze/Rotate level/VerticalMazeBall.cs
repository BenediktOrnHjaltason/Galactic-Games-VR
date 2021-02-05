using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class VerticalMazeBall : MonoBehaviour
{
    [SerializeField]
    RealtimeTransform ballRtt;

    RealtimeTransform thisRtt;

    Realtime realtime;

    // Start is called before the first frame update
    void Start()
    {
        thisRtt = GetComponent<RealtimeTransform>();

        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!realtime.connected) return;

        else if (thisRtt.ownerIDSelf == -1) ballRtt.SetOwnership(0);

        else ballRtt.SetOwnership(thisRtt.ownerIDSelf);
    }
}
