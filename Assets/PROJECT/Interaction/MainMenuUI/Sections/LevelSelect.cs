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
        buttons[1].OnExecute += SelectOceanLevel;
        buttons[2].OnExecute += Select3PProtoLevel;
        buttons[3].OnExecute += OpenLevel;
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

    void SelectOceanLevel()
    {
        if (!makingNewSelection)
        {
            newIndicatorLocalPos = buttons[1].transform.localPosition + IndicatorOffsettToButton;
            selectedLevel = "OceanLevel";
            makingNewSelection = true;
        }
    }

    void Select3PProtoLevel()
    {
        if (!makingNewSelection)
        {
            newIndicatorLocalPos = buttons[2].transform.localPosition + IndicatorOffsettToButton;
            selectedLevel = "MultiplayerPrototyping";
            makingNewSelection = true;
        }
    }

    void OpenLevel()
    {
        SceneManager.LoadScene(selectedLevel);
    }
}
