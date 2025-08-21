using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private string bulletLayerName = "Bullet";
    [SerializeField] private bool requireLayerMatch = true;

    private int bulletLayer = -1;

    private void Awake()
    {

        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[PlayerHitBox] Collider2D is not set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);

    }

    private void OnTriggerEnter2D(Collider2D other) { Handle(other); }

    private void Handle(Collider2D other)
    {
        // Use the Rigidbody2D root in case the collider is on a child
        var rb = other.attachedRigidbody;
        var hitGO = rb ? rb.gameObject : other.gameObject;

        if (requireLayerMatch && bulletLayer != -1 && hitGO.layer != bulletLayer)
            return;

        if (!hitGO.TryGetComponent<Bullet>(out var bullet))
            return;

        switch (bullet.Type)
        {
            case BulletData.BulletType.Normal:
                // Normal: destroy only
                Destroy(hitGO);
                break;

            case BulletData.BulletType.DontHit:
                // NoHit: destroy AND take damage
                battleManager?.TakeDamage(bullet.Damage);
                Destroy(hitGO);
                break;
        }
    }
}
