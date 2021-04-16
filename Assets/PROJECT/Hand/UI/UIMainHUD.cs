﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Normal.Realtime;

public class UIMainHUD : MonoBehaviour
{

    //TextMeshPro UITime;

    int nextSecond = 1;

    [SerializeField]
    GameObject eyeAnchor;

    [SerializeField]
    InteractButton mainMenuButton;

    List<BoxCollider> allButtonsColliders = new List<BoxCollider>();

    [SerializeField]
    MeshRenderer UIBackground;

    

    [SerializeField]
    Vector3 fullScale;

    bool playerWatching = false;
    float scaleMultiplier = 0;

    // Start is called before the first frame update
    void Start()
    {
        //UITime = GetComponentInChildren<TextMeshPro>();
        transform.localScale = Vector3.zero;

        if (mainMenuButton)
        {
            allButtonsColliders.Add(mainMenuButton.GetComponent<BoxCollider>());

            mainMenuButton.OnExecute += BackToMainMenu;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        if (Time.timeSinceLevelLoad > nextSecond)
        {
            nextSecond++;
            UITime.text = getTimeText();
        }
        */

        playerWatching = (Vector3.Dot(-transform.right, eyeAnchor.transform.forward) < -0.96 &&
                          Vector3.Dot(transform.forward, -eyeAnchor.transform.right) < -0.96);


        if (playerWatching && scaleMultiplier < 1)
        {
            if (scaleMultiplier < 0.1 && allButtonsColliders[0].enabled == false)
                SetMenuActive(true);

            scaleMultiplier += 0.1f;
        }

        else if (!playerWatching && scaleMultiplier > 0)
        {
            scaleMultiplier -= 0.1f;

            if (scaleMultiplier < 0.1f && allButtonsColliders[0].enabled == true)
            {
                SetMenuActive(false);
            }
        }

        transform.localScale = fullScale * scaleMultiplier;
    }

    string getTimeText()
    {
        float time = Time.timeSinceLevelLoad;

        int iMinutes = ((int)(time / 60));
        int iSeconds = ((int)(time % 60));

        string minutes = (iMinutes > 9) ? iMinutes.ToString() : "0" + iMinutes.ToString();
        string seconds = (iSeconds > 9) ? iSeconds.ToString() : "0" + iSeconds.ToString();

        return minutes + ":" + seconds;
    }

    void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void SetMenuActive(bool active)
    {
        UIBackground.enabled = active;

        foreach(BoxCollider button in allButtonsColliders)
        {
            button.enabled = active;
            button.gameObject.SetActive(active);
        }
            
    }
}
