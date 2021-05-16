using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    float rotateIncrement = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        rotateIncrement += 19.4f;

        if (rotateIncrement > 1940) rotateIncrement = 0;

        transform.localRotation = Quaternion.Euler(new Vector3(rotateIncrement, 0, 0));
    }
}
