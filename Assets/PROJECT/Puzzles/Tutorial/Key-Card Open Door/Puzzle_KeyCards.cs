using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_KeyCards : MonoBehaviour
{
    [SerializeField]
    Door door;

    static int insertedKeys = 0;

    static bool previousActionOpenedDoor = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
            insertedKeys++;

        VerifyCondition();
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.layer.Equals(10))
            insertedKeys--;

        VerifyCondition();
    }

    void VerifyCondition()
    {
        if (insertedKeys == 2 && !previousActionOpenedDoor)
        {
            door.Operate();
            previousActionOpenedDoor = true;
        }
        else if ((insertedKeys == 0 && previousActionOpenedDoor) || (insertedKeys == 1 && previousActionOpenedDoor))
        {
            door.Operate();
            previousActionOpenedDoor = false;
        }
    }
}
