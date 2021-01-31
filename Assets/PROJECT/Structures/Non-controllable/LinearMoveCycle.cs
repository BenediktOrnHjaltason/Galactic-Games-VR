using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class LinearMoveCycle : MonoBehaviour
{

    /// <summary>
    /// Moves an object between start and end-points in an endless cycle.
    /// Brings it back to startpoint when end is reached.
    /// NOTE! Needs implementation for directions other than negative Z
    /// </summary>

    [SerializeField]
    bool moving = false;

    [SerializeField]
    float respawnPoint;

    [SerializeField]
    float endPoint;

    [SerializeField]
    EPlatformMoveGlobal globalMoveDirection;

    [SerializeField]
    float speed = 1.0f;


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
            case EPlatformMoveGlobal.Z_Negative:

                if (transform.position.z > endPoint) transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
                else transform.position = new Vector3(transform.position.x, transform.position.y, respawnPoint);

                break;
        }

    }
}
