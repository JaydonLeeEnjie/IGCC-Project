using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClockFace : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private string bulletLayerName = "Bullet";

    private int bulletLayer;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[ClockFace] Collider2D should be set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != bulletLayer) return;

        var rb = other.attachedRigidbody;
        var root = rb ? rb.gameObject : other.gameObject;

        var bullet = root.GetComponent<Bullet>();
        if (!bullet) return;

        switch (bullet.Type)
        {
            case BulletData.BulletType.Normal:
            case BulletData.BulletType.Weak:
            case BulletData.BulletType.Click:
            case BulletData.BulletType.Hold: // NEW: hitting clock hurts
                battleManager?.TakeDamage(bullet.Damage);
                Destroy(root);
                break;

            case BulletData.BulletType.DontHit:
                Destroy(root);
                break;

            case BulletData.BulletType.Ring:
                // Ring resolves in Bullet.cs by scale threshold
                break;
        }
    }
}
