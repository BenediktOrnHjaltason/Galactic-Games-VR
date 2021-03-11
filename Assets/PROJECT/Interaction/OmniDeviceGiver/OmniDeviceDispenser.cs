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
    Animatic screwDownLegs;

    [SerializeField]
    Animatic shrinkFoot;

    bool runDisappearAnimatic = false;

    //-------------

    [SerializeField]
    OmniDeviceDispenser_Trigger trigger;

    [SerializeField]
    GameObject handSphere;

    Transform handTransform;

    bool runDispenseSequence = false;

    float timeAtSequenceStart = 0;
    float runningTime = 0;
    float sequenceDuration = 4;
    float timeAmplifier = 20;

    float yOffsett = 10f;
    float zOffsett = 20f;

    float curveValue = 0;

    [SerializeField]
    AnimationCurve dispenseSequenceCurve;

    float planeMaxScale = 0.002093048f;
    float planeMinScale = 0.001132486f;

    Vector3 handSphereMaxScale = new Vector3(0.1804088f, 0.1804088f, 0.1804088f);



    // Start is called before the first frame update
    void Start()
    {
        shrinkRing.OnAnimaticEnds += screwDownLegs.startSequence;
        screwDownLegs.OnAnimaticEnds += shrinkFoot.startSequence;

        if (trigger) trigger.OnHandEnters += OnHandEntersTrigger;
    }

    void startDisappearSequence()
    {
        shrinkRing.startSequence();
    }

    void OnHandEntersTrigger(Hand hand)
    {
        handTransform = hand.transform;

        if (hand && !runDispenseSequence && !hand.HandSync.OmniDeviceActive)
        {
            if (hand.OtherHand.HandSync.OmniDeviceActive) hand.OtherHand.SetOmniDeviceActive(false);

            hand.SetOmniDeviceActive(true);

            runDispenseSequence = true;
            timeAtSequenceStart = Time.time;

            handSphere.transform.SetParent(handTransform);
            handSphere.transform.localPosition = new Vector3(0, 0, -0.1f);
        }
    }

    private void FixedUpdate()
    {
        if (runDispenseSequence)
        {
            runningTime = Time.time - timeAtSequenceStart;

            if (runningTime < sequenceDuration)
            {
                curveValue = dispenseSequenceCurve.Evaluate(runningTime);

                graphic.transform.localScale =
                    new Vector3(Mathf.Lerp(planeMaxScale, planeMinScale, Mathf.Sin(runningTime * timeAmplifier) * curveValue),
                                planeMaxScale,
                                Mathf.Lerp(planeMaxScale, planeMinScale, Mathf.Sin((runningTime + yOffsett) * timeAmplifier) * curveValue));

                //handSphere.transform.position = handTransform.position - handTransform.forward * 0.06f;
                handSphere.transform.localScale = Vector3.Lerp(Vector3.zero, handSphereMaxScale, curveValue);
            }

            else
            {
                runDispenseSequence = false;
                runningTime = 0;
                handTransform = null;

                handSphere.transform.localScale = Vector3.zero;
                handSphere.transform.SetParent(this.transform);

                //startDisappearSequence();
            }
        }
    }
}
