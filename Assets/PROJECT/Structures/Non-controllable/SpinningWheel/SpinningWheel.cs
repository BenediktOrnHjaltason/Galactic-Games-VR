using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class SpinningWheel : MonoBehaviour
{

    [SerializeField]
    ESpinDirection spinDirection;

    [SerializeField]
    float spinAmount = 15.0f;

    float spinMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        spinMultiplier = (spinDirection == ESpinDirection.CLOCKWISE) ? 1 : -1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 
                                                          transform.rotation.eulerAngles.y, 
                                                          transform.rotation.eulerAngles.z + spinAmount * spinMultiplier * Time.deltaTime));
    }
}
