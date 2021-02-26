using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSolarSystem : MonoBehaviour
{

    [SerializeField]
    GameObject star;


    //----Planet1

    [SerializeField]
    GameObject planet1Pivot;

    [SerializeField]
    GameObject planet1;

    [SerializeField]
    float speed_Planet1;
    float increment_Planet1 = 0;


    //----Planet2
    [SerializeField]
    GameObject planet2Pivot;

    [SerializeField]
    GameObject planet2;

    [SerializeField]
    float speed_Planet2;
    float increment_Planet2 = 0;

    [SerializeField]
    GameObject planet2Moon1;

    [SerializeField]
    float speed_Planet2Moon1;
    float increment_Planet2Moon1 = 0;

    //----Planet3

    [SerializeField]
    GameObject planet3Pivot;

    [SerializeField]
    GameObject planet3;

    [SerializeField]
    float speed_Planet3;
    float increment_Planet3 = 0;

    //----Planet4

    [SerializeField]
    GameObject planet4Pivot;

    [SerializeField]
    GameObject planet4;

    [SerializeField]
    float speed_Planet4;
    float increment_Planet4 = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        increment_Planet1 += speed_Planet1 * Time.deltaTime;
        planet1Pivot.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet1, 0));
        planet1.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet1 * 3, 0));


        increment_Planet2 += speed_Planet2 * Time.deltaTime;
        planet2Pivot.transform.rotation =  Quaternion.Euler(new Vector3(0, increment_Planet2, 0));
        planet2.transform.rotation = Quaternion.Euler(new Vector3(0, -increment_Planet2 *3, 0));

        increment_Planet2Moon1 += speed_Planet2Moon1 * Time.deltaTime;
        planet2Moon1.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet2Moon1, 0));


        increment_Planet3 += speed_Planet3 * Time.deltaTime;
        planet3Pivot.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet3, 0));
        planet3.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet3 * 4, 0));

        increment_Planet4 += speed_Planet4 * Time.deltaTime;
        planet4Pivot.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet4, 0));
        planet4.transform.rotation = Quaternion.Euler(new Vector3(0, -increment_Planet4, 0));





        star.transform.rotation = Quaternion.Euler(new Vector3(0, increment_Planet1, 0));


    }
}
