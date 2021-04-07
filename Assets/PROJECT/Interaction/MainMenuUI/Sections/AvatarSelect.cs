using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSelect : MainMenuSection
{
    [SerializeField]
    InteractButton cycleForward;

    [SerializeField]
    InteractButton cycleBackwards;

    List<GameObject> heads = new List<GameObject>();

    int currentHeadIndex = 0;

    public int CurrentHeadIndex { get => currentHeadIndex; }


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        heads.Add(transform.GetChild(0).GetChild(0).gameObject);
        heads.Add(transform.GetChild(0).GetChild(1).gameObject);
        heads.Add(transform.GetChild(0).GetChild(2).gameObject);

        foreach (GameObject head in heads) head.SetActive(false);

        currentHeadIndex = PlayerPrefs.GetInt("currentHeadIndex");


        cycleForward.OnExecute += CycleForward;
        cycleBackwards.OnExecute += CycleBackwards; 
    }

    void CycleForward()
    {
        currentHeadIndex++;

        if (currentHeadIndex > heads.Count - 1) currentHeadIndex = 0;

        ShowSelection();
    }

    void CycleBackwards()
    {
        currentHeadIndex--;

        if (currentHeadIndex < 0) currentHeadIndex = heads.Count - 1;

        ShowSelection();
    }

    public void MakeCurrentHeadVisible()
    {
        heads[currentHeadIndex].SetActive(true);
    }

    void ShowSelection()
    {
        for (int i = 0; i < heads.Count; i++)
        {
            if (i == currentHeadIndex) heads[i].SetActive(true);
            else heads[i].SetActive(false);
        }
    }
}
