using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<EnemyAttackSequence> attackPattern = new(); // fill in Inspector
    [SerializeField] private List<GameObject> bulletPrefabs = new();          // fill in Inspector

    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 10f; // distance from center to spawn bullets

    public EnemyAttackSequence GetFirstSequence() => attackPattern.FirstOrDefault();
    public EnemyAttackSequence GetRandomSequence() => attackPattern.Count > 0 ? attackPattern[Random.Range(0, attackPattern.Count)] : null;

    public Coroutine RunAttackSequence(
        EnemyAttackSequence sequence,
        Transform clockCenter,
        float clockOffsetDeg,
        System.Action onComplete)
    {
        return StartCoroutine(AttackRoutine(sequence, clockCenter, clockOffsetDeg, onComplete));
    }

    private IEnumerator AttackRoutine(
        EnemyAttackSequence sequence,
        Transform clockCenter,
        float clockOffsetDeg,
        System.Action onComplete)
    {
        if (sequence == null || clockCenter == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Order bullets by spawn time
        var list = sequence.Projectiles.OrderBy(b => b.SpawnTime).ToList();
        int i = 0;
        float t = 0f;

        while (t < sequence.Duration)
        {
            // Spawn all bullets whose spawnTime <= t
            while (i < list.Count && list[i].SpawnTime <= t + 0.0001f)
            {
                SpawnBullet(list[i], clockCenter, clockOffsetDeg);
                i++;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // If any with spawnTime > Duration, you can decide whether to skip or wait—skipping here.
        onComplete?.Invoke();
    }

    private void SpawnBullet(BulletData data, Transform center, float clockOffsetDeg)
    {
        if (data == null) return;

        int idx = Mathf.Clamp(data.BulletPrefabIndex, 0, bulletPrefabs.Count - 1);
        var prefab = bulletPrefabs.Count > 0 ? bulletPrefabs[idx] : null;
        if (prefab == null) return;

        float baseZ = center.eulerAngles.z; // or localEulerAngles.z if needed
        float worldAngle = baseZ + data.Angle + clockOffsetDeg;

        Vector3 dir = Quaternion.Euler(0f, 0f, worldAngle) * Vector3.right;

        // spawn at a fixed distance from the clock center
        Vector3 spawnPos = center.position + dir.normalized * spawnRadius;

        // no parent — just drop it into the scene at the fixed distance
        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Init(dir, data.Speed, data.Damage, data.Type);  // pass type + damage
        else
            go.transform.right = dir; // at least face travel direction
    }
}
