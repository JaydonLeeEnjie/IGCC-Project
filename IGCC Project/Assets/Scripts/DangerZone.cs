using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class DangerZone : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;

    [Header("Target Filter")]
    [SerializeField] private string targetLayerName = "ClockHand";

    [Header("Damage")]
    [Min(0f)]
    [SerializeField] private float damagePerSecond = 0.01f;

    private int targetLayer = -1;
    private Collider2D _col;

    // Keep track of all target colliders currently inside
    private readonly HashSet<Collider2D> _inside = new HashSet<Collider2D>();

    // For seeding overlaps on enable (so damage starts even if nothing moves)
    private ContactFilter2D _filter;
    private readonly Collider2D[] _overlaps = new Collider2D[8];

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col && !_col.isTrigger)
            Debug.LogWarning("[DangerZone] Collider2D should be set as Trigger.");

        targetLayer = LayerMask.NameToLayer(targetLayerName);

        _filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = targetLayer >= 0 ? (1 << targetLayer) : ~0,
            useTriggers = true
        };

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // If this zone turns on while already overlapping, seed the set now
        _inside.Clear();
        if (_col != null && targetLayer != -1)
        {
            int count = _col.Overlap(_filter, _overlaps);
            for (int i = 0; i < count; i++)
                _inside.Add(_overlaps[i]);
        }
    }

    private void OnDisable()
    {
        _inside.Clear();
    }

    /// <summary>Set DPS at runtime before turning this zone on.</summary>
    public void Configure(float dps) => damagePerSecond = Mathf.Max(0f, dps);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == targetLayer)
            _inside.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == targetLayer)
            _inside.Remove(other);
    }

    private void Update()
    {
        if (_inside.Count > 0 && damagePerSecond > 0f)
        {
            battleManager?.TakeDamage(damagePerSecond * Time.deltaTime * 2);
        }
    }
}
