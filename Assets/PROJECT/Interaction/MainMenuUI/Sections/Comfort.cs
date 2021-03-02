using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comfort : MainMenuSection
{

    // Start is called before the first frame update
    protected override void Start()
    {
        buttons[0].OnExecute += SelectBlindersOff;
        buttons[1].OnExecute += SelectBlindersOn;

        if (PlayerPrefs.GetInt("blinders") == 0)
            SelectIndicator.transform.localPosition = previousIndicatorLocalPos = buttons[0].transform.localPosition + IndicatorOffsettToButton;

        else
            SelectIndicator.transform.localPosition = previousIndicatorLocalPos = buttons[1].transform.localPosition + IndicatorOffsettToButton;

    }

    void SelectBlindersOff()
    {
        if (!makingNewSelection)
        {
            PlayerPrefs.SetInt("blinders", 0);

            newIndicatorLocalPos = buttons[0].transform.localPosition + IndicatorOffsettToButton;
            makingNewSelection = true;
        }
    }

    void SelectBlindersOn()
    {
        if (!makingNewSelection)
        {
            PlayerPrefs.SetInt("blinders", 1);

            newIndicatorLocalPos = buttons[1].transform.localPosition + IndicatorOffsettToButton;
            makingNewSelection = true;
        }
    }
}
