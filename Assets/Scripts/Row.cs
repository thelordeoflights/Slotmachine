using System.Collections;
using UnityEngine;

public class Row : MonoBehaviour
{
    int randomValue;
    float timeInterval;

    public bool rowStopped;
    public string stoppedSlot;

    void Start()
    {
        rowStopped = true;
        GameControl.HandlePulled += StartRotating;
    }
    void StartRotating()
    {
        stoppedSlot = "";
        //StartCoroutine("Rotate");
    }
    // IEnumerator Rotate()
    // {
    //     rowStopped = false;
    //     timeInterval = 0.025f;

    //     for(int i = 0; i < 30; i++)
    //     {
            
    //     }

    // }
}
