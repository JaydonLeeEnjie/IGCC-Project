using System;
using UnityEngine;

[Serializable]
public class BulletData
{
    public enum BulletType { Normal, DontHit, Weak, Ring }

    [Header("Bullet")]
    [SerializeField] private BulletType bulletType = BulletType.Normal;
    [SerializeField] private int bulletPrefab;

    [Header("Timing")]
    [Min(0f)][SerializeField] private float spawnTime;
    [Min(0f)][SerializeField] private float speed;

    [Header("Stats")]
    [Range(-360f, 360f)][SerializeField] private float angle;
    [Min(0f)][SerializeField] private float damage;
    [SerializeField] private float ringTriggerScale;

    public BulletType Type => bulletType;
    public int BulletPrefabIndex => bulletPrefab;
    public float SpawnTime => spawnTime;
    public float Speed => speed;
    public float Angle => angle;
    public float Damage => damage;
    public float Ring => ringTriggerScale;
}
