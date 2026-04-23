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
        StartCoroutine(Rotate());
    }
    IEnumerator Rotate()
    {
        rowStopped = false;
        timeInterval = 0.025f;

        for(int i = 0; i < 30; i++)
        {
            if(transform.position.y <= -6.75f)
            {
                transform.position = new Vector2(transform.position.x, 2.25f);
            }
            transform.position = new Vector2(transform.position.x, transform.position.y - 0.25f);
            yield return new WaitForSeconds(timeInterval);
        }

        randomValue = Random.Range(60, 100);

        switch (randomValue % 3)
        {
            case 1:
                randomValue += 2;
                break;
            case 2:
                randomValue += 1;
                break;
        }
        for(int i = 0; i < randomValue; i++)
        {
            if(transform.position.y <= -6.75f)
            {
                transform.position = new Vector2(transform.position.x, 2.25f);
            }
            transform.position = new Vector2(transform.position.x, transform.position.y - 0.25f);
            if(i > Mathf.RoundToInt(randomValue * 0.5f) && i <= Mathf.RoundToInt(randomValue * 0.75f))
            {
                timeInterval += 0.05f;
            }
            else if(i > Mathf.RoundToInt(randomValue * 0.75f) && i <= Mathf.RoundToInt(randomValue * 0.95f))
            {
                timeInterval += 0.15f;
            }
            else if(i > Mathf.RoundToInt(randomValue * 0.95f))
            {
                timeInterval += 0.2f;
            }
            yield return new WaitForSeconds(timeInterval); 
        }
        // Round to nearest 0.25f increment for reliable float comparison
        float roundedY = Mathf.Round(transform.position.y * 4f) / 4f;
        
        if(Mathf.Approximately(roundedY, -6.75f))
        {
            stoppedSlot = "Seven";
        }
        else if(Mathf.Approximately(roundedY, -5.25f))
        {
            stoppedSlot = "Cherry";
        }
        else if(Mathf.Approximately(roundedY, -3.75f))
        {
            stoppedSlot = "Bell";
        }
        else if(Mathf.Approximately(roundedY, -2.25f))
        {
            stoppedSlot = "Bar";
        }
        else if(Mathf.Approximately(roundedY, -0.75f))
        {
            stoppedSlot = "Bell";
        }
        else if(Mathf.Approximately(roundedY, 0.75f))
        {
            stoppedSlot = "Cherry";
        }
        else if(Mathf.Approximately(roundedY, 2.25f))
        {
            stoppedSlot = "Seven";
        }
        rowStopped = true;

    }
    void OnDestroy()
    {
        GameControl.HandlePulled -= StartRotating;        
    }
}
