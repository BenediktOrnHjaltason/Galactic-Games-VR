using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OmniDeviceDispenser : MonoBehaviour
{
    [SerializeField]
    GameObject foot;

    [SerializeField]
    GameObject lowerLegPivot;

    [SerializeField]
    GameObject ringPivot;

    [SerializeField]
    GameObject graphic;

    [SerializeField]
    Animatic shrinkRing;

    [SerializeField]
    Animatic shrinkLegs;

    [SerializeField]
    Animatic shrinkFoot;

    bool animaticHasRun = false;

    // Start is called before the first frame update
    void Start()
    {
        shrinkRing.OnAnimaticEnds += shrinkLegs.startSequence;
        shrinkLegs.OnAnimaticEnds += shrinkFoot.startSequence;
    }

    private void FixedUpdate()
    {
        if (Time.time > 5.0f && !animaticHasRun)
        {
            animaticHasRun = true;
            startDisappearSequence();
        }
    }

    void startDisappearSequence()
    {
        shrinkRing.startSequence();
    }
}
