using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head"))
        {
            RealtimeView rtv = other.gameObject.GetComponent<RealtimeView>();

            if (rtv) GalacticGamesManager.Instance.RegisterFinishedPlayer(rtv.ownerIDSelf);
        }
    }
}
