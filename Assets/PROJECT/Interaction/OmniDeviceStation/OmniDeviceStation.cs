using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmniDeviceStation : MonoBehaviour
{
    [SerializeField]
    AnimationCurve curve;

    [SerializeField]
    GameObject outerHull;

    [SerializeField]
    GameObject handSphere;

    [SerializeField]
    Vector3 hullMaxScale;

    [SerializeField]
    Vector3 hullMinScale;


    [SerializeField]
    Vector3 handSphereMaxScale;

    float sequenceDuration = 4;

    Hand hand;


    bool runSequence = false;
    float increment = 0;

    float timeAtSequenceStart = 0;

    float runningTime = 0;

    float timeAmplifier = 50;

    float yOffsett = 10f;
    float zOffsett = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11))
        {
            hand = other.GetComponent<Hand>();

            if (hand && hand.HandSide == EHandSide.RIGHT && !runSequence && !hand.OmniDeviceActive)
            {
                hand.OmniDeviceActive = true;

                runSequence = true;
                timeAtSequenceStart = Time.time;
            }
        }
    }

    private void FixedUpdate()
    {
        if (runSequence)
        {
            runningTime = Time.time - timeAtSequenceStart;

            if (runningTime < sequenceDuration)
            {
                outerHull.transform.localScale =
                    new Vector3(Mathf.Lerp(hullMaxScale.x, hullMinScale.x, Mathf.Sin(runningTime * timeAmplifier) * curve.Evaluate(runningTime)),
                                Mathf.Lerp(hullMaxScale.y, hullMinScale.y, Mathf.Sin((runningTime + yOffsett) * timeAmplifier) * curve.Evaluate(runningTime)),
                                Mathf.Lerp(hullMaxScale.z, hullMinScale.z, Mathf.Sin((runningTime + zOffsett) * timeAmplifier) * curve.Evaluate(runningTime))
                                );

                handSphere.transform.position = hand.transform.position - hand.transform.forward * 0.06f;
                handSphere.transform.localScale = Vector3.Lerp(Vector3.zero, handSphereMaxScale, curve.Evaluate(runningTime));
            }


            else
            {
                runSequence = false;
                runningTime = 0;
            }

        }
    }
}
