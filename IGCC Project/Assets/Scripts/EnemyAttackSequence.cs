using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackSequence", menuName = "Combat/Enemy Attack Sequence", order = 0)]
public class EnemyAttackSequence : ScriptableObject
{
    [SerializeField] private string Name;
    [SerializeField] private List<BulletData> projectiles = new();
    [SerializeField] private float AttackSequenceEnd = 2f;

    public IReadOnlyList<BulletData> Projectiles => projectiles;
    public float Duration => AttackSequenceEnd;
}
