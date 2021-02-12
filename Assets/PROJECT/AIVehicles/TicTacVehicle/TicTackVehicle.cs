using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TicTackVehicle : MonoBehaviour
{
    [System.Serializable]
    public struct MovementSequence
    {
        public Vector3 fromPosition;
        public Vector3 fromRotation;
        public Vector3 toPosition;
        public Vector3 toRotation;
        public float duration;
        public bool facePlayers;
    }


    [SerializeField]
    GameplayTrigger trigger;

    [SerializeField]
    GameObject vehicle;

    [SerializeField]
    List<MovementSequence> movementSequences;

    int activeSequence = 0;

    bool sequenceRunning;

    float timeOnSequenceStart;

    float sequenceRunningTime = 0;

    float increment = 0;

    private void Awake()
    {
        trigger.Execute += startSequence;
    }


    private void FixedUpdate()
    {
        if (sequenceRunning)
        {
            sequenceRunningTime = Time.time - timeOnSequenceStart;

            if (sequenceRunningTime < movementSequences[activeSequence].duration)
            {
                increment = sequenceRunningTime / movementSequences[activeSequence].duration;

                //Position
                vehicle.transform.localPosition = 
                    Vector3.Lerp(movementSequences[activeSequence].fromPosition, 
                                 movementSequences[activeSequence].toPosition, 
                                 increment);

                //Rotation
                vehicle.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(movementSequences[activeSequence].fromRotation),
                                                                  Quaternion.Euler(movementSequences[activeSequence].toRotation),
                                                                                   increment);
            }

            else
            {
                if ( (activeSequence + 1) < (movementSequences.Count) )
                {
                    activeSequence++;
                    timeOnSequenceStart = Time.time;
                }
                else
                {
                    //Reset Sequence

                    activeSequence = 0;
                    sequenceRunning = false;
                    vehicle.transform.localPosition = movementSequences[0].fromPosition;
                    vehicle.transform.localRotation = Quaternion.Euler(movementSequences[0].fromRotation);
                }
            }
        }
    }

    void startSequence()
    {
        if (!sequenceRunning)
        {
            sequenceRunning = true;
            timeOnSequenceStart = Time.time;
        }
    }
}
