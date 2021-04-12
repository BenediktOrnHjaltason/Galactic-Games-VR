using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Animatic : MonoBehaviour
{

    [System.Serializable]
    public class MovementSequence
    {
        public bool animatePosition;
        [HideInInspector]
        public Vector3 fromPosition;
        public Vector3 toPosition;

        public bool animateRotation;
        [HideInInspector]
        public Vector3 fromRotation;
        public Vector3 toRotation;

        public bool animateScale;
        [HideInInspector]
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

    
    public List<MovementSequence> movementSequences;

    int activeSequence = 0;

    bool sequenceRunning = false;

    float timeOnSequenceStart = 0;

    float sequenceRunningTime = 0;

    float increment = 0;

    //Events and delegates
    public event Action OnAnimaticEnds;

    public delegate void OnSequenceStart();

    public OnSequenceStart[] onSequenceStart = null;


    // Start is called before the first frame update
    void Start()
    {
        if (trigger) trigger.Execute += Run;

        InitializeTransforms();
    }

    //Sets start values for sequences based on end values from previous sequences
    //(To avoid the tediousness of copying start positions manually)
    void InitializeTransforms()
    {
        if (movementSequences != null && movementSequences.Count > 0)
        {
            movementSequences[0].fromPosition = target.transform.localPosition;
            movementSequences[0].fromRotation = target.transform.localRotation.eulerAngles;
            movementSequences[0].fromScale = target.transform.localScale;


            //Ensures correct start values for rest of sequences. If previous sequence not animated (to-value 0), 
            //sample from-value instead of to-value. Correct values trickle down. 
            for (int i = 1; i < movementSequences.Count -1; i++)
            {
                movementSequences[i].fromPosition =
                    (movementSequences[i - 1].animatePosition) ? movementSequences[i - 1].toPosition
                                                               : movementSequences[i - 1].fromPosition;

                movementSequences[i].fromRotation =
                    (movementSequences[i - 1].animateRotation) ? movementSequences[i - 1].toRotation
                                                               : movementSequences[i - 1].fromRotation;

                movementSequences[i].fromScale =
                    (movementSequences[i - 1].animateScale) ? movementSequences[i - 1].toScale
                                                            : movementSequences[i - 1].fromScale;
            }
        }
    }


    public void InitializeSequenceDelegates()
    {
        onSequenceStart = new OnSequenceStart[movementSequences.Count];
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

                    if (onSequenceStart != null && onSequenceStart[activeSequence] != null)
                        onSequenceStart[activeSequence]();

                    timeOnSequenceStart = Time.time;
                }
                else
                {
                    //Ensure everything gets to the end
                    increment = 1;
                    SetTransforms();

                    activeSequence = 0;
                    sequenceRunning = false;

                    OnAnimaticEnds?.Invoke();
                }
            }
        }
    }

    public void Run()
    {
        if (!sequenceRunning)
        {
            
            sequenceRunning = true;

            if (onSequenceStart != null && onSequenceStart[activeSequence] != null)
                onSequenceStart[activeSequence]();

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
