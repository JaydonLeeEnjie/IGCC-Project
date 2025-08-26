using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<EnemyAttackSequence> attackPattern = new();
    [SerializeField] private List<GameObject> bulletPrefabs = new();
    [SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public bool isDead;
    [SerializeField] private List<Animator> animators = new();
    [SerializeField] private BattleManager battleManager;


    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 10f;

    [Header("UI")]
    [SerializeField] private GameObject EnemyUI;
    [SerializeField] private Image HealthBar;
    [SerializeField] private TextMeshProUGUI DamageText;

    [Header("UI Drain Settings")]
    [SerializeField] private float drainSpeed = 1.2f;
    [SerializeField] private float uiHoldAfterDrain = 0.5f;

    private Coroutine uiRoutine;
    private int uiSessionId; // used to know if a newer drain started
    private int sequenceCursor = 0;

    private void Start()
    {
        // Initialize UI/health bar
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 0, maxHealth);
        if (HealthBar) HealthBar.fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        if (EnemyUI) EnemyUI.SetActive(false);
        //for (int i = 0; i < animators.Count; i++)
        //{
        //    animators[i].
        //}
    }

    // === Public API you can call from BattleManager ===
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        // Start (or restart) the drain UI from the CURRENT displayed fill
        StartHealthDrainUI(amount);
    }

    private void StartHealthDrainUI(float damageAmount)
    {
        if (!HealthBar || !EnemyUI)
            return;

        if (uiRoutine != null)
            StopCoroutine(uiRoutine);

        uiRoutine = StartCoroutine(DrainHealthbarRoutine(damageAmount));
    }

    private IEnumerator DrainHealthbarRoutine(float damageAmount)
    {
        uiSessionId++;
        int myId = uiSessionId;

        // Show UI and update damage text
        EnemyUI.SetActive(true);
        if (DamageText) DamageText.text = $"-{Mathf.RoundToInt(damageAmount)}";

        // Drain from what's currently displayed to the new health
        float fromFill = HealthBar.fillAmount;
        float toFill = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;

        // Compute duration based on how much fill we need to drain and the speed
        float delta = Mathf.Abs(fromFill - toFill);
        float duration = Mathf.Max(0.05f, delta / Mathf.Max(0.0001f, drainSpeed));

        float t = 0f;
        while (t < duration)
        {
            float p = t / duration;
            HealthBar.fillAmount = Mathf.Lerp(fromFill, toFill, p);
            t += Time.unscaledDeltaTime; // UI usually ignores timescale
            yield return null;
        }
        HealthBar.fillAmount = toFill;
        if (currentHealth <= 0f)
        {
            battleManager.FreezeBattle();
        }


        // Hold UI for a bit, then hide if no newer drain started
        yield return new WaitForSecondsRealtime(uiHoldAfterDrain);
        if (myId == uiSessionId)
        {
            EnemyUI.SetActive(false);
            if (currentHealth <= 0f)
            {
                isDead = true;
            }
        }

        uiRoutine = null;

    }

    // --- Your existing sequence code (unchanged) ---
    public EnemyAttackSequence GetFirstSequence() => attackPattern.FirstOrDefault();
    public EnemyAttackSequence GetRandomSequence() => attackPattern.Count > 0 ? attackPattern[Random.Range(0, attackPattern.Count)] : null;

    public Coroutine RunAttackSequence(EnemyAttackSequence sequence, Transform clockCenter, float clockOffsetDeg, System.Action onComplete)
    {
        return StartCoroutine(AttackRoutine(sequence, clockCenter, clockOffsetDeg, onComplete));
    }

    private IEnumerator AttackRoutine(EnemyAttackSequence sequence, Transform clockCenter, float clockOffsetDeg, System.Action onComplete)
    {
        if (sequence == null || clockCenter == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        var list = sequence.Projectiles.OrderBy(b => b.SpawnTime).ToList();
        int i = 0;
        float t = 0f;

        while (t < sequence.Duration)
        {
            while (i < list.Count && list[i].SpawnTime <= t + 0.0001f)
            {
                SpawnBullet(list[i], clockCenter, clockOffsetDeg);
                i++;
            }
            t += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void SpawnBullet(BulletData data, Transform center, float clockOffsetDeg)
    {
        if (data == null) return;

        if (data.IsTimeSlowDown)
        {
            TimeSlowManager.Ensure().ApplyBulletScale(data.SlowMultiplier, data.SlowDuration);
            return;
        }
        if (data.IsTimeSpeedUp)
        {
            TimeSlowManager.Ensure().ApplyBulletScale(data.SpeedUpMultiplier, data.SpeedUpDuration);
            return;
        }

        if (data.IsDangerZone)
        {
            // Ask the BattleManager to run the special
            var bm = FindObjectOfType<BattleManager>(); // or serialize a reference on Enemy if you prefer
            bm?.TriggerDangerZone(data.DZPreDelay, data.DZActiveDuration, data.DZDamagePerSecond);
            return;
        }

        int idx = Mathf.Clamp(data.BulletPrefabIndex, 0, bulletPrefabs.Count - 1);
        var prefab = bulletPrefabs.Count > 0 ? bulletPrefabs[idx] : null;
        if (prefab == null) return;

        // Base travel direction from clock + angle (bullets still travel inward)
        float baseZ = center.eulerAngles.z;
        float baseAngle = baseZ + data.Angle + clockOffsetDeg;
        Vector3 dir = Quaternion.Euler(0f, 0f, baseAngle) * Vector3.right;

        // RING: unchanged
        if (data.Type == BulletData.BulletType.Ring)
        {
            var goR = Instantiate(prefab, center.position, Quaternion.identity);
            var bR = goR.GetComponent<Bullet>();
            if (bR != null) bR.Init(Vector3.right, data.Speed, data.Damage, data.Type, data.Ring);
            else goR.transform.right = Vector3.right;
            return;
        }

        // HOLD: spawn a line ALONG the travel direction (collinear with velocity)
        if (data.Type == BulletData.BulletType.Hold)
        {
            int count = data.HoldCount;            // from BulletData
            float spacing = data.HoldSpacingUnits;     // world units between bullets

            // Usual ring spawn position
            Vector3 lineCenter = center.position + dir.normalized * spawnRadius;

            // Travel direction is toward the clock center (your bullets move along -dir)
            Vector3 travelAxis = (-dir).normalized;
            float half = 0.5f * (count - 1);
            for (int i = 0; i < count; i++)
            {
                float offsetIdx = i - half;
                Vector3 spawnPos = lineCenter + travelAxis * (offsetIdx * spacing);

                var go = Instantiate(prefab, spawnPos, Quaternion.identity);
                var bullet = go.GetComponent<Bullet>();
                if (bullet != null)
                    bullet.Init(dir, data.Speed, data.Damage, data.Type, 0f); // ring scale not used for Hold
                else
                    go.transform.right = dir; // keep sprite facing travel direction setup
            }
            return;
        }


        // Others (Normal/Weak/DontHit/Click): single spawn as before
        {
            Vector3 spawnPos = center.position + dir.normalized * spawnRadius;
            var go = Instantiate(prefab, spawnPos, Quaternion.identity);
            var bullet = go.GetComponent<Bullet>();
            if (bullet != null)
                bullet.Init(dir, data.Speed, data.Damage, data.Type, 0f);
            else
                go.transform.right = dir;
        }
    }

    public EnemyAttackSequence GetNextSequence(bool random = false)
    {
        if (attackPattern == null || attackPattern.Count == 0) return null;

        if (random)
            return attackPattern[Random.Range(0, attackPattern.Count)];

        var s = attackPattern[sequenceCursor];
        sequenceCursor = (sequenceCursor + 1) % attackPattern.Count; // advance & wrap
        return s;
    }





}
