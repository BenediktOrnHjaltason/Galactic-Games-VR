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

    float timeAmplifier = 20;

    float yOffsett = 10f;
    float zOffsett = 20f;

    float curveValue = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(11))
        {
            hand = other.GetComponent<Hand>();

            if (hand && !runSequence && !hand.HandSync.OmniDeviceActive)
            {
                hand.HandSync.OmniDeviceActive = true;

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
                curveValue = curve.Evaluate(runningTime);

                outerHull.transform.localScale =
                    new Vector3(Mathf.Lerp(hullMaxScale.x, hullMinScale.x, Mathf.Sin(runningTime * timeAmplifier) * curveValue),
                                Mathf.Lerp(hullMaxScale.y, hullMinScale.y, Mathf.Sin((runningTime + yOffsett) * timeAmplifier) * curveValue),
                                Mathf.Lerp(hullMaxScale.z, hullMinScale.z, Mathf.Sin((runningTime + zOffsett) * timeAmplifier) * curveValue)
                                );

                handSphere.transform.position = hand.transform.position - hand.transform.forward * 0.06f;
                handSphere.transform.localScale = Vector3.Lerp(Vector3.zero, handSphereMaxScale, curve.Evaluate(runningTime));
            }


            else
            {
                runSequence = false;
                runningTime = 0;
                hand = null;
            }

        }
    }
}
