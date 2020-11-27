using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIWrist : MonoBehaviour
{

    TextMeshPro UITime;

    int nextSecond = 1;

    [SerializeField]
    GameObject leftHandAnchor;

    [SerializeField]
    GameObject eyeAnchor;

    bool playerWatching = false;

    Vector3 scale_Active = new Vector3(0.1579223f, 0.4397724f, 0.07973082f);

    float activationScalar = 0;

    // Start is called before the first frame update
    void Start()
    {
        UITime = GetComponentInChildren<TextMeshPro>();
        transform.localScale = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > nextSecond)
        {
            nextSecond++;
            UITime.text = getTimeText();
        }

        // Dot (-Hand.Right, Eye.Forward),
        // Dot (Hand.Forward, -Eye.Right



        if (Vector3.Dot(-leftHandAnchor.transform.right, eyeAnchor.transform.forward) < -0.8 &&
            Vector3.Dot(leftHandAnchor.transform.forward, -eyeAnchor.transform.right) < -0.8) playerWatching = true;

        else playerWatching = false;


        if (playerWatching && activationScalar < 1) activationScalar += 0.1f;

        else if (!playerWatching && activationScalar > 0) activationScalar -= 0.1f;

        transform.localScale = scale_Active * activationScalar;
            
            //Debug.Log("Dot(-Hand.Right, Eye.Forward) = " + Vector3.Dot(-leftHandAnchor.transform.right, eyeAnchor.transform.forward));
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
}
