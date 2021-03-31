using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class ClimbingWallCylinder : MonoBehaviour
{

    [SerializeField]
    ESpinDirection spinDirection = ESpinDirection.CLOCKWISE;

    [SerializeField]
    float spinSpeed = 1;

    float spinMultiplier = 0;


    // Start is called before the first frame update
    void Start()
    {
        spinMultiplier = (spinDirection == ESpinDirection.CLOCKWISE) ? 0.03f * spinSpeed : -0.03f * spinSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, spinMultiplier, 0));
    }
}
