using System.Collections;
using UnityEngine;

public class TimeSlowManager : MonoBehaviour
{
    public static TimeSlowManager Instance { get; private set; }

    [Header("UI Icons")]
    [SerializeField] private GameObject slowDownIcon;   // assign in Inspector
    [SerializeField] private GameObject fastForwardIcon; // assign in Inspector

    private Coroutine activeRoutine;
    private int token;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideIcons(); // make sure both start hidden
    }

    public static TimeSlowManager Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("TimeSlowManager");
            Instance = go.AddComponent<TimeSlowManager>();
        }
        return Instance;
    }

    /// <summary>
    /// Apply any 0..2x bullet time scale for <duration> seconds.
    /// multiplier &lt; 1 = slow, &gt; 1 = speed up. Icons reflect state.
    /// </summary>
    public void ApplyBulletScale(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp(multiplier, 0f, 2f); // cap at 200%
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ScaleRoutine(multiplier, duration));
    }

    // Back-compat with earlier calls
    public void ApplyBulletSlow(float multiplier, float duration) => ApplyBulletScale(multiplier, duration);

    private IEnumerator ScaleRoutine(float multiplier, float duration)
    {
        int my = ++token;

        // Apply scale and show the right icon
        Bullet.SetGlobalSpeedMultiplier(multiplier);
        ShowIcons(multiplier);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // unaffected by Time.timeScale
            yield return null;
        }

        // only revert if we’re the latest effect
        if (my == token)
        {
            Bullet.SetGlobalSpeedMultiplier(1f);
            HideIcons();
        }

        activeRoutine = null;
    }

    private void ShowIcons(float multiplier)
    {
        bool isSlow = multiplier < 1f;
        bool isFast = multiplier > 1f;

        if (slowDownIcon) slowDownIcon.SetActive(isSlow);
        if (fastForwardIcon) fastForwardIcon.SetActive(isFast);

        // If multiplier == 1, hide both just in case
        if (!isSlow && !isFast) HideIcons();
    }

    private void HideIcons()
    {
        if (slowDownIcon) slowDownIcon.SetActive(false);
        if (fastForwardIcon) fastForwardIcon.SetActive(false);
    }
}
