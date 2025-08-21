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

    private void Start()
    {
        // Initialize UI/health bar
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 0, maxHealth);
        if (HealthBar) HealthBar.fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        if (EnemyUI) EnemyUI.SetActive(false);
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

        // Hold UI for a bit, then hide if no newer drain started
        yield return new WaitForSecondsRealtime(uiHoldAfterDrain);
        if (myId == uiSessionId)
            EnemyUI.SetActive(false);

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

        int idx = Mathf.Clamp(data.BulletPrefabIndex, 0, bulletPrefabs.Count - 1);
        var prefab = bulletPrefabs.Count > 0 ? bulletPrefabs[idx] : null;
        if (prefab == null) return;

        float baseZ = center.eulerAngles.z;
        float worldAngle = baseZ + data.Angle + clockOffsetDeg;
        Vector3 dir = Quaternion.Euler(0f, 0f, worldAngle) * Vector3.right;
        Vector3 spawnPos = center.position + dir.normalized * spawnRadius;

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Init(dir, data.Speed, data.Damage, data.Type);
        else
            go.transform.right = dir;
    }
}
