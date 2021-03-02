using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSection : MonoBehaviour
{
    [SerializeField]
    AnimationCurve selectionMoveCurve;

    [SerializeField]
    GameObject SelectIndicator;
    float increment = 0;

    protected bool makingNewSelection = false;
    protected Vector3 previousIndicatorLocalPos;
    protected Vector3 newIndicatorLocalPos;

    protected Vector3 IndicatorOffsettToButton = new Vector3(0, 0.02f, 0);

    [SerializeField]
    protected List<InteractButton> buttons;

    private void Start()
    {
        previousIndicatorLocalPos = new Vector3(2.725f, 0, 0.92f) + IndicatorOffsettToButton;
        SelectIndicator.transform.localPosition = previousIndicatorLocalPos;
    }

    private void FixedUpdate()
    {
        if (makingNewSelection)
        {
            if (increment < 1)
            {
                increment += 0.05f;
                SelectIndicator.transform.localPosition = Vector3.Lerp(previousIndicatorLocalPos, newIndicatorLocalPos, selectionMoveCurve.Evaluate(increment));
            }
            else
            {
                increment = 0;
                previousIndicatorLocalPos = newIndicatorLocalPos;
                makingNewSelection = false;
            }
        }
    }
}
