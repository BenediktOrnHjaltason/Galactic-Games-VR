using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MainMenuSection
{

    string selectedLevel = "TutorialLevelScene";

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        buttons[0].OnExecute += SelectTutorialLevel;
        buttons[1].OnExecute += SelectDummyLevel;
        buttons[2].OnExecute += OpenLevel;
    }

    void SelectTutorialLevel()
    {
        if (!makingNewSelection)
        {

            newIndicatorLocalPos = buttons[0].transform.localPosition + IndicatorOffsettToButton;
            selectedLevel = "TutorialLevelScene";
            makingNewSelection = true;
        }
    }

    void SelectDummyLevel()
    {
        if (!makingNewSelection)
        {
            newIndicatorLocalPos = buttons[1].transform.localPosition + IndicatorOffsettToButton;
            selectedLevel = "OceanLevel";
            makingNewSelection = true;
        }
    }

    void OpenLevel()
    {
        SceneManager.LoadScene(selectedLevel);
    }
}
