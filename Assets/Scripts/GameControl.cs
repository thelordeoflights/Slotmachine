using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };
    [SerializeField] Row[] rows;
    [SerializeField] Transform handle, handlePulled;
    [SerializeField] TextMeshProUGUI PrizeText;
    int prizeValue;
    bool resultsChecked = false;
    void Start()
    {
        handle.gameObject.SetActive(true);
        handlePulled.gameObject.SetActive(false);
        PrizeText.enabled = false;
    }
    void Update()
    {
        if(!rows[0].rowStopped || !rows[1].rowStopped || !rows[2].rowStopped)
        {
            resultsChecked = false;
            PrizeText.enabled = false;
            prizeValue = 0;
        }
        if(rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked)
        {
            CheckResults();
            PrizeText.enabled = true;
            PrizeText.text = "You win $" + prizeValue.ToString() + "!";
        }
    }

    void OnMouseDown()
    {
        if(rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped)
        {
            StartCoroutine(PullHandle());
        }
    }

    IEnumerator PullHandle()
    {
        Debug.Log("Handle pulled!");

        handle.gameObject.SetActive(false);
        handlePulled.gameObject.SetActive(true);
        // for(int i = 0; i < 10; i += 5)
        // {
        //     handle.Rotate(0, 0, i);
        //     yield return new WaitForSeconds(0.1f);
        // }
        HandlePulled();
        yield return new WaitForSeconds(0.1f);
        handlePulled.gameObject.SetActive(false);
        handle.gameObject.SetActive(true);
        
        // for(int i = 0; i < 15; i += 5)
        // {
        //     handle.Rotate(0, 0, -i);
        //     yield return new WaitForSeconds(0.1f);
        // }
    }
    void CheckResults()
    {
        if(rows[0].stoppedSlot == rows[1].stoppedSlot && rows[1].stoppedSlot == rows[2].stoppedSlot)
        {
            switch(rows[0].stoppedSlot)
            {
                case "Cherry":
                    prizeValue = 100;
                    break;
                case "Bell":
                    prizeValue = 200;
                    break;
                case "Seven":
                    prizeValue = 300;
                    break;
                case "Bar":
                    prizeValue = 400;
                    break;
            }
            
        }
        else
        {
            PrizeText.text = "You lose. Try again!";
        }
     
    }
}
