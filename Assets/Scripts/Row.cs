using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    // ── Constants ──────────────────────────────────────────────────────────
    const float StepSize      = 0.25f;
    const float WrapThreshold = -6.75f;
    const float WrapReset     = 2.25f;
    const int   StepsPerSlot  = 6;        // 6 × 0.25 = 1.5 (icon spacing)

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

    // ── Inspector ──────────────────────────────────────────────────────────
    [Header("Spin Speed (seconds between steps)")]
    [SerializeField] float speedFull  = 0.025f;
    [SerializeField] float speedCrawl = 0.22f;

    [Header("Deceleration Curve")]
    [Tooltip("X = progress through coast phase (0–1), Y = interpolation toward speedCrawl (0–1).")]
    [SerializeField] AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Jitter")]
    [SerializeField] float jitterAmount = 0.004f;

    [Header("Spin Duration")]
    [Tooltip("How long the reel spins at full speed before decelerating (seconds).")]
    [SerializeField] float warmUpDuration = 3f;
    [SerializeField] int   minExtraSteps  = 60;
    [SerializeField] int   maxExtraSteps  = 100;

    [Header("Settle")]
    [SerializeField] float settleSpeed = 8f;

    // ── State ──────────────────────────────────────────────────────────────
    public bool   rowStopped;
    public string stoppedSlot;
    float cachedX;
    int   warmUpStepCount;   // set by RunForDuration, read by Rotate

    // ── Unity callbacks ────────────────────────────────────────────────────
    void Start()
    {
        cachedX    = transform.position.x;
        rowStopped = true;
        GameControl.HandlePulled += StartRotating;
    }

    void OnDestroy() => GameControl.HandlePulled -= StartRotating;

    // ── Spin logic ─────────────────────────────────────────────────────────
    void StartRotating()
    {
        stoppedSlot = "";
        StartCoroutine(Rotate());
    }

    IEnumerator Rotate()
    {
        rowStopped = false;

        // Phase 1 – full speed for warmUpDuration seconds; counts steps taken
        yield return RunForDuration(warmUpDuration);

        // Phase 2 – deceleration
        // Align so that (Phase1 steps + Phase2 steps) is a multiple of StepsPerSlot,
        // guaranteeing we land exactly on an icon no matter how Phase 1 ended.
        int total = Random.Range(minExtraSteps, maxExtraSteps);
        int combined = warmUpStepCount + total;
        total += (StepsPerSlot - combined % StepsPerSlot) % StepsPerSlot;

        yield return RunSteps(total, i =>
        {
            float t      = (float)i / total;
            float curved = decelerationCurve.Evaluate(t);
            float wait   = Mathf.Lerp(speedFull, speedCrawl, curved);
            float jitter = Random.Range(-jitterAmount, jitterAmount);
            return Mathf.Max(0.01f, wait + jitter);
        });

        // Phase 3 – settle
        yield return Settle();

        float y = Mathf.Round(transform.position.y * 4f) / 4f;
        stoppedSlot = SlotMap.TryGetValue(y, out string name) ? name : "";

        rowStopped = true;
    }

    // Full-speed spin for `duration` seconds. Records exact step count in warmUpStepCount.
    IEnumerator RunForDuration(float duration)
    {
        float elapsed   = 0f;
        warmUpStepCount = 0;

        while (elapsed < duration)
        {
            Step();
            warmUpStepCount++;

            float wait = Mathf.Max(0.01f, speedFull + Random.Range(-jitterAmount, jitterAmount));
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }
    }

    // Decelerating spin for exactly `count` steps; getWait(i) controls delay per step.
    IEnumerator RunSteps(int count, System.Func<int, float> getWait)
    {
        for (int i = 0; i < count; i++)
        {
            Step();
            yield return new WaitForSeconds(getWait(i));
        }
    }

    // Moves the reel down one step, wrapping at the bottom.
    void Step()
    {
        float y = transform.position.y;
        if (y <= WrapThreshold) y = WrapReset;
        y -= StepSize;
        transform.position = new Vector2(cachedX, y);
    }

    // Smoothly nudges the reel to the nearest slot position.
    IEnumerator Settle()
    {
        float targetY = Mathf.Round(transform.position.y * 4f) / 4f;
        float startY  = transform.position.y;
        float t       = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * settleSpeed;
            float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, t));
            transform.position = new Vector2(cachedX, y);
            yield return null;
        }

        transform.position = new Vector2(cachedX, targetY);
    }
}