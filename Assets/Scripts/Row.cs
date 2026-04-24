using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    // ── Configuration ──────────────────────────────────────────────────────

    [Header("Speed")]
    [SerializeField] float fullSpeed  = 0.025f;   // seconds between steps at max speed
    [SerializeField] float crawlSpeed = 0.22f;    // seconds between steps when nearly stopped

    [Header("Deceleration")]
    [Tooltip("Y = interpolation toward crawlSpeed over the coast phase (X = 0–1).")]
    [SerializeField] AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float jitter = 0.004f;        // random ± added to each step's wait time

    [Header("Spin Duration")]
    [Tooltip("Seconds the reel runs at full speed before decelerating.")]
    [SerializeField] float fullSpeedDuration = 3f;
    [SerializeField] int   minCoastSteps     = 60;
    [SerializeField] int   maxCoastSteps     = 100;

    [Header("Settle")]
    [SerializeField] float settleSpeed = 8f;       // higher = snaps to slot faster

    // ── Public state ───────────────────────────────────────────────────────

    public bool   rowStopped;
    public string stoppedSlot;

    // ── Private constants ──────────────────────────────────────────────────

    const float StepSize      = 0.25f;   // world-units moved per step
    const float WrapThreshold = -6.75f;  // Y position that triggers a wrap
    const float WrapReset     = 2.25f;   // Y position snapped to after wrapping
    const int   StepsPerSlot  = 6;       // steps between icon centres (6 × 0.25 = 1.5 units)

    // Maps a stopped Y position to the icon name displayed at that slot.
    static readonly Dictionary<float, string> SlotNames = new()
    {
        { -6.75f, "Seven"  },
        { -5.25f, "Cherry" },
        { -3.75f, "Bell"   },
        { -2.25f, "Bar"    },
        { -0.75f, "Bell"   },
        {  0.75f, "Cherry" },
        {  2.25f, "Seven"  },
    };

    // ── Private state ──────────────────────────────────────────────────────

    float cachedX;           // X never changes; cached to avoid repeated position reads
    int   fullSpeedSteps;    // steps taken during the full-speed phase (used to align coast phase)

    // ── Unity lifecycle ────────────────────────────────────────────────────

    void Start()
    {
        cachedX    = transform.position.x;
        rowStopped = true;
        GameControl.HandlePulled += StartRotating;
    }

    void OnDestroy() => GameControl.HandlePulled -= StartRotating;

    // ── Public entry point ─────────────────────────────────────────────────

    void StartRotating()
    {
        stoppedSlot = "";
        StartCoroutine(SpinSequence());
    }

    // ── Spin sequence ──────────────────────────────────────────────────────

    IEnumerator SpinSequence()
    {
        rowStopped = false;

        yield return Phase_FullSpeed();
        yield return Phase_Decelerate();
        yield return Phase_Settle();

        stoppedSlot = ReadCurrentSlot();
        rowStopped  = true;
    }

    // Phase 1: spin at full speed for a fixed duration.
    IEnumerator Phase_FullSpeed()
    {
        float elapsed = 0f;
        fullSpeedSteps = 0;

        while (elapsed < fullSpeedDuration)
        {
            MoveOneStep();
            fullSpeedSteps++;

            float wait = RandomisedWait(fullSpeed);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }
    }

    // Phase 2: decelerate over a variable number of steps, then stop on an icon.
    IEnumerator Phase_Decelerate()
    {
        // Choose a random coast length, then round it up so the total step count
        // (full-speed + coast) is a multiple of StepsPerSlot. This guarantees the
        // reel always halts exactly on an icon centre.
        int coastSteps   = Random.Range(minCoastSteps, maxCoastSteps);
        int totalSteps   = fullSpeedSteps + coastSteps;
        int remainder    = totalSteps % StepsPerSlot;
        if (remainder != 0) coastSteps += StepsPerSlot - remainder;

        yield return RunSteps(coastSteps, stepIndex =>
        {
            float progress = (float)stepIndex / coastSteps;
            float blend    = decelerationCurve.Evaluate(progress);
            return RandomisedWait(Mathf.Lerp(fullSpeed, crawlSpeed, blend));
        });
    }

    // Phase 3: smooth snap to the nearest slot centre.
    IEnumerator Phase_Settle()
    {
        float startY  = transform.position.y;
        float targetY = Mathf.Round(startY * 4f) / 4f;
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

    // ── Helpers ────────────────────────────────────────────────────────────

    // Advances the reel by one step, wrapping back to the top when needed.
    void MoveOneStep()
    {
        float y = transform.position.y;
        if (y <= WrapThreshold) y = WrapReset;
        y -= StepSize;
        transform.position = new Vector2(cachedX, y);
    }

    // Runs exactly `count` steps; `getWait` receives the current step index and
    // returns the delay (in seconds) to apply after that step.
    IEnumerator RunSteps(int count, System.Func<int, float> getWait)
    {
        for (int i = 0; i < count; i++)
        {
            MoveOneStep();
            yield return new WaitForSeconds(getWait(i));
        }
    }

    // Returns the base wait time plus a small random offset, clamped to a safe minimum.
    float RandomisedWait(float baseWait) =>
        Mathf.Max(0.01f, baseWait + Random.Range(-jitter, jitter));

    // Looks up the icon name for the reel's current Y position.
    string ReadCurrentSlot()
    {
        float y = Mathf.Round(transform.position.y * 4f) / 4f;
        return SlotNames.TryGetValue(y, out string name) ? name : "";
    }
}