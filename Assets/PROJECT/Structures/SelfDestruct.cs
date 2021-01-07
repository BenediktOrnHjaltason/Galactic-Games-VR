using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
public class SelfDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        Realtime.Destroy(this.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
