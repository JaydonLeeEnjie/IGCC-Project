using System;
using UnityEngine;

[Serializable]
public class BulletData
{
    public enum BulletType { Normal, DontHit, Weak, Ring, Click, Hold }

    [Header("Bullet")]
    [SerializeField] private BulletType bulletType = BulletType.Normal;
    [SerializeField] private int bulletPrefab;

    [Header("Timing")]
    [Min(0f)][SerializeField] private float spawnTime;
    [Min(0f)][SerializeField] private float speed;

    [Header("Stats")]
    [Range(-360f, 360f)][SerializeField] private float angle;
    [Min(0f)][SerializeField] private float damage;

    [Header("Special: Danger Zone")]
    [SerializeField] private bool isDangerZone = false;
    [Min(0f)][SerializeField] private float dzPreDelay = 1.0f;
    [Min(0f)][SerializeField] private float dzActiveDuration = 2.0f;
    [Min(0f)][SerializeField] private float dzDamagePerSecond = 10f;

    [Header("Special: Time Slow")]
    [SerializeField] private bool isTimeSlowDown = false;
    [Range(0f, 100f)][SerializeField] private float slowPercent = 50f;
    [Min(0f)][SerializeField] private float slowDuration = 2f;

    [Header("Special: Time Speed Up")]
    [SerializeField] private bool isTimeSpeedUp = false;
    [Range(0f, 100f)][SerializeField] private float speedUpPercent = 50f;
    [Min(0f)][SerializeField] private float speedUpDuration = 2f;

    [Header("Ring Settings")]
    [SerializeField] private float ringTriggerScale;

    [Header("Hold Line Settings")]
    [Min(1)][SerializeField] private int holdCount = 3;
    [Min(0f)][SerializeField] private float holdSpacingUnits = 1.0f;
    public BulletType Type => bulletType;
    public int BulletPrefabIndex => bulletPrefab;
    public float SpawnTime => spawnTime;
    public float Speed => speed;
    public float Angle => angle;
    public float Damage => damage;

    public bool IsDangerZone => isDangerZone;
    public float DZPreDelay => dzPreDelay;
    public float DZActiveDuration => dzActiveDuration;
    public float DZDamagePerSecond => dzDamagePerSecond;

    public bool IsTimeSlowDown => isTimeSlowDown;
    public float SlowPercent => slowPercent;
    public float SlowDuration => slowDuration;

    public bool IsTimeSpeedUp => isTimeSpeedUp;
    public float SpeedUpPercent => speedUpPercent;
    public float SpeedUpDuration => speedUpDuration;

    public float Ring => ringTriggerScale;

    // New getters
    public int HoldCount => Mathf.Max(1, holdCount);
    public float HoldSpacingUnits => Mathf.Max(0f, holdSpacingUnits);

    public float SlowMultiplier => Mathf.Clamp01(1f - (slowPercent / 100f));

    public float SpeedUpMultiplier => Mathf.Clamp(1f + (speedUpPercent / 100f), 0f, 2f);       // 0..2 (max 200%)
}
