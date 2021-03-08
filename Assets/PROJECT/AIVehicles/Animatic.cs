using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Animatic : MonoBehaviour
{

    [System.Serializable]
    public struct MovementSequence
    {
        public bool animatePosition;
        public Vector3 fromPosition;
        public Vector3 toPosition;
        public bool animateRotation;
        public Vector3 fromRotation;
        public Vector3 toRotation;
        public bool animateScale;
        public Vector3 fromScale;
        public Vector3 toScale;
        public AnimationCurve curve;
        public float duration;
        public bool pauseSequence;
    }

    [SerializeField]
    string title = "Default";

    [SerializeField]
    GameplayTrigger trigger;

    [SerializeField]
    GameObject target;

    [SerializeField]
    List<MovementSequence> movementSequences;

    int activeSequence = 0;

    bool sequenceRunning = false;

    float timeOnSequenceStart = 0;

    float sequenceRunningTime = 0;

    float increment = 0;

    public event Action OnAnimaticEnds;

    // Start is called before the first frame update
    void Start()
    {
        if (trigger) trigger.Execute += startSequence;
    }

    private void FixedUpdate()
    {
        if (sequenceRunning)
        {
            sequenceRunningTime = Time.time - timeOnSequenceStart;

            if (sequenceRunningTime < movementSequences[activeSequence].duration)
            {
                increment = sequenceRunningTime / movementSequences[activeSequence].duration;

                SetTransforms();
            }

            else
            {
                if ((activeSequence + 1) < (movementSequences.Count))
                {
                    activeSequence++;
                    timeOnSequenceStart = Time.time;
                }
                else
                {

                    Debug.Log("Animatic: local scale on animatic end: " + target.transform.localScale + " . Increment:  " + increment);

                    //Ensure everything gets to the end
                    increment = 1;
                    SetTransforms();

                    activeSequence = 0;
                    sequenceRunning = false;
                    //target.transform.localPosition = movementSequences[0].fromPosition;
                    //target.transform.localRotation = Quaternion.Euler(movementSequences[0].fromRotation);

                    OnAnimaticEnds?.Invoke();
                }
            }
        }
    }

    public void startSequence()
    {
        if (!sequenceRunning)
        {
            sequenceRunning = true;
            timeOnSequenceStart = Time.time;
        }
    }

    void SetTransforms()
    {
        //Position
        if (movementSequences[activeSequence].animatePosition)
            target.transform.localPosition =
                Vector3.Lerp(movementSequences[activeSequence].fromPosition,
                             movementSequences[activeSequence].toPosition,
                             movementSequences[activeSequence].curve.Evaluate(increment));

        //Rotation
        if (movementSequences[activeSequence].animateRotation)
            target.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(movementSequences[activeSequence].fromRotation),
                                                          Quaternion.Euler(movementSequences[activeSequence].toRotation),
                                                                           movementSequences[activeSequence].curve.Evaluate(increment));

        //Scale
        if (movementSequences[activeSequence].animateScale)
            target.transform.localScale = Vector3.Lerp(movementSequences[activeSequence].fromScale,
                                                       movementSequences[activeSequence].toScale,
                                                       movementSequences[activeSequence].curve.Evaluate(increment));
    }
}
