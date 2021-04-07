using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

enum EKeyCase
{
    UPPER,
    LOWER
}

public class MainMenuKeyboard : MonoBehaviour
{
    Dictionary<InteractButton, TextMeshPro> interactButtonToCharTMPro = new Dictionary<InteractButton, TextMeshPro>();

    InteractButton shift;
    InteractButton backspace;

    EKeyCase currentCase = EKeyCase.UPPER;

    [SerializeField]
    TextMeshPro playerName;

    // Start is called before the first frame update
    void Start()
    {
        List<InteractButton> allInteractButtons = new List<InteractButton>();
        List<TextMeshPro> allTMPro = new List<TextMeshPro>();
        
        for (int i = 2; i < 30; i++)
        {
            allInteractButtons.Add(transform.GetChild(i).GetComponent<InteractButton>());
            allTMPro.Add(transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshPro>());

            
        }

        for (int i = 0; i < allTMPro.Count; i++)
        {
            if (allTMPro[i].text == "Shift") shift = allInteractButtons[i];

            else if (allTMPro[i].text == "<-----") backspace = allInteractButtons[i];

            else interactButtonToCharTMPro.Add(allInteractButtons[i], allTMPro[i]);

            allInteractButtons[i].OnExecuteWithReference += ExecuteKey;
        }
    }

    void ExecuteKey(InteractButton button)
    {
        if (button == shift)
        {
            TriggerShift();
        }

        else if (button == backspace)
        {
            string temp = playerName.text;
            playerName.text = "";

            for (int i = 0; i < temp.Length -1; i++)
                playerName.text += temp[i];

            if (playerName.text.Length == 0 && currentCase == EKeyCase.LOWER)
                TriggerShift();
        }

        else
        {
            playerName.text += interactButtonToCharTMPro[button].text;

            if (playerName.text.Length == 1 && currentCase == EKeyCase.UPPER)
                TriggerShift();
        }
    }

    void TriggerShift()
    {
        currentCase = (currentCase == EKeyCase.UPPER) ? EKeyCase.LOWER : EKeyCase.UPPER;

        foreach (KeyValuePair<InteractButton, TextMeshPro> entry in interactButtonToCharTMPro)
        {
            if (entry.Key != shift && entry.Key != backspace)
            {
                string temp = "";

                if (currentCase == EKeyCase.UPPER)
                    temp += (char)(((int)entry.Value.text[0]) - 32);

                else if (currentCase == EKeyCase.LOWER)
                    temp += (char)(((int)entry.Value.text[0]) + 32);

                entry.Value.text = temp;
            }
        }
    }
}
