using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class Structure_Platform : MonoBehaviour
{

    [SerializeField]
    bool moving = false;

    [SerializeField]
    float respawnPoint;

    [SerializeField]
    float endPoint;

    [SerializeField]
    PlatformMoveGlobal globalMoveDirection;

    float moveIncrement = 1.0f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!moving) return;

        switch(globalMoveDirection)
        {
            case PlatformMoveGlobal.Z_Negative:

                if (transform.position.z > endPoint) transform.position -= new Vector3(0, 0, moveIncrement * Time.deltaTime);
                else transform.position = new Vector3(transform.position.x, transform.position.y, respawnPoint);

                break;
        }

    }
}
