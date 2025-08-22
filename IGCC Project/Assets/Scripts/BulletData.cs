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

    public float Ring => ringTriggerScale;

    // New getters
    public int HoldCount => Mathf.Max(1, holdCount);
    public float HoldSpacingUnits => Mathf.Max(0f, holdSpacingUnits);
}
