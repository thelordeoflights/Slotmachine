using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    // ── Constants ──────────────────────────────────────────────────────────
    const float StepSize      = 0.25f;
    const float WrapThreshold = -6.75f;
    const float WrapReset     = 2.25f;

    static readonly Dictionary<float, string> SlotMap = new()
    {
        { -6.75f, "Seven"  },
        { -5.25f, "Cherry" },
        { -3.75f, "Bell"   },
        { -2.25f, "Bar"    },
        { -0.75f, "Bell"   },
        {  0.75f, "Cherry" },
        {  2.25f, "Seven"  },
    };

    // ── Per-row custom timing (set in Inspector) ───────────────────────────
    [Header("Spin Speed (seconds between steps)")]
    [SerializeField] float speedFull  = 0.025f;
    [SerializeField] float speedSlow1 = 0.06f;
    [SerializeField] float speedSlow2 = 0.12f;
    [SerializeField] float speedCrawl = 0.22f;

    [Header("Spin Duration")]
    [SerializeField] int warmUpSteps  = 30;
    [SerializeField] int minExtraSteps = 60;
    [SerializeField] int maxExtraSteps = 100;

    // ── Cached WaitForSeconds (rebuilt when values change) ─────────────────
    WaitForSeconds waitFull;
    WaitForSeconds waitSlow1;
    WaitForSeconds waitSlow2;
    WaitForSeconds waitCrawl;

    // ── State ──────────────────────────────────────────────────────────────
    public bool   rowStopped;
    public string stoppedSlot;
    float cachedX;

    // ── Unity callbacks ────────────────────────────────────────────────────
    void Start()
    {
        cachedX    = transform.position.x;
        rowStopped = true;
        RebuildWaits();
        GameControl.HandlePulled += StartRotating;
    }

    void OnDestroy() => GameControl.HandlePulled -= StartRotating;

    // Call this if you change speed values at runtime
    void RebuildWaits()
    {
        waitFull  = new WaitForSeconds(speedFull);
        waitSlow1 = new WaitForSeconds(speedSlow1);
        waitSlow2 = new WaitForSeconds(speedSlow2);
        waitCrawl = new WaitForSeconds(speedCrawl);
    }

    // ── Public API – change speed at runtime and restart ──────────────────
    public void SetSpeeds(float full, float slow1, float slow2, float crawl)
    {
        speedFull  = full;
        speedSlow1 = slow1;
        speedSlow2 = slow2;
        speedCrawl = crawl;
        RebuildWaits();
    }

    // ── Spin logic ─────────────────────────────────────────────────────────
    void StartRotating()
    {
        stoppedSlot = "";
        StartCoroutine(Rotate());
    }

    IEnumerator Rotate()
    {
        rowStopped = false;

        // Phase 1 – warm-up
        yield return RunSteps(warmUpSteps, _ => waitFull);

        // Phase 2 – coast + decelerate
        int total = Random.Range(minExtraSteps, maxExtraSteps);
        total += (6 - total % 6) % 6;

        int p1 = Mathf.RoundToInt(total * 0.50f);
        int p2 = Mathf.RoundToInt(total * 0.75f);
        int p3 = Mathf.RoundToInt(total * 0.95f);

        yield return RunSteps(total, i =>
        {
            if      (i <= p1) return waitFull;
            else if (i <= p2) return waitSlow1;
            else if (i <= p3) return waitSlow2;
            else              return waitCrawl;
        });

        float y = Mathf.Round(transform.position.y * 4f) / 4f;
        stoppedSlot = SlotMap.TryGetValue(y, out string name) ? name : "";

        rowStopped = true;
    }

    IEnumerator RunSteps(int count, System.Func<int, WaitForSeconds> getWait)
    {
        for (int i = 0; i < count; i++)
        {
            float y = transform.position.y;
            if (y <= WrapThreshold) y = WrapReset;
            y -= StepSize;
            transform.position = new Vector2(cachedX, y);
            yield return getWait(i);
        }
    }
}