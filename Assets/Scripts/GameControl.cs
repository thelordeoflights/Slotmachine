using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
    // ── Events ─────────────────────────────────────────────────────────────

    public static event Action HandlePulled = delegate { };

    // ── Inspector ──────────────────────────────────────────────────────────

    [SerializeField] Row[]              rows;
    [SerializeField] Transform          handleIdle;
    [SerializeField] Transform          handlePulled;
    [SerializeField] TextMeshProUGUI    prizeText;
    [SerializeField] SoundManager       soundManager;

    // ── Constants ──────────────────────────────────────────────────────────

    static readonly Dictionary<string, int> Payouts = new()
    {
        { "Cherry", 100 },
        { "Bell",   200 },
        { "Seven",  300 },
        { "Bar",    400 },
    };

    // ── State ──────────────────────────────────────────────────────────────

    int  prizeValue;
    bool resultsChecked;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    void Start()
    {
        SetHandlePulled(false);
        prizeText.enabled = false;
    }

    void Update()
    {
        if (AnyRowSpinning())
        {
            resultsChecked    = false;
            prizeText.enabled = false;
            prizeValue        = 0;
        }
        else if (!resultsChecked)
        {
            OnAllRowsStopped();
        }
    }

    void OnMouseDown()
    {
        if (!AnyRowSpinning())
            StartCoroutine(PullHandle());
    }

    // ── Handle animation ───────────────────────────────────────────────────

    IEnumerator PullHandle()
    {
        SetHandlePulled(true);
        HandlePulled();                         // fires the event — starts all rows spinning

        yield return new WaitForSeconds(0.1f);

        SetHandlePulled(false);
    }

    void SetHandlePulled(bool pulled)
    {
        handleIdle.gameObject.SetActive(!pulled);
        handlePulled.gameObject.SetActive(pulled);
    }

    // ── Result evaluation ──────────────────────────────────────────────────

    void OnAllRowsStopped()
    {
        soundManager.StopSpinSound();
        EvaluateResult();
        prizeText.enabled = true;
        resultsChecked    = true;
    }

    void EvaluateResult()
    {
        string slot = rows[0].stoppedSlot;
        bool   isMatch = slot == rows[1].stoppedSlot && slot == rows[2].stoppedSlot;

        if (isMatch && Payouts.TryGetValue(slot, out int payout))
        {
            prizeValue     = payout;
            prizeText.text = $"You win ${prizeValue}!";
            soundManager.PlayWinSound();
        }
        else
        {
            prizeValue     = 0;
            prizeText.text = "Pull the handle to play!";
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    bool AnyRowSpinning() => !rows[0].rowStopped || !rows[1].rowStopped || !rows[2].rowStopped;
}