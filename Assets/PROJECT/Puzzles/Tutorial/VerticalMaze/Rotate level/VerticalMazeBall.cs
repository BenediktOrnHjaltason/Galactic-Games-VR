using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class VerticalMazeBall : MonoBehaviour
{
    [SerializeField]
    RealtimeTransform ballRtt;

    Realtime realtime;

    // Start is called before the first frame update
    void Start()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!realtime.connected) return;

        else if (ballRtt.ownerIDSelf == -1) ballRtt.RequestOwnership();
    }
}
